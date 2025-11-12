using Microsoft.CSharp.RuntimeBinder;

namespace TermuiX;

public sealed class Termui
{
    private IWidget? _widget = null;
    private readonly Renderer _renderer = new();
    private IWidget? _focusedWidget = null;
    private readonly List<IWidget> _focusableWidgets = [];

    // Event for keyboard shortcuts (Ctrl+Key combinations)
    public event EventHandler<ConsoleKeyInfo>? Shortcut;

    private Termui() { }

    public static Termui Init()
    {
        Console.Clear();
        Console.CursorVisible = false;
        return new Termui();
    }

    public static void DeInit()
    {
        Console.CursorVisible = true;
    }

    public void AddToWindow(IWidget widget)
    {
        _widget = widget;
        ValidateUniqueNames();
        RebuildFocusList();
    }

    public void LoadXml(string xml)
    {
        _widget = XmlParser.Parse(xml);
        ValidateUniqueNames();
        RebuildFocusList();
    }

    private void ValidateUniqueNames()
    {
        if (_widget is null) return;

        var names = new HashSet<string>();
        CollectNames(_widget, names);
    }

    private static void CollectNames(IWidget widget, HashSet<string> names)
    {
        if (!string.IsNullOrEmpty(widget.Name))
        {
            if (!names.Add(widget.Name))
            {
                throw new InvalidOperationException($"Duplicate widget name '{widget.Name}' found. All widget names must be unique.");
            }
        }

        foreach (var child in widget.Children)
        {
            CollectNames(child, names);
        }
    }

    public T? GetWidget<T>(string name) where T : class, IWidget
    {
        if (_widget is null) return null;
        return FindWidget<T>(_widget, name);
    }

    public List<T> GetWidgetsByGroup<T>(string group) where T : class, IWidget
    {
        var results = new List<T>();
        if (_widget is null) return results;
        FindWidgetsByGroup(_widget, group, results);
        return results;
    }

    private static T? FindWidget<T>(IWidget widget, string name) where T : class, IWidget
    {
        if (widget.Name == name && widget is T typedWidget)
        {
            return typedWidget;
        }

        foreach (var child in widget.Children)
        {
            var found = FindWidget<T>(child, name);
            if (found is not null) return found;
        }

        return null;
    }

    private static void FindWidgetsByGroup<T>(IWidget widget, string group, List<T> results) where T : class, IWidget
    {
        if (widget.Group == group && widget is T typedWidget)
        {
            results.Add(typedWidget);
        }

        foreach (var child in widget.Children)
        {
            FindWidgetsByGroup(child, group, results);
        }
    }

    private void RebuildFocusList()
    {
        _focusableWidgets.Clear();
        if (_widget is not null)
        {
            CollectFocusableWidgets(_widget, _focusableWidgets);
            if (_focusableWidgets.Count > 0)
            {
                _focusedWidget = _focusableWidgets[0];
                ((IWidget)_focusedWidget).Focussed = true;
            }
        }
    }

    private static void CollectFocusableWidgets(IWidget widget, List<IWidget> list)
    {
        // Skip invisible widgets and their children
        if (!widget.Visible) return;

        // Depth-first traversal
        if (widget.CanFocus)
        {
            list.Add(widget);
        }

        foreach (var child in widget.Children)
        {
            CollectFocusableWidgets(child, list);
        }
    }

    private static bool IsWidgetVisible(IWidget widget)
    {
        // Check if widget and all its parents are visible
        IWidget? current = widget;
        while (current is not null)
        {
            if (!current.Visible) return false;
            current = current.Parent;
        }
        return true;
    }

    public void SetFocus(IWidget widget)
    {
        // Check if widget can be focused and is visible
        if (!widget.CanFocus) return;
        if (!IsWidgetVisible(widget)) return;

        // Clear current focus
        if (_focusedWidget is not null)
        {
            _focusedWidget.Focussed = false;
        }

        // Set new focus
        _focusedWidget = widget;
        _focusedWidget.Focussed = true;
    }

    private void MoveFocus(bool forward)
    {
        // Rebuild focus list to include only currently visible widgets
        var visibleFocusableWidgets = new List<IWidget>();
        if (_widget is not null)
        {
            CollectFocusableWidgets(_widget, visibleFocusableWidgets);
        }

        if (visibleFocusableWidgets.Count == 0) return;

        // Clear current focus
        if (_focusedWidget is not null)
        {
            _focusedWidget.Focussed = false;
        }

        // Find next focus in visible widgets only
        int currentIndex = _focusedWidget is not null ? visibleFocusableWidgets.IndexOf(_focusedWidget) : -1;

        if (forward)
        {
            currentIndex = (currentIndex + 1) % visibleFocusableWidgets.Count;
        }
        else
        {
            currentIndex = currentIndex <= 0 ? visibleFocusableWidgets.Count - 1 : currentIndex - 1;
        }

        _focusedWidget = visibleFocusableWidgets[currentIndex];
        _focusedWidget.Focussed = true;

        // Auto-scroll to the focused widget if it's outside the visible area
        ScrollToWidget(_focusedWidget);
    }

    private void ScrollToWidget(IWidget widget)
    {
        // Find the nearest scrollable parent
        IWidget? scrollTarget = widget.Parent;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null) return;

        // Get scrollable parent's content dimensions
        int contentHeight = CalculateContentHeight(scrollTarget);
        int contentWidth = CalculateContentWidth(scrollTarget);

        // Widget position and size are relative to the scrollable parent
        // We need to parse them using the content dimensions
        int widgetPosY = ParseSize(widget.PositionY, contentHeight);
        int widgetHeight = ParseSize(widget.Height, contentHeight);
        int widgetPosX = ParseSize(widget.PositionX, contentWidth);
        int widgetWidth = ParseSize(widget.Width, contentWidth);

        // Vertical scrolling
        long currentScrollY = scrollTarget.ScrollOffsetY;

        // Check if widget is above visible area (need to scroll up)
        if (widgetPosY < currentScrollY)
        {
            scrollTarget.ScrollOffsetY = widgetPosY;
        }
        // Check if widget is below visible area (need to scroll down)
        else if (widgetPosY + widgetHeight > currentScrollY + contentHeight)
        {
            scrollTarget.ScrollOffsetY = widgetPosY + widgetHeight - contentHeight;
        }

        // Horizontal scrolling
        long currentScrollX = scrollTarget.ScrollOffsetX;

        // Check if widget is left of visible area (need to scroll left)
        if (widgetPosX < currentScrollX)
        {
            scrollTarget.ScrollOffsetX = widgetPosX;
        }
        // Check if widget is right of visible area (need to scroll right)
        else if (widgetPosX + widgetWidth > currentScrollX + contentWidth)
        {
            scrollTarget.ScrollOffsetX = widgetPosX + widgetWidth - contentWidth;
        }
    }

    private void HandleScrollHorizontal(bool left)
    {
        if (_focusedWidget is null) return;

        // Find the nearest scrollable widget (current or parent)
        IWidget? scrollTarget = _focusedWidget;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null || scrollTarget.Children.Count == 0) return;

        // Calculate max scroll based on children positions and widths
        int contentWidth = CalculateContentWidth(scrollTarget);
        int maxChildRight = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosX = ParseSize(child.PositionX, contentWidth);
            int childWidth = ParseSize(child.Width, contentWidth);
            maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
        }

        long maxScroll = Math.Max(0, maxChildRight - contentWidth);

        // Scroll the target widget with bounds checking
        if (left)
        {
            scrollTarget.ScrollOffsetX = Math.Max(0, scrollTarget.ScrollOffsetX - 1);
        }
        else
        {
            scrollTarget.ScrollOffsetX = Math.Min(maxScroll, scrollTarget.ScrollOffsetX + 1);
        }
    }

    private int CalculateContentWidth(IWidget widget)
    {
        // For root widget (no parent), use actual console width
        int parentWidth = 100;
        if (widget.Parent == null)
        {
            parentWidth = Console.WindowWidth;
        }

        // Parse widget width
        int width = ParseSize(widget.Width, parentWidth);

        // Subtract padding
        int padLeft = ParseSize(widget.PaddingLeft, width);
        int padRight = ParseSize(widget.PaddingRight, width);

        // If Container with border, add 1ch to padding
        if (widget is Widgets.Container container && container.HasBorder)
        {
            padLeft += 1;
            padRight += 1;
        }

        return Math.Max(0, width - padLeft - padRight);
    }

    private void HandleScroll(bool up)
    {
        if (_focusedWidget is null) return;

        // Find the nearest scrollable widget (current or parent)
        IWidget? scrollTarget = _focusedWidget;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null || scrollTarget.Children.Count == 0) return;

        // Calculate max scroll based on children positions and heights
        // We need to calculate the content height to determine max scroll
        int contentHeight = CalculateContentHeight(scrollTarget);
        int maxChildBottom = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosY = ParseSize(child.PositionY, contentHeight);
            int childHeight = ParseSize(child.Height, contentHeight);
            maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
        }

        long maxScroll = Math.Max(0, maxChildBottom - contentHeight);

        // Scroll the target widget with bounds checking
        if (up)
        {
            scrollTarget.ScrollOffsetY = Math.Max(0, scrollTarget.ScrollOffsetY - 1);
        }
        else
        {
            scrollTarget.ScrollOffsetY = Math.Min(maxScroll, scrollTarget.ScrollOffsetY + 1);
        }
    }

    private int CalculateContentHeight(IWidget widget)
    {
        // For root widget (no parent), use actual console height
        int parentHeight = 100;
        if (widget.Parent == null)
        {
            parentHeight = Console.WindowHeight;
        }

        // Parse widget height
        int height = ParseSize(widget.Height, parentHeight);

        // Subtract padding
        int padTop = ParseSize(widget.PaddingTop, height);
        int padBottom = ParseSize(widget.PaddingBottom, height);

        // If Container with border, add 1ch to padding
        if (widget is Widgets.Container container && container.HasBorder)
        {
            padTop += 1;
            padBottom += 1;
        }

        return Math.Max(0, height - padTop - padBottom);
    }

    private static int ParseSize(string size, int parentSize)
    {
        if (string.IsNullOrEmpty(size)) return 0;

        size = size.Trim();

        if (size.EndsWith("ch"))
        {
            var value = size[..^2].Trim();
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return 0;
        }
        else if (size.EndsWith('%'))
        {
            var value = size[..^1].Trim();
            if (float.TryParse(value, out float percent))
            {
                return (int)(parentSize * percent / 100.0f);
            }
            return 0;
        }

        return 0;
    }

    private void ProcessInput()
    {
        try
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                // Check for Ctrl shortcuts first (except text editing shortcuts)
                if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    // Ctrl+A, Ctrl+C, Ctrl+V are allowed to pass through to widgets (select all, copy, paste)
                    if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.C || key.Key == ConsoleKey.V)
                    {
                        _focusedWidget?.KeyPress(key);
                    }
                    // Ctrl+PageUp/PageDown for horizontal scrolling
                    else if (key.Key == ConsoleKey.PageUp)
                    {
                        HandleScrollHorizontal(true);
                    }
                    else if (key.Key == ConsoleKey.PageDown)
                    {
                        HandleScrollHorizontal(false);
                    }
                    else
                    {
                        // All other Ctrl combinations are shortcuts
                        Shortcut?.Invoke(this, key);
                    }
                }
                // Handle special keys
                else if (key.Key == ConsoleKey.Tab)
                {
                    MoveFocus(!key.Modifiers.HasFlag(ConsoleModifiers.Shift));
                }
                else if (key.Key == ConsoleKey.PageUp)
                {
                    HandleScroll(true);
                }
                else if (key.Key == ConsoleKey.PageDown)
                {
                    HandleScroll(false);
                }
                else
                {
                    // Forward to focused widget
                    _focusedWidget?.KeyPress(key);
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Console input not available (redirected or no console)
            // Silently ignore
        }
    }

    public void Render()
    {
        if (_widget is null)
        {
            throw new InvalidOperationException("No widget added to window. Call AddToWindow(widget) before Render().");
        }

        // Process input automatically
        ProcessInput();

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        // Fallback for non-interactive environments (testing)
        if (width == 0 || height == 0)
        {
            width = 80;
            height = 24;
            Console.WriteLine($"Warning: Console window size is 0. Using fallback size: {width}x{height}");
        }

        _renderer.Size(width, height);
        var (chars, fgColors, bgColors) = _renderer.Render(_widget);

        try
        {
            Console.SetCursorPosition(0, 0);
        }
        catch (IOException)
        {
            // Cursor positioning not supported in this environment
        }
        catch (ArgumentOutOfRangeException)
        {
            // Console too small for cursor positioning
        }

        // Render with colors
        for (int y = 0; y < chars.Length; y++)
        {
            try
            {
                Console.SetCursorPosition(0, y);
            }
            catch (IOException)
            {
                // Cursor positioning not supported
            }
            catch (ArgumentOutOfRangeException)
            {
                // Position out of range
            }

            // Render each character with its color
            for (int x = 0; x < chars[y].Length; x++)
            {
                Console.BackgroundColor = bgColors[y][x];
                Console.ForegroundColor = fgColors[y][x];
                Console.Write(chars[y][x]);
            }
        }

        // Reset colors
        Console.ResetColor();
    }
}

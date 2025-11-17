using System.Text;
using System.Threading;

namespace TermuiX;

/// <summary>
/// Main UI engine for TermuiX that manages widget rendering and input processing.
/// </summary>
public sealed class TermuiX
{
    private IWidget? _widget = null;
    private readonly Renderer _renderer = new();
    private IWidget? _focusedWidget = null;
    private readonly List<IWidget> _focusableWidgets = [];
    private static ConsoleCancelEventHandler? _cancelHandler = null;
    private static bool _isInitialized = false;

    /// <summary>
    /// Gets or sets a value indicating whether Ctrl+C should terminate the application.
    /// When true, Ctrl+C will call DeInit() and allow the process to exit.
    /// When false, Ctrl+C will call DeInit() but prevent process termination.
    /// Default is true.
    /// </summary>
    public static bool AllowCancelKeyExit { get; set; } = true;

    /// <summary>
    /// Event triggered when a keyboard shortcut (Ctrl+Key combination) is pressed.
    /// </summary>
    public event EventHandler<ConsoleKeyInfo>? Shortcut;

    /// <summary>
    /// Event triggered when the focused widget changes.
    /// </summary>
    public event EventHandler<IWidget>? FocusChanged;

    private TermuiX() { }

    /// <summary>
    /// Initializes a new Termui instance and prepares the console for rendering.
    /// </summary>
    /// <returns>A new initialized Termui instance.</returns>
    public static TermuiX Init()
    {
        Console.Clear();
        Console.CursorVisible = false;

        // Set up Ctrl+C handler to restore console state on exit
        _cancelHandler = new ConsoleCancelEventHandler(OnCancelKeyPress);
        Console.CancelKeyPress += _cancelHandler;

        _isInitialized = true;

        return new TermuiX();
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        // If not allowed to exit, prevent termination and don't deinitialize
        if (!AllowCancelKeyExit)
        {
            e.Cancel = true;
            return;
        }

        // Restore console state before the process terminates
        DeInit();
    }

    /// <summary>
    /// De-initializes the Termui instance and restores the console state.
    /// </summary>
    public static void DeInit()
    {
        // Mark as not initialized FIRST to stop any further rendering
        _isInitialized = false;

        // Small delay to ensure any in-flight Render() calls finish
        Thread.Sleep(50);

        // Remove Ctrl+C handler if it was set
        if (_cancelHandler != null)
        {
            Console.CancelKeyPress -= _cancelHandler;
            _cancelHandler = null;
        }

        Console.ResetColor();
        Console.Clear();
        Console.CursorVisible = true;
    }

    /// <summary>
    /// Adds a widget to the window as the root widget.
    /// </summary>
    /// <param name="widget">The widget to add as the root widget.</param>
    public void AddToWindow(IWidget widget)
    {
        _widget = widget;
        ValidateUniqueNames();
        RebuildFocusList();
    }

    /// <summary>
    /// Loads and parses an XML definition to create the UI widget tree.
    /// </summary>
    /// <param name="xml">The XML string containing the widget definition.</param>
    public void LoadXml(string xml)
    {
        _widget = XmlParser.Parse(xml);
        ValidateUniqueNames();
        RebuildFocusList();
    }

    private void ValidateUniqueNames()
    {
        if (_widget is null)
        {
            return;
        }

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

    /// <summary>
    /// Finds a widget by name in the widget tree.
    /// </summary>
    /// <typeparam name="T">The type of widget to search for.</typeparam>
    /// <param name="name">The name of the widget to find.</param>
    /// <returns>The widget if found; otherwise, null.</returns>
    public T? GetWidget<T>(string name) where T : class, IWidget
    {
        if (_widget is null)
        {
            return null;
        }

        return FindWidget<T>(_widget, name);
    }

    /// <summary>
    /// Finds all widgets in a specific group.
    /// </summary>
    /// <typeparam name="T">The type of widgets to search for.</typeparam>
    /// <param name="group">The group name to search for.</param>
    /// <returns>A list of widgets in the specified group.</returns>
    public List<T> GetWidgetsByGroup<T>(string group) where T : class, IWidget
    {
        var results = new List<T>();
        if (_widget is null)
        {
            return results;
        }

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
            if (found is not null)
            {
                return found;
            }
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
        if (!widget.Visible)
        {
            return;
        }

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
        IWidget? current = widget;
        while (current is not null)
        {
            if (!current.Visible)
            {
                return false;
            }

            current = current.Parent;
        }

        return true;
    }

    /// <summary>
    /// Sets focus to the specified widget.
    /// </summary>
    /// <param name="widget">The widget to focus.</param>
    public void SetFocus(IWidget widget)
    {
        if (!widget.CanFocus)
        {
            return;
        }

        if (!IsWidgetVisible(widget))
        {
            return;
        }

        if (_focusedWidget is not null)
        {
            _focusedWidget.Focussed = false;
        }

        _focusedWidget = widget;
        _focusedWidget.Focussed = true;

        FocusChanged?.Invoke(this, widget);

        // Auto-scroll to the focused widget if it's outside the visible area
        ScrollToWidget(_focusedWidget);
    }

    private void MoveFocus(bool forward)
    {
        // Rebuild focus list to include only currently visible widgets
        var visibleFocusableWidgets = new List<IWidget>();
        if (_widget is not null)
        {
            CollectFocusableWidgets(_widget, visibleFocusableWidgets);
        }

        if (visibleFocusableWidgets.Count == 0)
        {
            return;
        }

        if (_focusedWidget is not null)
        {
            _focusedWidget.Focussed = false;
        }

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

        FocusChanged?.Invoke(this, _focusedWidget);

        // Auto-scroll to the focused widget if it's outside the visible area
        ScrollToWidget(_focusedWidget);
    }

    private void ScrollToWidget(IWidget widget)
    {
        IWidget? scrollTarget = widget.Parent;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null)
        {
            return;
        }

        int contentHeight = CalculateContentHeight(scrollTarget);
        int contentWidth = CalculateContentWidth(scrollTarget);

        int widgetPosY = ParseSize(widget.PositionY, contentHeight);
        int widgetHeight = ParseSize(widget.Height, contentHeight);
        int widgetPosX = ParseSize(widget.PositionX, contentWidth);
        int widgetWidth = ParseSize(widget.Width, contentWidth);

        long currentScrollY = scrollTarget.ScrollOffsetY;

        if (widgetPosY < currentScrollY)
        {
            scrollTarget.ScrollOffsetY = widgetPosY;
        }
        else if (widgetPosY + widgetHeight > currentScrollY + contentHeight)
        {
            scrollTarget.ScrollOffsetY = widgetPosY + widgetHeight - contentHeight;
        }

        long currentScrollX = scrollTarget.ScrollOffsetX;

        if (widgetPosX < currentScrollX)
        {
            scrollTarget.ScrollOffsetX = widgetPosX;
        }
        else if (widgetPosX + widgetWidth > currentScrollX + contentWidth)
        {
            scrollTarget.ScrollOffsetX = widgetPosX + widgetWidth - contentWidth;
        }
    }

    private void HandleScrollHorizontal(bool left)
    {
        if (_focusedWidget is null)
        {
            return;
        }

        IWidget? scrollTarget = _focusedWidget;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null || scrollTarget.Children.Count == 0)
        {
            return;
        }

        int contentWidth = CalculateContentWidth(scrollTarget);

        // Subtract scrollbar space when calculating child sizes
        // This matches the logic in widget CalculateSize methods
        int availableWidth = contentWidth;
        if (scrollTarget.HasVerticalScrollbar)
        {
            availableWidth = Math.Max(0, availableWidth - 1);
        }

        int maxChildRight = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosX = ParseSize(child.PositionX, availableWidth);
            // Use computed size if available (set during rendering), otherwise calculate
            int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, availableWidth);
            maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
        }

        long maxScroll = Math.Max(0, maxChildRight - contentWidth);

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
        // Calculate the widget's total width
        int width = CalculateWidgetWidth(widget);

        int padLeft = ParseSize(widget.PaddingLeft, width);
        int padRight = ParseSize(widget.PaddingRight, width);

        // If widget is a Container with a border, add 1ch to all padding for the border
        if (widget is Widgets.Container container && container.HasBorder)
        {
            padLeft += 1;
            padRight += 1;
        }

        return Math.Max(0, width - padLeft - padRight);
    }

    private int CalculateWidgetWidth(IWidget widget)
    {
        if (widget.Parent is null)
        {
            return ParseSize(widget.Width, Console.WindowWidth);
        }

        // Recursively calculate parent's width first
        int parentWidth = CalculateWidgetWidth(widget.Parent);

        // Account for parent's padding
        if (widget.Parent is not null)
        {
            int parentPadLeft = ParseSize(widget.Parent.PaddingLeft, parentWidth);
            int parentPadRight = ParseSize(widget.Parent.PaddingRight, parentWidth);

            if (widget.Parent is Widgets.Container parentContainer && parentContainer.HasBorder)
            {
                parentPadLeft += 1;
                parentPadRight += 1;
            }

            parentWidth = Math.Max(0, parentWidth - parentPadLeft - parentPadRight);
        }

        return ParseSize(widget.Width, parentWidth);
    }

    private void HandleScroll(bool up)
    {
        if (_focusedWidget is null)
        {
            return;
        }

        IWidget? scrollTarget = _focusedWidget;

        while (scrollTarget is not null && !scrollTarget.Scrollable)
        {
            scrollTarget = scrollTarget.Parent;
        }

        if (scrollTarget is null || scrollTarget.Children.Count == 0)
        {
            return;
        }

        int contentHeight = CalculateContentHeight(scrollTarget);

        // Subtract scrollbar space when calculating child sizes
        // This matches the logic in widget CalculateSize methods
        int availableHeight = contentHeight;
        if (scrollTarget.HasHorizontalScrollbar)
        {
            availableHeight = Math.Max(0, availableHeight - 1);
        }

        int maxChildBottom = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosY = ParseSize(child.PositionY, availableHeight);
            // Use computed size if available (set during rendering), otherwise calculate
            int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, availableHeight);
            maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
        }

        long maxScroll = Math.Max(0, maxChildBottom - contentHeight);

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
        // Calculate the widget's total height
        int height = CalculateWidgetHeight(widget);

        int padTop = ParseSize(widget.PaddingTop, height);
        int padBottom = ParseSize(widget.PaddingBottom, height);

        // If widget is a Container with a border, add 1ch to all padding for the border
        if (widget is Widgets.Container container && container.HasBorder)
        {
            padTop += 1;
            padBottom += 1;
        }

        return Math.Max(0, height - padTop - padBottom);
    }

    private int CalculateWidgetHeight(IWidget widget)
    {
        if (widget.Parent is null)
        {
            return ParseSize(widget.Height, Console.WindowHeight);
        }

        // Recursively calculate parent's height first
        int parentHeight = CalculateWidgetHeight(widget.Parent);

        // Account for parent's padding
        if (widget.Parent is not null)
        {
            int parentPadTop = ParseSize(widget.Parent.PaddingTop, parentHeight);
            int parentPadBottom = ParseSize(widget.Parent.PaddingBottom, parentHeight);

            if (widget.Parent is Widgets.Container parentContainer && parentContainer.HasBorder)
            {
                parentPadTop += 1;
                parentPadBottom += 1;
            }

            parentHeight = Math.Max(0, parentHeight - parentPadTop - parentPadBottom);
        }

        return ParseSize(widget.Height, parentHeight);
    }

    private static int ParseSize(string size, int parentSize)
    {
        if (string.IsNullOrEmpty(size))
        {
            return 0;
        }

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

                if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.C || key.Key == ConsoleKey.V)
                    {
                        _focusedWidget?.KeyPress(key);
                    }
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
                        Shortcut?.Invoke(this, key);
                    }
                }
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
                    _focusedWidget?.KeyPress(key);
                }
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Renders the UI to the console and processes input.
    /// </summary>
    public void Render()
    {
        // Don't render if not initialized
        if (!_isInitialized)
        {
            return;
        }

        if (_widget is null)
        {
            throw new InvalidOperationException("No widget added to window. Call AddToWindow(widget) before Render().");
        }

        ProcessInput();

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

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
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        // Pre-allocate buffer outside loop to avoid stack overflow warning
        Span<char> buffer = stackalloc char[2];

        for (int y = 0; y < chars.Length; y++)
        {
            try
            {
                Console.SetCursorPosition(0, y);
            }
            catch (IOException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            int displayWidth = 0;
            for (int x = 0; x < chars[y].Length; x++)
            {
                var rune = chars[y][x];
                int runeWidth = GetRuneDisplayWidth(rune);

                // Stop if this rune would exceed the terminal width
                if (displayWidth + runeWidth > width)
                {
                    break;
                }

                Console.BackgroundColor = bgColors[y][x];
                Console.ForegroundColor = fgColors[y][x];

                // Write the Rune by encoding to UTF16
                int charsWritten = rune.EncodeToUtf16(buffer);
                if (charsWritten == 1)
                {
                    Console.Write(buffer[0]);
                }
                else if (charsWritten == 2)
                {
                    Console.Write(buffer[0]);
                    Console.Write(buffer[1]);
                }

                displayWidth += runeWidth;

                // Skip the next array position if this was a wide character
                // (the next position contains a placeholder space)
                // But don't skip if the next position contains something other than a space
                if (runeWidth == 2 && x + 1 < chars[y].Length)
                {
                    var nextRune = chars[y][x + 1];
                    // Only skip if it's a space (placeholder)
                    if (nextRune.Value == ' ')
                    {
                        x++;
                    }
                }
            }
        }

        Console.ResetColor();
    }

    private static int GetRuneDisplayWidth(Rune rune)
    {
        int value = rune.Value;

        // Emoji ranges (simplified check for common emoji blocks)
        if ((value >= 0x1F300 && value <= 0x1F9FF) || // Misc Symbols and Pictographs, Emoticons, etc.
            (value >= 0x1F600 && value <= 0x1F64F) || // Emoticons
            (value >= 0x1F680 && value <= 0x1F6FF) || // Transport and Map
            (value >= 0x1F900 && value <= 0x1F9FF))   // Supplemental Symbols
        {
            return 2;
        }

        // East Asian Wide and Fullwidth characters
        if ((value >= 0x1100 && value <= 0x115F) ||   // Hangul Jamo
            (value >= 0x2E80 && value <= 0x9FFF) ||   // CJK
            (value >= 0xAC00 && value <= 0xD7A3) ||   // Hangul Syllables
            (value >= 0xF900 && value <= 0xFAFF) ||   // CJK Compatibility Ideographs
            (value >= 0xFF00 && value <= 0xFF60) ||   // Fullwidth Forms
            (value >= 0xFFE0 && value <= 0xFFE6) ||   // Fullwidth Forms
            (value >= 0x20000 && value <= 0x2FFFD) || // CJK Extension
            (value >= 0x30000 && value <= 0x3FFFD))   // CJK Extension
        {
            return 2;
        }

        // Default: 1 cell for ASCII and most characters
        return 1;
    }
}

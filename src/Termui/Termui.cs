using Microsoft.CSharp.RuntimeBinder;

namespace Termui;

public sealed class Termui
{
    private IWidget? _widget = null;
    private readonly Renderer _renderer = new();
    private IWidget? _focusedWidget = null;
    private readonly List<IWidget> _focusableWidgets = [];

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

    private void MoveFocus(bool forward)
    {
        if (_focusableWidgets.Count == 0) return;

        // Clear current focus
        if (_focusedWidget is not null)
        {
            _focusedWidget.Focussed = false;
        }

        // Find next focus
        int currentIndex = _focusedWidget is not null ? _focusableWidgets.IndexOf(_focusedWidget) : -1;

        if (forward)
        {
            currentIndex = (currentIndex + 1) % _focusableWidgets.Count;
        }
        else
        {
            currentIndex = currentIndex <= 0 ? _focusableWidgets.Count - 1 : currentIndex - 1;
        }

        _focusedWidget = _focusableWidgets[currentIndex];
        _focusedWidget.Focussed = true;
    }

    private void HandleScroll(bool up)
    {
        if (_focusedWidget is null || !_focusedWidget.Scrollable) return;

        var widget = (IWidget)_focusedWidget;
        if (up)
        {
            widget.ScrollOffsetY = Math.Max(0, widget.ScrollOffsetY - 1);
        }
        else
        {
            widget.ScrollOffsetY++;
        }
    }

    private void ProcessInput()
    {
        try
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                // Handle special keys
                if (key.Key == ConsoleKey.Tab)
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

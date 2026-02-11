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
    private IWidget? _hoveredWidget = null;
    private readonly List<IWidget> _focusableWidgets = [];
    private static ConsoleCancelEventHandler? _cancelHandler = null;
    private static bool _isInitialized = false;
    private readonly HitTestMap _hitTestMap = new();
    private bool _mouseEnabled = true;
    private bool _focusVisible = true;
    private IWidget? _dragTarget = null;

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
    /// Includes the reason for the change (Keyboard, Click, Hover, Programmatic).
    /// </summary>
    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    /// <summary>
    /// Event triggered when a mouse click occurs anywhere on the screen.
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseClick;

    /// <summary>
    /// Gets or sets a value indicating whether mouse input is enabled.
    /// Default is true.
    /// </summary>
    public bool MouseEnabled
    {
        get => _mouseEnabled;
        set
        {
            _mouseEnabled = value;
            if (!value)
            {
                if (_hoveredWidget != null)
                {
                    _hoveredWidget.Hovered = false;
                    _hoveredWidget = null;
                }
                _focusVisible = true;
            }
        }
    }

    /// <summary>
    /// Gets the currently focused widget, or null if no widget has focus.
    /// </summary>
    public IWidget? FocusedWidget => _focusedWidget;

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

        MouseInput.Enable();

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

        MouseInput.Disable();

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

    /// <summary>
    /// Registers a custom widget type for use in XML parsing.
    /// Use this for widgets that implement IWidget directly.
    /// </summary>
    /// <param name="tagName">The XML tag name to use for this widget (case-insensitive).</param>
    /// <param name="factory">A factory function that receives XML attributes and creates new instances of the widget.</param>
    /// <example>
    /// <code>
    /// termui.RegisterWidget("CustomButton", attrs =>
    /// {
    ///     var text = attrs.GetValueOrDefault("Text", "Default");
    ///     return new CustomButton(text);
    /// });
    ///
    /// // Then use in XML:
    /// // &lt;CustomButton Text="Click Me" /&gt;
    /// </code>
    /// </example>
    public void RegisterWidget(string tagName, Func<Dictionary<string, string>, IWidget> factory)
    {
        XmlParser.RegisterWidget(tagName, factory);
    }

    /// <summary>
    /// Unregisters a custom widget type from XML parsing.
    /// </summary>
    /// <param name="tagName">The XML tag name to unregister.</param>
    public void UnregisterWidget(string tagName)
    {
        XmlParser.UnregisterWidget(tagName);
    }

    /// <summary>
    /// Registers a custom component for use in XML parsing.
    /// Use this for components that generate XML through a BuildXml() method.
    /// </summary>
    /// <param name="tagName">The XML tag name to use for this component (case-insensitive).</param>
    /// <param name="xmlFactory">A factory function that receives XML attributes and returns the XML string for this component.</param>
    /// <example>
    /// <code>
    /// var fileExplorer = new FileExplorer(termui);
    /// termui.RegisterComponent("FileExplorer", attrs =>
    /// {
    ///     var width = attrs.GetValueOrDefault("Width", "100%");
    ///     var height = attrs.GetValueOrDefault("Height", "100%");
    ///     // You can use attributes to customize the component
    ///     return fileExplorer.BuildXml();
    /// });
    /// fileExplorer.Initialize();
    ///
    /// // Then use in XML:
    /// // &lt;FileExplorer Width="80%" Height="90%" /&gt;
    /// </code>
    /// </example>
    public void RegisterComponent(string tagName, Func<Dictionary<string, string>, string> xmlFactory)
    {
        XmlParser.RegisterComponent(tagName, xmlFactory);
    }

    /// <summary>
    /// Unregisters a custom component from XML parsing.
    /// </summary>
    /// <param name="tagName">The XML tag name to unregister.</param>
    public void UnregisterComponent(string tagName)
    {
        XmlParser.UnregisterComponent(tagName);
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
        SetFocus(widget, FocusChangeReason.Programmatic);
    }

    private void SetFocus(IWidget widget, FocusChangeReason reason)
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

        FocusChanged?.Invoke(this, new FocusChangedEventArgs(widget, reason));

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

        FocusChanged?.Invoke(this, new FocusChangedEventArgs(_focusedWidget, FocusChangeReason.Keyboard));

        // Auto-scroll to the focused widget if it's outside the visible area
        ScrollToWidget(_focusedWidget);
    }

    private void ScrollToWidget(IWidget widget)
    {
        // Find nearest Y-scrollable ancestor
        IWidget? scrollTargetY = widget.Parent;
        while (scrollTargetY is not null && !scrollTargetY.ScrollY)
            scrollTargetY = scrollTargetY.Parent;

        if (scrollTargetY is not null)
        {
            int contentHeight = CalculateContentHeight(scrollTargetY);
            int widgetPosY = ParseSize(widget.PositionY, contentHeight);
            int widgetHeight = ParseSize(widget.Height, contentHeight);

            long currentScrollY = scrollTargetY.ScrollOffsetY;
            if (widgetPosY < currentScrollY)
                scrollTargetY.ScrollOffsetY = widgetPosY;
            else if (widgetPosY + widgetHeight > currentScrollY + contentHeight)
                scrollTargetY.ScrollOffsetY = widgetPosY + widgetHeight - contentHeight;
        }

        // Find nearest X-scrollable ancestor
        IWidget? scrollTargetX = widget.Parent;
        while (scrollTargetX is not null && !scrollTargetX.ScrollX)
            scrollTargetX = scrollTargetX.Parent;

        if (scrollTargetX is not null)
        {
            int contentWidth = CalculateContentWidth(scrollTargetX);
            int widgetPosX = ParseSize(widget.PositionX, contentWidth);
            int widgetWidth = ParseSize(widget.Width, contentWidth);

            long currentScrollX = scrollTargetX.ScrollOffsetX;
            if (widgetPosX < currentScrollX)
                scrollTargetX.ScrollOffsetX = widgetPosX;
            else if (widgetPosX + widgetWidth > currentScrollX + contentWidth)
                scrollTargetX.ScrollOffsetX = widgetPosX + widgetWidth - contentWidth;
        }
    }

    private void HandleScrollHorizontal(bool left)
    {
        if (_focusedWidget is null)
        {
            return;
        }

        IWidget? scrollTarget = _focusedWidget;

        while (scrollTarget is not null && !scrollTarget.ScrollX)
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

        while (scrollTarget is not null && !scrollTarget.ScrollY)
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
            // Mouse escape sequences are always active at the terminal level so that
            // stdin doesn't get polluted with raw escape bytes.  We must always drain
            // them via MouseInput.TryRead().  When _mouseEnabled is false we simply
            // discard all mouse events and only forward keyboard events.
            MouseEventArgs? lastMoved = null;

            while (true)
            {
                int result = MouseInput.TryRead(out var mouseEvent, out var keyEvent);

                if (result == 0)
                    break;

                if (result == 1)
                {
                    // Mouse disabled → drain event, don't process
                    if (!_mouseEnabled)
                        continue;

                    if (mouseEvent.EventType == MouseEventType.Moved)
                    {
                        // Coalesce: just remember the latest position
                        lastMoved = mouseEvent;
                    }
                    else
                    {
                        // Non-move event: flush any pending move first, then process
                        if (lastMoved.HasValue)
                        {
                            ProcessMouseEvent(lastMoved.Value);
                            lastMoved = null;
                        }
                        ProcessMouseEvent(mouseEvent);
                    }
                }
                else if (result == 2)
                {
                    // Key event: flush any pending move first
                    if (lastMoved.HasValue)
                    {
                        ProcessMouseEvent(lastMoved.Value);
                        lastMoved = null;
                    }
                    ProcessKeyEvent(keyEvent);
                }
            }

            // Flush final pending move
            if (_mouseEnabled && lastMoved.HasValue)
            {
                ProcessMouseEvent(lastMoved.Value);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void ProcessKeyEvent(ConsoleKeyInfo key)
    {
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
            _focusVisible = true;
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
        else if (key.Key == ConsoleKey.Escape)
        {
            Shortcut?.Invoke(this, key);
        }
        else
        {
            _focusedWidget?.KeyPress(key);
        }
    }

    private void ProcessMouseEvent(MouseEventArgs e)
    {
        MouseClick?.Invoke(this, e);

        if (e.EventType == MouseEventType.WheelUp || e.EventType == MouseEventType.WheelDown)
        {
            var scrollable = _hitTestMap.GetScrollableWidgetAt(e.X, e.Y);
            if (scrollable != null)
            {
                bool horizontal = e.Shift;

                // Auto-detect: if widget only has horizontal scrollbar (no vertical),
                // route regular wheel to horizontal scroll
                if (!horizontal && scrollable.HasHorizontalScrollbar && !scrollable.HasVerticalScrollbar)
                    horizontal = true;

                if (horizontal)
                    HandleScrollHorizontalOnWidget(scrollable, e.EventType == MouseEventType.WheelUp);
                else
                    HandleScrollOnWidget(scrollable, e.EventType == MouseEventType.WheelUp);
            }
            return;
        }

        if (e.EventType == MouseEventType.Moved)
        {
            _focusVisible = false;

            // Drag: forward move events to the widget that received the initial press
            if (_dragTarget != null)
            {
                var bounds = _hitTestMap.GetBounds(_dragTarget);
                int bx = bounds?.X ?? 0;
                int by = bounds?.Y ?? 0;
                _dragTarget.MousePress(new MouseEventArgs
                {
                    X = e.X, Y = e.Y,
                    LocalX = e.X - bx, LocalY = e.Y - by,
                    EventType = e.EventType
                });
                return;
            }

            var target = _hitTestMap.GetFocusableWidgetAt(e.X, e.Y);
            if (target != _hoveredWidget)
            {
                if (_hoveredWidget != null)
                {
                    _hoveredWidget.Hovered = false;
                }
                _hoveredWidget = target;
                if (_hoveredWidget != null)
                {
                    _hoveredWidget.Hovered = true;
                }
            }
            return;
        }

        if (e.EventType == MouseEventType.LeftButtonReleased || e.EventType == MouseEventType.RightButtonReleased)
        {
            _dragTarget = null;
        }

        if (e.EventType == MouseEventType.LeftButtonPressed || e.EventType == MouseEventType.RightButtonPressed)
        {
            var target = _hitTestMap.GetFocusableWidgetAt(e.X, e.Y);
            if (target != null)
            {
                SetFocus(target, FocusChangeReason.Click);
                _dragTarget = target;

                var bounds = _hitTestMap.GetBounds(target);
                int bx = bounds?.X ?? 0;
                int by = bounds?.Y ?? 0;
                target.MousePress(new MouseEventArgs
                {
                    X = e.X, Y = e.Y,
                    LocalX = e.X - bx, LocalY = e.Y - by,
                    EventType = e.EventType
                });
            }
        }
    }

    private void HandleScrollOnWidget(IWidget scrollTarget, bool up)
    {
        if (scrollTarget.Children.Count == 0) return;

        int contentHeight = CalculateContentHeight(scrollTarget);

        int availableHeight = contentHeight;
        if (scrollTarget.HasHorizontalScrollbar)
        {
            availableHeight = Math.Max(0, availableHeight - 1);
        }

        int maxChildBottom = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosY = ParseSize(child.PositionY, availableHeight);
            int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, availableHeight);
            maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
        }

        long maxScroll = Math.Max(0, maxChildBottom - contentHeight);

        if (up)
        {
            scrollTarget.ScrollOffsetY = Math.Max(0, scrollTarget.ScrollOffsetY - 3);
        }
        else
        {
            scrollTarget.ScrollOffsetY = Math.Min(maxScroll, scrollTarget.ScrollOffsetY + 3);
        }
    }

    private void HandleScrollHorizontalOnWidget(IWidget scrollTarget, bool left)
    {
        if (scrollTarget.Children.Count == 0) return;

        int contentWidth = CalculateContentWidth(scrollTarget);

        int availableWidth = contentWidth;
        if (scrollTarget.HasVerticalScrollbar)
        {
            availableWidth = Math.Max(0, availableWidth - 1);
        }

        int maxChildRight = 0;

        foreach (var child in scrollTarget.Children)
        {
            int childPosX = ParseSize(child.PositionX, availableWidth);
            int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, availableWidth);
            maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
        }

        long maxScroll = Math.Max(0, maxChildRight - contentWidth);

        if (left)
        {
            scrollTarget.ScrollOffsetX = Math.Max(0, scrollTarget.ScrollOffsetX - 3);
        }
        else
        {
            scrollTarget.ScrollOffsetX = Math.Min(maxScroll, scrollTarget.ScrollOffsetX + 3);
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
        var (chars, fgColors, bgColors) = _renderer.Render(_widget, _hitTestMap, _focusVisible);

        // Build entire frame into a reusable char[] buffer using ANSI escape codes.
        // Single Console.Out.Write() call per frame — no per-character I/O overhead.
        // The char[] is reused across frames (only reallocated when terminal grows).
        int requiredSize = width * height * 16; // worst case: ~16 chars per cell (escape + rune)
        if (_renderBuf.Length < requiredSize)
        {
            _renderBuf = new char[requiredSize];
        }
        int pos = 0;

        // Move to top-left (cursor is hidden for the entire TUI lifetime via Init())
        WriteAnsi(ref pos, "\x1b[H");

        Span<char> runeChars = stackalloc char[2];
        ConsoleColor prevFg = (ConsoleColor)(-1);
        ConsoleColor prevBg = (ConsoleColor)(-1);

        for (int y = 0; y < chars.Length; y++)
        {
            if (y > 0)
            {
                _renderBuf[pos++] = '\n';
            }

            int displayWidth = 0;
            for (int x = 0; x < chars[y].Length; x++)
            {
                var rune = chars[y][x];
                int runeWidth = GetRuneDisplayWidth(rune);

                if (displayWidth + runeWidth > width)
                {
                    break;
                }

                // Only emit color escape when color actually changes
                var fg = fgColors[y][x];
                var bg = bgColors[y][x];
                if (fg != prevFg || bg != prevBg)
                {
                    _renderBuf[pos++] = '\x1b';
                    _renderBuf[pos++] = '[';
                    WriteAnsiCode(ref pos, AnsiFg[(int)fg]);
                    _renderBuf[pos++] = ';';
                    WriteAnsiCode(ref pos, AnsiBg[(int)bg]);
                    _renderBuf[pos++] = 'm';
                    prevFg = fg;
                    prevBg = bg;
                }

                int charsWritten = rune.EncodeToUtf16(runeChars);
                _renderBuf[pos++] = runeChars[0];
                if (charsWritten == 2)
                {
                    _renderBuf[pos++] = runeChars[1];
                }

                displayWidth += runeWidth;

                if (runeWidth == 2 && x + 1 < chars[y].Length)
                {
                    var nextRune = chars[y][x + 1];
                    if (nextRune.Value == ' ')
                    {
                        x++;
                    }
                }
            }
        }

        // Reset colors, move cursor to top-left (cursor stays hidden during TUI runtime)
        WriteAnsi(ref pos, "\x1b[0m\x1b[H");

        // Single write for entire frame — zero allocation
        Console.Out.Write(_renderBuf, 0, pos);
    }

    // Reusable frame buffer — only grows, never shrinks. Zero allocation per frame.
    private char[] _renderBuf = new char[32768];

    private void WriteAnsi(ref int pos, string s)
    {
        for (int i = 0; i < s.Length; i++)
            _renderBuf[pos++] = s[i];
    }

    private void WriteAnsiCode(ref int pos, string code)
    {
        for (int i = 0; i < code.Length; i++)
            _renderBuf[pos++] = code[i];
    }

    // Pre-computed ANSI SGR codes for all 16 ConsoleColors × fg/bg.
    // Index = (int)ConsoleColor. Avoids any allocation at render time.
    private static readonly string[] AnsiFg =
    [
        "30",  // Black
        "34",  // DarkBlue
        "32",  // DarkGreen
        "36",  // DarkCyan
        "31",  // DarkRed
        "35",  // DarkMagenta
        "33",  // DarkYellow
        "37",  // Gray
        "90",  // DarkGray
        "94",  // Blue
        "92",  // Green
        "96",  // Cyan
        "91",  // Red
        "95",  // Magenta
        "93",  // Yellow
        "97",  // White
    ];

    private static readonly string[] AnsiBg =
    [
        "40",   // Black
        "44",   // DarkBlue
        "42",   // DarkGreen
        "46",   // DarkCyan
        "41",   // DarkRed
        "45",   // DarkMagenta
        "43",   // DarkYellow
        "47",   // Gray
        "100",  // DarkGray
        "104",  // Blue
        "102",  // Green
        "106",  // Cyan
        "101",  // Red
        "105",  // Magenta
        "103",  // Yellow
        "107",  // White
    ];

    private static int GetRuneDisplayWidth(Rune rune)
    {
        int value = rune.Value;

        // Control characters
        if (value == 0 || Rune.IsControl(rune))
            return 0;

        // Explicit zero-width characters
        if (value == 0x200B ||                          // Zero-Width Space
            value == 0x200C ||                          // Zero-Width Non-Joiner
            value == 0x200D ||                          // Zero-Width Joiner
            value == 0x2060 ||                          // Word Joiner
            value == 0xFEFF ||                          // Zero-Width No-Break Space (BOM)
            (value >= 0xFE00 && value <= 0xFE0F) ||     // Variation Selectors
            (value >= 0xE0100 && value <= 0xE01EF) ||   // Variation Selectors Supplement
            (value >= 0xE0000 && value <= 0xE007F))     // Tags Block
        {
            return 0;
        }

        // Hangul Jungseong/Jongseong (combining vowels/finals in Korean syllables)
        if ((value >= 0x1160 && value <= 0x11FF) ||
            (value >= 0xD7B0 && value <= 0xD7C6) ||
            (value >= 0xD7CB && value <= 0xD7FB))
        {
            return 0;
        }

        // Combining marks and format characters
        var runeCategory = Rune.GetUnicodeCategory(rune);
        if (runeCategory == System.Globalization.UnicodeCategory.NonSpacingMark ||
            runeCategory == System.Globalization.UnicodeCategory.EnclosingMark ||
            runeCategory == System.Globalization.UnicodeCategory.Format)
        {
            return 0;
        }

        if ((value >= 0x1100 && value <= 0x115F) ||
            (value >= 0x231A && value <= 0x231B) ||
            (value >= 0x2329 && value <= 0x232A) ||
            (value >= 0x23E9 && value <= 0x23EC) ||
            value == 0x23F0 || value == 0x23F3 ||
            (value >= 0x25FD && value <= 0x25FE) ||
            (value >= 0x2614 && value <= 0x2615) ||
            (value >= 0x2648 && value <= 0x2653) ||
            value == 0x267F || value == 0x2693 || value == 0x26A1 ||
            (value >= 0x26AA && value <= 0x26AB) ||
            (value >= 0x26BD && value <= 0x26BE) ||
            (value >= 0x26C4 && value <= 0x26C5) ||
            value == 0x26CE || value == 0x26D4 || value == 0x26EA ||
            (value >= 0x26F2 && value <= 0x26F3) ||
            value == 0x26F5 || value == 0x26FA || value == 0x26FD ||
            value == 0x2705 ||
            (value >= 0x270A && value <= 0x270B) ||
            value == 0x2728 || value == 0x274C || value == 0x274E ||
            (value >= 0x2753 && value <= 0x2755) ||
            value == 0x2757 ||
            (value >= 0x2795 && value <= 0x2797) ||
            value == 0x27B0 || value == 0x27BF ||
            (value >= 0x2B1B && value <= 0x2B1C) ||
            value == 0x2B50 || value == 0x2B55 ||
            (value >= 0x2E80 && value <= 0x9FFF) ||
            (value >= 0xA000 && value <= 0xA4C6) ||
            (value >= 0xA960 && value <= 0xA97C) ||
            (value >= 0xAC00 && value <= 0xD7A3) ||
            (value >= 0xF900 && value <= 0xFAFF) ||
            (value >= 0xFE10 && value <= 0xFE19) ||
            (value >= 0xFE30 && value <= 0xFE6B) ||
            (value >= 0xFF00 && value <= 0xFF60) ||
            (value >= 0xFFE0 && value <= 0xFFE6))
        {
            return 2;
        }

        if (value == 0x1F004 || value == 0x1F0CF || value == 0x1F18E ||
            (value >= 0x1F191 && value <= 0x1F19A) ||
            (value >= 0x1F200 && value <= 0x1F202) ||
            (value >= 0x1F210 && value <= 0x1F23B) ||
            (value >= 0x1F240 && value <= 0x1F248) ||
            (value >= 0x1F250 && value <= 0x1F251) ||
            (value >= 0x1F260 && value <= 0x1F265) ||
            (value >= 0x1F300 && value <= 0x1F320) ||
            (value >= 0x1F32D && value <= 0x1F335) ||
            (value >= 0x1F337 && value <= 0x1F37C) ||
            (value >= 0x1F37E && value <= 0x1F393) ||
            (value >= 0x1F3A0 && value <= 0x1F3CA) ||
            (value >= 0x1F3CF && value <= 0x1F3D3) ||
            (value >= 0x1F3E0 && value <= 0x1F3F0) ||
            value == 0x1F3F4 ||
            (value >= 0x1F3F8 && value <= 0x1F43E) ||
            value == 0x1F440 ||
            (value >= 0x1F442 && value <= 0x1F4FC) ||
            (value >= 0x1F4FF && value <= 0x1F53D) ||
            (value >= 0x1F54B && value <= 0x1F54E) ||
            (value >= 0x1F550 && value <= 0x1F567) ||
            value == 0x1F57A ||
            (value >= 0x1F595 && value <= 0x1F596) ||
            value == 0x1F5A4 ||
            (value >= 0x1F5FB && value <= 0x1F64F) ||
            (value >= 0x1F680 && value <= 0x1F6C5) ||
            value == 0x1F6CC ||
            (value >= 0x1F6D0 && value <= 0x1F6D2) ||
            (value >= 0x1F6D5 && value <= 0x1F6D7) ||
            (value >= 0x1F6DC && value <= 0x1F6DF) ||
            (value >= 0x1F6EB && value <= 0x1F6EC) ||
            (value >= 0x1F6F4 && value <= 0x1F6FC) ||
            (value >= 0x1F7E0 && value <= 0x1F7EB) ||
            value == 0x1F7F0 ||
            (value >= 0x1F90C && value <= 0x1F93A) ||
            (value >= 0x1F93C && value <= 0x1F945) ||
            (value >= 0x1F947 && value <= 0x1F9FF) ||
            (value >= 0x1FA70 && value <= 0x1FA7C) ||
            (value >= 0x1FA80 && value <= 0x1FA88) ||
            (value >= 0x1FA90 && value <= 0x1FABD) ||
            (value >= 0x1FABF && value <= 0x1FAC5) ||
            (value >= 0x1FACE && value <= 0x1FADB) ||
            (value >= 0x1FAE0 && value <= 0x1FAE8) ||
            (value >= 0x1FAF0 && value <= 0x1FAF8))
        {
            return 2;
        }

        if ((value >= 0x16FE0 && value <= 0x16FE3) ||
            (value >= 0x16FF0 && value <= 0x16FF1) ||
            (value >= 0x17000 && value <= 0x187F7) ||
            (value >= 0x18800 && value <= 0x18CD5) ||
            (value >= 0x18D00 && value <= 0x18D08) ||
            (value >= 0x1AFF0 && value <= 0x1AFF3) ||
            (value >= 0x1AFF5 && value <= 0x1AFFB) ||
            (value >= 0x1AFFD && value <= 0x1AFFE) ||
            (value >= 0x1B000 && value <= 0x1B122) ||
            value == 0x1B132 ||
            (value >= 0x1B150 && value <= 0x1B152) ||
            value == 0x1B155 ||
            (value >= 0x1B164 && value <= 0x1B167) ||
            (value >= 0x1B170 && value <= 0x1B2FB) ||
            (value >= 0x20000 && value <= 0x2FA1D) ||
            (value >= 0x30000 && value <= 0x3FFFD))
        {
            return 2;
        }

        return 1;
    }
}

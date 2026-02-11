namespace TermuiX;

/// <summary>
/// Maps screen coordinates to widgets for mouse hit-testing.
/// Rebuilt each frame during rendering.
/// </summary>
internal class HitTestMap
{
    private List<IWidget>?[][]? _map;
    private int _width;
    private int _height;

    /// <summary>
    /// Initializes or resets the map for the current frame.
    /// </summary>
    internal void Reset(int width, int height)
    {
        if (_map == null || _width != width || _height != height)
        {
            _map = new List<IWidget>?[height][];
            for (int y = 0; y < height; y++)
            {
                _map[y] = new List<IWidget>?[width];
            }
            _width = width;
            _height = height;
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _map[y][x]?.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Records that the given screen rectangle belongs to the specified widget.
    /// Widgets are appended in render order (parents before children).
    /// </summary>
    internal void Set(int x, int y, int width, int height, IWidget widget,
                      int clipX, int clipY, int clipWidth, int clipHeight)
    {
        if (_map == null) return;

        int startX = Math.Max(x, clipX);
        int startY = Math.Max(y, clipY);
        int endX = Math.Min(x + width, clipX + clipWidth);
        int endY = Math.Min(y + height, clipY + clipHeight);

        for (int row = startY; row < endY && row < _height; row++)
        {
            for (int col = startX; col < endX && col < _width; col++)
            {
                if (col >= 0 && row >= 0)
                {
                    _map[row][col] ??= new(4);
                    _map[row][col]!.Add(widget);
                }
            }
        }
    }

    /// <summary>
    /// Returns the topmost (innermost) widget at the given screen coordinates, or null.
    /// </summary>
    internal IWidget? GetWidgetAt(int x, int y)
    {
        if (_map == null || x < 0 || y < 0 || y >= _height || x >= _width)
            return null;

        var list = _map[y][x];
        if (list == null || list.Count == 0)
            return null;

        return list[^1];
    }

    /// <summary>
    /// Finds the nearest focusable widget at the given coordinates.
    /// Searches from innermost to outermost in the render stack,
    /// then walks the Parent chain for each.
    /// This handles composite widgets (like Button) where the internal
    /// children's Parent chain may not include the owning widget.
    /// </summary>
    internal IWidget? GetFocusableWidgetAt(int x, int y)
    {
        if (_map == null || x < 0 || y < 0 || y >= _height || x >= _width)
            return null;

        var list = _map[y][x];
        if (list == null || list.Count == 0)
            return null;

        // Walk from innermost to outermost in the render stack.
        // First, try the Parent chain from the innermost widget.
        // If that fails, check each widget in the stack directly.
        var innermost = list[^1];
        IWidget? target = innermost;
        while (target != null)
        {
            if (target.CanFocus)
                return target;
            target = target.Parent;
        }

        // Parent chain didn't find a focusable widget.
        // This can happen with composite widgets (e.g. Button) where internal
        // Container.Parent is set to Button.Parent for sizing, skipping Button itself.
        // Search the render stack directly (outermost widgets rendered first).
        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i].CanFocus)
                return list[i];
        }

        return null;
    }

    /// <summary>
    /// Finds the nearest scrollable ancestor for the widget at the given coordinates.
    /// </summary>
    internal IWidget? GetScrollableWidgetAt(int x, int y)
    {
        var widget = GetWidgetAt(x, y);
        while (widget != null)
        {
            if (widget.Scrollable)
                return widget;
            widget = widget.Parent;
        }
        return null;
    }
}

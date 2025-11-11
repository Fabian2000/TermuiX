namespace Termui.Widgets;

public class Container : IWidget
{
    private readonly List<IWidget> _children = [];

    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "100%";
    public string Height { get; set; } = "100%";
    public string PaddingLeft { get; set; } = "0ch";
    public string PaddingTop { get; set; } = "0ch";
    public string PaddingRight { get; set; } = "0ch";
    public string PaddingBottom { get; set; } = "0ch";
    public string PositionX { get; set; } = "0ch";
    public string PositionY { get; set; } = "0ch";
    public bool Visible { get; set; } = true;
    public bool AllowWrapping { get; set; } = false;

    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    public bool CanFocus => false;
    public bool Scrollable { get; set; } = false;

    // Border support
    public BorderStyle? BorderStyle { get; set; } = null;
    public bool HasBorder => BorderStyle.HasValue;

    // Explicit interface implementation to hide these members
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => _children;
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    // Public API for adding children
    public void Add(IWidget widget)
    {
        widget.Parent = this;
        _children.Add(widget);
    }

    public void Remove(IWidget widget)
    {
        widget.Parent = null;
        _children.Remove(widget);
    }

    public void Clear()
    {
        _children.Clear();
    }

    char[][] IWidget.GetRaw()
    {
        // Calculate actual widget size based on parent
        int actualWidth = CalculateSize(Width, ((IWidget)this).Parent?.Width, true);
        int actualHeight = CalculateSize(Height, ((IWidget)this).Parent?.Height, false);

        // If we can't determine size, return empty
        if (actualWidth <= 0 || actualHeight <= 0)
        {
            return [];
        }

        // Always create result filled with spaces - like a JPEG, not a PNG!
        var result = new char[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new char[actualWidth];
            Array.Fill(result[i], ' ');
        }

        // Draw border on top of spaces if we have one
        if (HasBorder)
        {
            var (topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical) = GetBorderChars();

            // Top border
            result[0][0] = topLeft;
            result[0][actualWidth - 1] = topRight;
            for (int x = 1; x < actualWidth - 1; x++)
            {
                result[0][x] = horizontal;
            }

            // Bottom border
            int lastY = actualHeight - 1;
            result[lastY][0] = bottomLeft;
            result[lastY][actualWidth - 1] = bottomRight;
            for (int x = 1; x < actualWidth - 1; x++)
            {
                result[lastY][x] = horizontal;
            }

            // Left and right borders
            for (int y = 1; y < actualHeight - 1; y++)
            {
                result[y][0] = vertical;
                result[y][actualWidth - 1] = vertical;
            }
        }

        return result;
    }

    private int CalculateSize(string size, string? parentSize, bool isWidth)
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
            // Need to resolve parent size first
            if (string.IsNullOrEmpty(parentSize)) return 0;

            var parent = ((IWidget)this).Parent;
            int parentSizeValue = CalculateSize(parentSize, parent?.Parent?.Width, isWidth);

            // Subtract parent's padding since children render in content area
            if (parent is not null)
            {
                if (isWidth)
                {
                    int padLeft = ParsePadding(parent.PaddingLeft);
                    int padRight = ParsePadding(parent.PaddingRight);
                    parentSizeValue = Math.Max(0, parentSizeValue - padLeft - padRight);
                }
                else
                {
                    int padTop = ParsePadding(parent.PaddingTop);
                    int padBottom = ParsePadding(parent.PaddingBottom);
                    parentSizeValue = Math.Max(0, parentSizeValue - padTop - padBottom);
                }
            }

            var value = size[..^1].Trim();
            if (float.TryParse(value, out float percent))
            {
                return (int)(parentSizeValue * percent / 100.0f);
            }
            return 0;
        }

        return 0;
    }

    private static int ParsePadding(string padding)
    {
        if (string.IsNullOrEmpty(padding)) return 0;

        padding = padding.Trim();
        if (padding.EndsWith("ch"))
        {
            var value = padding[..^2].Trim();
            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }

        return 0;
    }

    private (char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical) GetBorderChars()
    {
        return BorderStyle switch
        {
            Widgets.BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            Widgets.BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            _ => ('┌', '┐', '└', '┘', '─', '│')
        };
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Container doesn't handle key presses directly
    }
}

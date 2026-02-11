using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A line widget that renders horizontal or vertical lines with different styles.
/// </summary>
public class Line : IWidget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> class.
    /// </summary>
    public Line() { }

    private string _width = "100%";
    private string _height = "1ch";

    /// <summary>
    /// Gets or sets the orientation of the line.
    /// </summary>
    public LineOrientation Orientation { get; set; } = LineOrientation.Horizontal;

    /// <summary>
    /// Gets or sets the visual style of the line.
    /// </summary>
    public LineType Type { get; set; } = LineType.Solid;

    /// <summary>
    /// Gets or sets the unique name of the line widget.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the line widget.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the line widget.
    /// For vertical lines, this always returns "1ch" regardless of the set value.
    /// </summary>
    public string Width
    {
        get => Orientation == LineOrientation.Vertical ? "1ch" : _width;
        set => _width = value;
    }

    /// <summary>
    /// Gets or sets the height of the line widget.
    /// For horizontal lines, this always returns "1ch" regardless of the set value.
    /// </summary>
    public string Height
    {
        get => Orientation == LineOrientation.Horizontal ? "1ch" : _height;
        set => _height = value;
    }

    /// <summary>
    /// Gets or sets the left padding.
    /// </summary>
    public string PaddingLeft { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the top padding.
    /// </summary>
    public string PaddingTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right padding.
    /// </summary>
    public string PaddingRight { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the bottom padding.
    /// </summary>
    public string PaddingBottom { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the left margin.
    /// </summary>
    public string MarginLeft { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the top margin.
    /// </summary>
    public string MarginTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right margin.
    /// </summary>
    public string MarginRight { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the bottom margin.
    /// </summary>
    public string MarginBottom { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the X position.
    /// </summary>
    public string PositionX { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the Y position.
    /// </summary>
    public string PositionY { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets a value indicating whether the widget is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets a value indicating whether the line widget can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets a value indicating whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

    // Explicit interface implementation to hide these members
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    bool IWidget.Hovered { get; set; }
    bool IWidget.AllowWrapping { get; set; } = false;
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }
    bool IWidget.Disabled { get; set; }
    ConsoleColor? IWidget.DisabledBackgroundColor { get; set; }
    ConsoleColor IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    Rune[][] IWidget.GetRaw()
    {
        int actualWidth;
        int actualHeight;

        if (Orientation == LineOrientation.Horizontal)
        {
            // Horizontal line: use specified width, but force height to 1
            actualWidth = CalculateSize(Width, ((IWidget)this).Parent, true);
            actualHeight = 1;
        }
        else // Vertical
        {
            // Vertical line: force width to 1, use specified height
            actualWidth = 1;
            actualHeight = CalculateSize(Height, ((IWidget)this).Parent, false);
        }

        // Store computed values
        ((IWidget)this).ComputedWidth = actualWidth;
        ((IWidget)this).ComputedHeight = actualHeight;

        if (actualWidth <= 0 || actualHeight <= 0)
        {
            return [];
        }

        var result = new Rune[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new Rune[actualWidth];
            Array.Fill(result[i], new Rune(' '));
        }

        Rune lineChar = GetLineCharacter();

        if (Orientation == LineOrientation.Horizontal)
        {
            // Fill all rows with the horizontal line character
            for (int y = 0; y < actualHeight; y++)
            {
                for (int x = 0; x < actualWidth; x++)
                {
                    result[y][x] = lineChar;
                }
            }
        }
        else // Vertical
        {
            // Fill all columns with the vertical line character
            for (int y = 0; y < actualHeight; y++)
            {
                for (int x = 0; x < actualWidth; x++)
                {
                    result[y][x] = lineChar;
                }
            }
        }

        return result;
    }

    private Rune GetLineCharacter()
    {
        return (Orientation, Type) switch
        {
            (LineOrientation.Horizontal, LineType.Solid) => new Rune('─'),
            (LineOrientation.Horizontal, LineType.Double) => new Rune('═'),
            (LineOrientation.Horizontal, LineType.Thick) => new Rune('━'),
            (LineOrientation.Horizontal, LineType.Dotted) => new Rune('┈'),
            (LineOrientation.Vertical, LineType.Solid) => new Rune('│'),
            (LineOrientation.Vertical, LineType.Double) => new Rune('║'),
            (LineOrientation.Vertical, LineType.Thick) => new Rune('┃'),
            (LineOrientation.Vertical, LineType.Dotted) => new Rune('┊'),
            _ => new Rune('─')
        };
    }

    private int CalculateSize(string size, IWidget? parent, bool isWidth)
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
            int parentSizeValue;

            if (parent == null)
            {
                // No parent - use console dimensions
                parentSizeValue = isWidth ? Console.WindowWidth : Console.WindowHeight;
            }
            else
            {
                // Use parent's computed size if available, otherwise fall back to console dimensions
                parentSizeValue = isWidth ?
                    (parent.ComputedWidth > 0 ? parent.ComputedWidth : Console.WindowWidth) :
                    (parent.ComputedHeight > 0 ? parent.ComputedHeight : Console.WindowHeight);

                // Subtract padding from parent's available space
                if (isWidth)
                {
                    int padLeft = ParsePadding(parent.PaddingLeft);
                    int padRight = ParsePadding(parent.PaddingRight);
                    parentSizeValue = Math.Max(0, parentSizeValue - padLeft - padRight);

                    // Subtract 1ch for vertical scrollbar if it was rendered in the previous frame
                    if (parent.HasVerticalScrollbar)
                    {
                        parentSizeValue = Math.Max(0, parentSizeValue - 1);
                    }
                }
                else
                {
                    int padTop = ParsePadding(parent.PaddingTop);
                    int padBottom = ParsePadding(parent.PaddingBottom);
                    parentSizeValue = Math.Max(0, parentSizeValue - padTop - padBottom);

                    // Subtract 1ch for horizontal scrollbar if it was rendered in the previous frame
                    if (parent.HasHorizontalScrollbar)
                    {
                        parentSizeValue = Math.Max(0, parentSizeValue - 1);
                    }
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
        if (string.IsNullOrEmpty(padding))
        {
            return 0;
        }

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

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }

    /// <summary>
    /// Creates a copy of this line widget.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for Line).</param>
    /// <returns>A new Line instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Line
        {
            Orientation = Orientation,
            Type = Type,
            Name = Name,
            Group = Group,
            _width = _width,
            _height = _height,
            PaddingLeft = PaddingLeft,
            PaddingTop = PaddingTop,
            PaddingRight = PaddingRight,
            PaddingBottom = PaddingBottom,
            MarginLeft = MarginLeft,
            MarginTop = MarginTop,
            MarginRight = MarginRight,
            MarginBottom = MarginBottom,
            PositionX = PositionX,
            PositionY = PositionY,
            Visible = Visible,
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor
        };

        return clone;
    }
}

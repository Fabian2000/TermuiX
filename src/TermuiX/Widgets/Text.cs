namespace TermuiX.Widgets;

/// <summary>
/// A text display widget that renders static or dynamic text content.
/// </summary>
public class Text : IWidget
{
    private string _text = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class.
    /// </summary>
    public Text() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class with the specified text.
    /// </summary>
    /// <param name="text">The initial text content.</param>
    public Text(string text)
    {
        _text = text;
    }

    /// <summary>
    /// Gets or sets the text content to display.
    /// </summary>
    public string Content
    {
        get => _text;
        set => _text = value;
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the text.
    /// </summary>
    public TextAlign TextAlign { get; set; } = TextAlign.Left;

    /// <summary>
    /// Gets or sets the unique name of the text widget.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the text widget.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the text widget.
    /// </summary>
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Gets or sets the height of the text widget.
    /// </summary>
    public string Height { get; set; } = "100%";

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
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = true;

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
    /// Gets a value indicating whether the text widget can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether the text widget is scrollable.
    /// </summary>
    public bool Scrollable => false;

    // Explicit interface implementation to hide these members
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    char[][] IWidget.GetRaw()
    {
        int actualWidth = CalculateSize(Width, ((IWidget)this).Parent?.Width, true);
        int actualHeight = CalculateSize(Height, ((IWidget)this).Parent?.Height, false);

        if (actualWidth <= 0 || actualHeight <= 0)
        {
            return [];
        }

        var result = new char[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new char[actualWidth];
            Array.Fill(result[i], ' ');
        }

        if (string.IsNullOrEmpty(_text))
        {
            return result;
        }

        var lines = _text.Split('\n');

        for (int i = 0; i < lines.Length && i < actualHeight; i++)
        {
            string line = lines[i];

            if (line.Length > actualWidth)
            {
                line = line[..actualWidth];
            }

            int offset = TextAlign switch
            {
                TextAlign.Center => Math.Max(0, (actualWidth - line.Length) / 2),
                TextAlign.Right => Math.Max(0, actualWidth - line.Length),
                _ => 0
            };

            for (int j = 0; j < line.Length; j++)
            {
                result[i][offset + j] = line[j];
            }
        }

        return result;
    }

    private int CalculateSize(string size, string? parentSize, bool isWidth)
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
            if (string.IsNullOrEmpty(parentSize))
            {
                return 0;
            }

            var parent = ((IWidget)this).Parent;
            int parentSizeValue = CalculateSize(parentSize, parent?.Parent?.Width, isWidth);

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
}

namespace TermuiX.Widgets;

public class Text : IWidget
{
    private string _text = string.Empty;

    public Text() { }

    public Text(string text)
    {
        _text = text;
    }

    public string Content
    {
        get => _text;
        set => _text = value;
    }

    public TextAlign TextAlign { get; set; } = TextAlign.Left;

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
    public bool AllowWrapping { get; set; } = true;

    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    public bool CanFocus => false;
    public bool Scrollable => false;

    // Explicit interface implementation to hide these members
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    char[][] IWidget.GetRaw()
    {
        // Calculate actual widget size based on parent
        int actualWidth = CalculateSize(Width, ((IWidget)this).Parent?.Width, true);
        int actualHeight = CalculateSize(Height, ((IWidget)this).Parent?.Height, false);

        // If we can't determine size or text is empty, return spaces
        if (actualWidth <= 0 || actualHeight <= 0)
        {
            return [];
        }

        // Create result filled with spaces - like a JPEG!
        var result = new char[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new char[actualWidth];
            Array.Fill(result[i], ' ');
        }

        // If no text, just return spaces
        if (string.IsNullOrEmpty(_text))
        {
            return result;
        }

        // Split text into lines
        var lines = _text.Split('\n');

        for (int i = 0; i < lines.Length && i < actualHeight; i++)
        {
            string line = lines[i];

            // Truncate line if too long
            if (line.Length > actualWidth)
            {
                line = line[..actualWidth];
            }

            // Calculate offset based on alignment
            int offset = TextAlign switch
            {
                TextAlign.Center => Math.Max(0, (actualWidth - line.Length) / 2),
                TextAlign.Right => Math.Max(0, actualWidth - line.Length),
                _ => 0 // Left
            };

            // Copy line into result
            for (int j = 0; j < line.Length; j++)
            {
                result[i][offset + j] = line[j];
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

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Text doesn't handle key presses
    }
}

using System.Text;

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
    /// Gets or sets the visual style of the text.
    /// </summary>
    public TextStyle Style { get; set; } = TextStyle.Normal;

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
        // If text is empty and using default width/height (100%), set computed size to 0
        if (string.IsNullOrEmpty(_text) && Width == "100%" && Height == "100%")
        {
            ((IWidget)this).ComputedWidth = 0;
            ((IWidget)this).ComputedHeight = 0;
            return [];
        }

        int actualWidth = CalculateSize(Width, ((IWidget)this).Parent, true);
        int actualHeight = CalculateSize(Height, ((IWidget)this).Parent, false);

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

        if (string.IsNullOrEmpty(_text))
        {
            return result;
        }

        // Apply text styling using Unicode transformation
        var lines = _text.Split('\n');

        for (int i = 0; i < lines.Length && i < actualHeight; i++)
        {
            string line = lines[i];

            // Convert line to Runes with styling and calculate display width
            var displayRunes = new List<Rune>();
            int totalDisplayWidth = 0;

            foreach (Rune rune in line.EnumerateRunes())
            {
                Rune styledRune = ApplyTextStyleToRune(rune, Style);
                int runeWidth = GetRuneDisplayWidth(styledRune);

                // Check if adding this rune would exceed the width
                if (totalDisplayWidth + runeWidth > actualWidth)
                {
                    break;
                }

                displayRunes.Add(styledRune);
                totalDisplayWidth += runeWidth;
            }

            int offset = TextAlign switch
            {
                TextAlign.Center => Math.Max(0, (actualWidth - totalDisplayWidth) / 2),
                TextAlign.Right => Math.Max(0, actualWidth - totalDisplayWidth),
                _ => 0
            };

            int currentX = offset;
            for (int j = 0; j < displayRunes.Count && currentX < actualWidth; j++)
            {
                Rune rune = displayRunes[j];
                result[i][currentX] = rune;

                int runeWidth = GetRuneDisplayWidth(rune);
                currentX += runeWidth;

                // Fill additional cells for wide characters (emojis)
                for (int k = 1; k < runeWidth && currentX < actualWidth; k++)
                {
                    result[i][currentX] = new Rune(' ');
                    currentX++;
                }
            }
        }

        return result;
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

    private static int GetRuneDisplayWidth(Rune rune)
    {
        // Most emojis and East Asian characters take 2 cells
        // ASCII and most Latin characters take 1 cell
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

    private static Rune ApplyTextStyleToRune(Rune rune, TextStyle style)
    {
        if (style == TextStyle.Normal)
        {
            return rune;
        }

        // Only apply styling to ASCII characters that can be converted
        // For emojis and other complex unicode, return as-is
        if (!rune.IsAscii)
        {
            return rune;
        }

        int value = rune.Value;
        if (value > 127)
        {
            return rune;
        }

        char c = (char)value;

        // Transform based on style
        if (style == TextStyle.Bold)
        {
            return ConvertToBold(c);
        }
        else if (style == TextStyle.Italic)
        {
            return ConvertToItalic(c);
        }
        else if (style == TextStyle.BoldItalic)
        {
            return ConvertToBoldItalic(c);
        }
        else if (style == TextStyle.Underline || style == TextStyle.Strikethrough)
        {
            // For underline and strikethrough, we use the base character
            // Note: Combining characters are not well-supported in Rune rendering
            // so we'll just return the base character
            return rune;
        }
        else
        {
            return rune;
        }
    }

    private static Rune ConvertToBold(char c)
    {
        // Unicode Mathematical Alphanumeric Symbols - Bold
        if (c >= 'A' && c <= 'Z')
        {
            int codePoint = 0x1D400 + (c - 'A');
            return new Rune(codePoint);
        }
        if (c >= 'a' && c <= 'z')
        {
            int codePoint = 0x1D41A + (c - 'a');
            return new Rune(codePoint);
        }
        if (c >= '0' && c <= '9')
        {
            int codePoint = 0x1D7CE + (c - '0');
            return new Rune(codePoint);
        }

        return new Rune(c);
    }

    private static Rune ConvertToItalic(char c)
    {
        // Unicode Mathematical Alphanumeric Symbols - Italic
        if (c >= 'A' && c <= 'Z')
        {
            int codePoint = 0x1D434 + (c - 'A');
            return new Rune(codePoint);
        }
        if (c >= 'a' && c <= 'z')
        {
            int codePoint = 0x1D44E + (c - 'a');
            return new Rune(codePoint);
        }

        return new Rune(c);
    }

    private static Rune ConvertToBoldItalic(char c)
    {
        // Unicode Mathematical Alphanumeric Symbols - Bold Italic
        if (c >= 'A' && c <= 'Z')
        {
            int codePoint = 0x1D468 + (c - 'A');
            return new Rune(codePoint);
        }
        if (c >= 'a' && c <= 'z')
        {
            int codePoint = 0x1D482 + (c - 'a');
            return new Rune(codePoint);
        }

        return new Rune(c);
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }

    /// <summary>
    /// Creates a copy of this text widget.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for Text).</param>
    /// <returns>A new Text instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Text(_text)
        {
            TextAlign = TextAlign,
            Style = Style,
            Name = Name,
            Group = Group,
            Width = Width,
            Height = Height,
            PaddingLeft = PaddingLeft,
            PaddingTop = PaddingTop,
            PaddingRight = PaddingRight,
            PaddingBottom = PaddingBottom,
            PositionX = PositionX,
            PositionY = PositionY,
            Visible = Visible,
            AllowWrapping = AllowWrapping,
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor
        };

        return clone;
    }
}

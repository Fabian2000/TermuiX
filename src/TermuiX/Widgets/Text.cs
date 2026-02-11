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
    bool IWidget.Hovered { get; set; }
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
            MarginLeft = MarginLeft,
            MarginTop = MarginTop,
            MarginRight = MarginRight,
            MarginBottom = MarginBottom,
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

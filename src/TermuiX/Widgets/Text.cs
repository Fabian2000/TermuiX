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
    /// Enables inline Markdown rendering. When true, overrides the Style property.
    /// Supports **bold**, *italic*, `code`, ~~strike~~, and fenced code blocks with syntax highlighting.
    /// </summary>
    public bool Markdown { get; set; } = false;

    /// <summary>Background color for inline `code` and fenced code blocks.</summary>
    public Color? CodeBackgroundColor { get; set; }

    /// <summary>Foreground color for inline `code` text.</summary>
    public Color? CodeForegroundColor { get; set; }

    private TextStyle[][]? _rawStyles;
    private (Color[][] fg, Color[][] bg)? _rawColors;

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
    /// Gets or sets the minimum width constraint.
    /// </summary>
    public string MinWidth { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum width constraint.
    /// </summary>
    public string MaxWidth { get; set; } = "";

    /// <summary>
    /// Gets or sets the minimum height constraint.
    /// </summary>
    public string MinHeight { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum height constraint.
    /// </summary>
    public string MaxHeight { get; set; } = "";

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
    public Color BackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets a value indicating whether the text widget can receive focus.
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
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }
    bool IWidget.Disabled { get; set; }
    Color? IWidget.DisabledBackgroundColor { get; set; }
    Color IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    Rune[][] IWidget.GetRaw()
    {
        // If text is empty and using default width/height (100%), set computed size to 0
        if (string.IsNullOrEmpty(_text) && Width == "100%" && Height == "100%")
        {
            ((IWidget)this).ComputedWidth = 0;
            ((IWidget)this).ComputedHeight = 0;
            return [];
        }

        // Use ComputedWidth/Height when set by the Renderer/StackPanel layout.
        // Fall back to CalculateSize when no computed value is available.
        int computedW = ((IWidget)this).ComputedWidth;
        int actualWidth = computedW > 0 ? computedW : CalculateSize(Width, ((IWidget)this).Parent, true);

        // For Height="auto": always recompute from wrapping — start with 1 line
        // and let the expansion logic determine the actual height. Never reuse the
        // cached ComputedHeight because resize changes wrapping and line count.
        bool autoHeight = Height.Equals("auto", StringComparison.OrdinalIgnoreCase);
        int computedH = ((IWidget)this).ComputedHeight;
        int actualHeight;
        if (autoHeight && actualWidth > 0 && !string.IsNullOrEmpty(_text))
        {
            actualHeight = 1; // Will be expanded after wrapping
        }
        else
        {
            actualHeight = computedH > 0 ? computedH : CalculateSize(Height, ((IWidget)this).Parent, false);
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

        if (string.IsNullOrEmpty(_text))
        {
            _rawStyles = null;
            _rawColors = null;
            return result;
        }

        if (Markdown)
        {
            return RenderMarkdown(result, actualWidth, ref actualHeight);
        }

        return RenderPlainText(result, actualWidth, ref actualHeight);
    }

    private Rune[][] RenderPlainText(Rune[][] result, int actualWidth, ref int actualHeight)
    {
        // Build wrapped lines: split by \n first, then word-wrap if AllowWrapping
        var sourceLines = _text.Split('\n');
        var wrappedLines = new List<List<Rune>>();

        foreach (var sourceLine in sourceLines)
        {
            if (!AllowWrapping)
            {
                var lineRunes = new List<Rune>();
                int lineWidth = 0;
                foreach (Rune rune in sourceLine.EnumerateRunes())
                {
                    int rw = GetRuneDisplayWidth(rune);
                    if (lineWidth + rw > actualWidth) { break; }
                    lineRunes.Add(rune);
                    lineWidth += rw;
                }
                wrappedLines.Add(lineRunes);
            }
            else
            {
                WrapPlainLine(sourceLine, actualWidth, wrappedLines);
            }
        }

        result = FinalizeLines(wrappedLines, actualWidth, ref actualHeight);

        // Build per-cell style array for ANSI SGR rendering
        if (Style != TextStyle.Normal)
        {
            _rawStyles = new TextStyle[actualHeight][];
            for (int i = 0; i < actualHeight; i++)
            {
                _rawStyles[i] = new TextStyle[actualWidth];
                Array.Fill(_rawStyles[i], Style);
            }
        }
        else
        {
            _rawStyles = null;
        }
        _rawColors = null;

        return result;
    }

    private Rune[][] RenderMarkdown(Rune[][] result, int actualWidth, ref int actualHeight)
    {
        var parsedLines = ParseMarkdown(_text);
        var wrappedLines = new List<List<StyledRune>>();

        foreach (var styledLine in parsedLines)
        {
            if (!AllowWrapping)
            {
                var lineOut = new List<StyledRune>();
                int lineWidth = 0;
                foreach (var sr in styledLine)
                {
                    int rw = GetRuneDisplayWidth(sr.Rune);
                    if (lineWidth + rw > actualWidth) { break; }
                    lineOut.Add(sr);
                    lineWidth += rw;
                }
                wrappedLines.Add(lineOut);
            }
            else
            {
                WrapStyledLine(styledLine, actualWidth, wrappedLines);
            }
        }

        // Expand height if needed
        if (wrappedLines.Count > actualHeight)
        {
            actualHeight = wrappedLines.Count;
            ((IWidget)this).ComputedHeight = actualHeight;
        }

        // Build result buffers
        result = new Rune[actualHeight][];
        _rawStyles = new TextStyle[actualHeight][];
        var fgBuf = new Color[actualHeight][];
        var bgBuf = new Color[actualHeight][];
        bool hasColors = false;

        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new Rune[actualWidth];
            Array.Fill(result[i], new Rune(' '));
            _rawStyles[i] = new TextStyle[actualWidth];
            fgBuf[i] = new Color[actualWidth];
            bgBuf[i] = new Color[actualWidth];
        }

        // Render styled wrapped lines
        for (int i = 0; i < wrappedLines.Count && i < actualHeight; i++)
        {
            var line = wrappedLines[i];
            int totalWidth = 0;
            foreach (var sr in line)
            {
                totalWidth += GetRuneDisplayWidth(sr.Rune);
            }

            int offset = TextAlign switch
            {
                TextAlign.Center => Math.Max(0, (actualWidth - totalWidth) / 2),
                TextAlign.Right => Math.Max(0, actualWidth - totalWidth),
                _ => 0
            };

            int currentX = offset;
            for (int j = 0; j < line.Count && currentX < actualWidth; j++)
            {
                var sr = line[j];
                result[i][currentX] = sr.Rune;
                _rawStyles[i][currentX] = sr.Style;

                if (sr.Fg.HasValue)
                {
                    fgBuf[i][currentX] = sr.Fg.Value;
                    hasColors = true;
                }
                if (sr.Bg.HasValue)
                {
                    bgBuf[i][currentX] = sr.Bg.Value;
                    hasColors = true;
                }

                int runeWidth = GetRuneDisplayWidth(sr.Rune);
                for (int k = 1; k < runeWidth && currentX + k < actualWidth; k++)
                {
                    result[i][currentX + k] = new Rune(' ');
                    _rawStyles[i][currentX + k] = sr.Style;
                    if (sr.Bg.HasValue)
                    {
                        bgBuf[i][currentX + k] = sr.Bg.Value;
                    }
                }
                currentX += runeWidth;
            }
        }

        _rawColors = hasColors ? (fgBuf, bgBuf) : null;
        return result;
    }

    private void WrapPlainLine(string sourceLine, int actualWidth, List<List<Rune>> wrappedLines)
    {
        var words = SplitIntoWords(sourceLine);
        var currentLine = new List<Rune>();
        int currentWidth = 0;

        foreach (var word in words)
        {
            var wordRunes = new List<Rune>();
            int wordWidth = 0;
            foreach (Rune rune in word.EnumerateRunes())
            {
                wordRunes.Add(rune);
                wordWidth += GetRuneDisplayWidth(rune);
            }

            if (wordWidth > actualWidth)
            {
                foreach (var rune in wordRunes)
                {
                    int rw = GetRuneDisplayWidth(rune);
                    if (currentWidth + rw > actualWidth && currentLine.Count > 0)
                    {
                        wrappedLines.Add(currentLine);
                        currentLine = new List<Rune>();
                        currentWidth = 0;
                    }
                    currentLine.Add(rune);
                    currentWidth += rw;
                }
                continue;
            }

            if (currentWidth + wordWidth > actualWidth && currentLine.Count > 0)
            {
                while (currentLine.Count > 0 && currentLine[^1].Value == ' ')
                {
                    currentLine.RemoveAt(currentLine.Count - 1);
                }
                wrappedLines.Add(currentLine);
                currentLine = new List<Rune>();
                currentWidth = 0;
                if (wordRunes.Count > 0 && wordRunes[0].Value == ' ')
                {
                    wordRunes.RemoveAt(0);
                    wordWidth -= 1;
                }
            }

            currentLine.AddRange(wordRunes);
            currentWidth += wordWidth;
        }

        if (currentLine.Count > 0)
        {
            wrappedLines.Add(currentLine);
        }

        if (wrappedLines.Count == 0 || sourceLine.Length == 0)
        {
            wrappedLines.Add(new List<Rune>());
        }
    }

    private static void WrapStyledLine(List<StyledRune> styledLine, int actualWidth, List<List<StyledRune>> wrappedLines)
    {
        // Split styled runes into "words" (space-delimited)
        var currentLine = new List<StyledRune>();
        int currentWidth = 0;
        var wordBuf = new List<StyledRune>();
        int wordWidth = 0;

        for (int i = 0; i < styledLine.Count; i++)
        {
            var sr = styledLine[i];
            int rw = GetRuneDisplayWidth(sr.Rune);
            bool isSpace = sr.Rune.Value == ' ';

            wordBuf.Add(sr);
            wordWidth += rw;

            // End of word: at space followed by non-space, or end of line
            bool endOfWord = isSpace && i + 1 < styledLine.Count && styledLine[i + 1].Rune.Value != ' ';
            bool endOfLine = i == styledLine.Count - 1;

            if (endOfWord || endOfLine)
            {
                if (wordWidth > actualWidth)
                {
                    // Word too long — break char by char
                    foreach (var wr in wordBuf)
                    {
                        int wrw = GetRuneDisplayWidth(wr.Rune);
                        if (currentWidth + wrw > actualWidth && currentLine.Count > 0)
                        {
                            wrappedLines.Add(currentLine);
                            currentLine = new List<StyledRune>();
                            currentWidth = 0;
                        }
                        currentLine.Add(wr);
                        currentWidth += wrw;
                    }
                }
                else if (currentWidth + wordWidth > actualWidth && currentLine.Count > 0)
                {
                    // Trim trailing spaces
                    while (currentLine.Count > 0 && currentLine[^1].Rune.Value == ' ')
                    {
                        currentLine.RemoveAt(currentLine.Count - 1);
                    }
                    wrappedLines.Add(currentLine);
                    currentLine = new List<StyledRune>();
                    currentWidth = 0;
                    // Skip leading space
                    if (wordBuf.Count > 0 && wordBuf[0].Rune.Value == ' ')
                    {
                        wordBuf.RemoveAt(0);
                        wordWidth -= 1;
                    }
                    currentLine.AddRange(wordBuf);
                    currentWidth += wordWidth;
                }
                else
                {
                    currentLine.AddRange(wordBuf);
                    currentWidth += wordWidth;
                }

                wordBuf.Clear();
                wordWidth = 0;
            }
        }

        if (currentLine.Count > 0)
        {
            wrappedLines.Add(currentLine);
        }

        if (styledLine.Count == 0)
        {
            wrappedLines.Add(new List<StyledRune>());
        }
    }

    private Rune[][] FinalizeLines(List<List<Rune>> wrappedLines, int actualWidth, ref int actualHeight)
    {
        if (wrappedLines.Count > actualHeight)
        {
            actualHeight = wrappedLines.Count;
            ((IWidget)this).ComputedHeight = actualHeight;
        }

        var result = new Rune[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new Rune[actualWidth];
            Array.Fill(result[i], new Rune(' '));
        }

        for (int i = 0; i < wrappedLines.Count && i < actualHeight; i++)
        {
            var displayRunes = wrappedLines[i];
            int totalDisplayWidth = 0;
            foreach (var r in displayRunes)
            {
                totalDisplayWidth += GetRuneDisplayWidth(r);
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
                for (int k = 1; k < runeWidth && currentX + k < actualWidth; k++)
                {
                    result[i][currentX + k] = new Rune(' ');
                }
                currentX += runeWidth;
            }
        }

        return result;
    }

    TextStyle[][]? IWidget.GetRawStyles() => _rawStyles;

    /// <summary>
    /// Splits a string into words, keeping the whitespace attached to each word
    /// so that spacing is preserved during word-wrap.
    /// </summary>
    private static List<string> SplitIntoWords(string text)
    {
        var words = new List<string>();
        int start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ' && i + 1 < text.Length && text[i + 1] != ' ')
            {
                words.Add(text[start..(i + 1)]);
                start = i + 1;
            }
        }

        if (start < text.Length)
        {
            words.Add(text[start..]);
        }

        return words;
    }

    private int CalculateSize(string size, IWidget? parent, bool isWidth)
    {
        if (string.IsNullOrEmpty(size))
        {
            return 0;
        }

        size = size.Trim();

        if (size.Equals("fill", StringComparison.OrdinalIgnoreCase))
        {
            if (parent is null)
            {
                return isWidth ? Console.WindowWidth : Console.WindowHeight;
            }
            return isWidth ?
                (parent.ComputedWidth > 0 ? parent.ComputedWidth : Console.WindowWidth) :
                (parent.ComputedHeight > 0 ? parent.ComputedHeight : Console.WindowHeight);
        }

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

            if (parent is null)
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
                // Note: border is already accounted for by the Renderer's padding adjustment
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

    /// <summary>
    /// Returns the display width of a rune in terminal columns (0, 1, or 2).
    /// </summary>
    /// <param name="rune">The rune to measure.</param>
    /// <returns>The number of terminal columns the rune occupies.</returns>
    public static int GetRuneDisplayWidth(Rune rune)
    {
        int value = rune.Value;

        // Control characters
        if (value == 0 || Rune.IsControl(rune))
        {
            return 0;
        }

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
            (value >= 0x2630 && value <= 0x2637) ||   // ☰-☷ I Ching trigrams
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

    (Color[][] fg, Color[][] bg)? IWidget.GetRawColors() => _rawColors;

    // ── Markdown Parsing ──────────────────────────────────────────────

    private record struct StyledRune(Rune Rune, TextStyle Style, Color? Fg, Color? Bg);

    /// <summary>
    /// Parses the full text into styled lines, handling fenced code blocks and inline markdown.
    /// </summary>
    private List<List<StyledRune>> ParseMarkdown(string text)
    {
        var result = new List<List<StyledRune>>();
        var lines = text.Split('\n');
        bool inCodeBlock = false;
        string codeBlockLang = "";
        var codeBg = CodeBackgroundColor;
        var codeFg = CodeForegroundColor;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    inCodeBlock = true;
                    codeBlockLang = trimmed.Length > 3 ? trimmed[3..].Trim() : "";
                    // Don't add the ``` line itself
                    continue;
                }
                else
                {
                    inCodeBlock = false;
                    codeBlockLang = "";
                    continue;
                }
            }

            if (inCodeBlock)
            {
                result.Add(HighlightSyntax(line, codeBlockLang, codeBg));
            }
            else if (trimmed.StartsWith("### "))
            {
                // H3 — bold
                var content = trimmed[4..];
                var styledLine = new List<StyledRune>();
                foreach (Rune r in content.EnumerateRunes())
                {
                    styledLine.Add(new StyledRune(r, TextStyle.Bold, null, null));
                }
                result.Add(styledLine);
            }
            else if (trimmed.StartsWith("## "))
            {
                // H2 — bold
                var content = trimmed[3..];
                var styledLine = new List<StyledRune>();
                foreach (Rune r in content.EnumerateRunes())
                {
                    styledLine.Add(new StyledRune(r, TextStyle.Bold, null, null));
                }
                result.Add(styledLine);
            }
            else if (trimmed.StartsWith("# "))
            {
                // H1 — bold
                var content = trimmed[2..];
                var styledLine = new List<StyledRune>();
                foreach (Rune r in content.EnumerateRunes())
                {
                    styledLine.Add(new StyledRune(r, TextStyle.Bold, null, null));
                }
                result.Add(styledLine);
            }
            else
            {
                result.Add(ParseInlineMarkdown(line));
            }
        }

        return result;
    }

    /// <summary>
    /// Parses inline markdown: **bold**, *italic*, `code`, ~~strike~~, bullet lists
    /// </summary>
    private List<StyledRune> ParseInlineMarkdown(string line)
    {
        var result = new List<StyledRune>();
        bool isBold = false, isItalic = false, isStrike = false, isCode = false;
        var codeBg = CodeBackgroundColor;
        var codeFg = CodeForegroundColor;
        int i = 0;

        // Detect bullet list lines: "* ", "- ", "1. " etc. at start (after optional whitespace)
        var trimmed = line.AsSpan().TrimStart();
        int contentStart = line.Length - trimmed.Length;
        bool isBulletLine = false;
        int bulletEnd = 0; // index after the bullet prefix (including the space)

        if (trimmed.Length >= 2)
        {
            // "* " or "- " or "+ "
            if ((trimmed[0] == '*' || trimmed[0] == '-' || trimmed[0] == '+') && trimmed[1] == ' ')
            {
                isBulletLine = true;
                bulletEnd = contentStart + 2;
            }
            // "1. ", "2. " etc.
            else if (char.IsDigit(trimmed[0]))
            {
                int d = 0;
                while (d < trimmed.Length && char.IsDigit(trimmed[d])) { d++; }
                if (d < trimmed.Length - 1 && trimmed[d] == '.' && trimmed[d + 1] == ' ')
                {
                    isBulletLine = true;
                    bulletEnd = contentStart + d + 2;
                }
            }
        }

        // Render bullet prefix as literal text (not markdown)
        if (isBulletLine)
        {
            // Replace "* " with "• " for nicer bullets, keep "- " and numbered as-is
            if (line[contentStart] == '*')
            {
                // Add leading whitespace
                for (int w = 0; w < contentStart; w++)
                {
                    result.Add(new StyledRune(new Rune(line[w]), TextStyle.Normal, null, null));
                }
                result.Add(new StyledRune(new Rune('•'), TextStyle.Normal, null, null));
                result.Add(new StyledRune(new Rune(' '), TextStyle.Normal, null, null));
            }
            else
            {
                for (int w = 0; w < bulletEnd; w++)
                {
                    result.Add(new StyledRune(new Rune(line[w]), TextStyle.Normal, null, null));
                }
            }
            i = bulletEnd;
        }

        while (i < line.Length)
        {
            // Backtick — inline code (no nesting inside code)
            if (line[i] == '`' && !isCode)
            {
                isCode = true;
                i++;
                continue;
            }
            if (line[i] == '`' && isCode)
            {
                isCode = false;
                i++;
                continue;
            }

            // Inside inline code — no markdown parsing
            if (isCode)
            {
                if (char.IsHighSurrogate(line[i]) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
                {
                    result.Add(new StyledRune(new Rune(line[i], line[i + 1]), TextStyle.Normal, codeFg, codeBg));
                    i += 2;
                }
                else
                {
                    result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, codeFg, codeBg));
                    i++;
                }
                continue;
            }

            // ~~strikethrough~~
            if (i + 1 < line.Length && line[i] == '~' && line[i + 1] == '~')
            {
                isStrike = !isStrike;
                i += 2;
                continue;
            }

            // *** or ___ — bold+italic toggle
            if (i + 2 < line.Length &&
                ((line[i] == '*' && line[i + 1] == '*' && line[i + 2] == '*') ||
                 (line[i] == '_' && line[i + 1] == '_' && line[i + 2] == '_')))
            {
                isBold = !isBold;
                isItalic = !isItalic;
                i += 3;
                continue;
            }

            // ** or __ — bold toggle
            if (i + 1 < line.Length &&
                ((line[i] == '*' && line[i + 1] == '*') ||
                 (line[i] == '_' && line[i + 1] == '_')))
            {
                isBold = !isBold;
                i += 2;
                continue;
            }

            // * or _ — italic toggle (only mid-word/inline, not at line start as bullet)
            if (line[i] == '*' || line[i] == '_')
            {
                isItalic = !isItalic;
                i++;
                continue;
            }

            // Regular character (handle surrogate pairs for emojis)
            var style = GetCombinedStyle(isBold, isItalic, isStrike);
            if (char.IsHighSurrogate(line[i]) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
            {
                result.Add(new StyledRune(new Rune(line[i], line[i + 1]), style, null, null));
                i += 2;
            }
            else
            {
                result.Add(new StyledRune(new Rune(line[i]), style, null, null));
                i++;
            }
        }

        return result;
    }

    private static TextStyle GetCombinedStyle(bool bold, bool italic, bool strike)
    {
        if (bold && italic) { return TextStyle.BoldItalic; }
        if (bold) { return TextStyle.Bold; }
        if (italic) { return TextStyle.Italic; }
        if (strike) { return TextStyle.Strikethrough; }
        return TextStyle.Normal;
    }

    // ── Syntax Highlighting ──────────────────────────────────────────

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "if", "else", "for", "foreach", "while", "do", "switch", "case", "break", "continue",
        "return", "class", "struct", "enum", "interface", "record",
        "function", "fn", "func", "def", "lambda",
        "var", "let", "const", "mut", "ref", "out",
        "import", "from", "export", "require", "include", "using", "namespace", "package", "module",
        "public", "private", "protected", "internal", "static", "abstract", "virtual", "override", "sealed",
        "void", "int", "float", "double", "string", "bool", "char", "byte", "long", "short",
        "true", "false", "null", "nil", "None", "undefined", "NaN",
        "new", "this", "self", "super", "base",
        "async", "await", "yield",
        "try", "catch", "finally", "throw", "raise", "except",
        "in", "is", "as", "not", "and", "or", "typeof", "sizeof", "nameof",
        "where", "select", "from", "join", "on", "into", "orderby", "group",
        "with", "match", "when", "then", "end", "begin",
        "print", "println", "printf", "fmt", "console", "log",
    };

    // Catppuccin Mocha colors for syntax highlighting
    private static readonly Color SyntaxKeyword = Color.Parse("#89b4fa");   // Blue
    private static readonly Color SyntaxString = Color.Parse("#a6e3a1");    // Green
    private static readonly Color SyntaxComment = Color.Parse("#6c7086");   // Overlay0
    private static readonly Color SyntaxNumber = Color.Parse("#fab387");    // Peach
    private static readonly Color SyntaxFunction = Color.Parse("#f9e2af");  // Yellow
    private static readonly Color SyntaxType = Color.Parse("#94e2d5");      // Teal
    private static readonly Color SyntaxOperator = Color.Parse("#9399b2");  // Overlay2
    private static readonly Color SyntaxAttribute = Color.Parse("#cba6f7"); // Mauve

    private static readonly HashSet<string> BuiltinTypes = new(StringComparer.Ordinal)
    {
        // C# / Java / C / C++
        "String", "Int32", "Int64", "Boolean", "Double", "Single", "Decimal", "Byte", "Char",
        "Object", "List", "Dictionary", "Array", "HashSet", "Queue", "Stack",
        "Task", "Func", "Action", "IEnumerable", "IList", "IDictionary", "ICollection",
        "Console", "Math", "File", "Path", "Directory", "Stream", "Exception",
        "StringBuilder", "DateTime", "TimeSpan", "Guid",
        // Python
        "str", "dict", "list", "tuple", "set", "frozenset", "type", "object",
        // JS/TS
        "Promise", "Map", "Set", "WeakMap", "WeakSet", "RegExp", "Error", "JSON",
        "Number", "Symbol", "BigInt", "Date",
    };

    private List<StyledRune> HighlightSyntax(string line, string language, Color? codeBg)
    {
        var result = new List<StyledRune>();
        int i = 0;

        while (i < line.Length)
        {
            char c = line[i];

            // Line comment: // or #
            if ((c == '/' && i + 1 < line.Length && line[i + 1] == '/') ||
                (c == '#' && (language != "csharp" && language != "c#")))
            {
                for (int j = i; j < line.Length;)
                {
                    if (char.IsHighSurrogate(line[j]) && j + 1 < line.Length && char.IsLowSurrogate(line[j + 1]))
                    {
                        result.Add(new StyledRune(new Rune(line[j], line[j + 1]), TextStyle.Italic, SyntaxComment, codeBg));
                        j += 2;
                    }
                    else
                    {
                        result.Add(new StyledRune(new Rune(line[j]), TextStyle.Italic, SyntaxComment, codeBg));
                        j++;
                    }
                }
                return result;
            }

            // Decorators / Attributes: @word or [word]
            if (c == '@' && i + 1 < line.Length && char.IsLetter(line[i + 1]))
            {
                int start = i;
                i++;
                while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_' || line[i] == '.'))
                {
                    i++;
                }
                for (int j = start; j < i; j++)
                {
                    result.Add(new StyledRune(new Rune(line[j]), TextStyle.Italic, SyntaxAttribute, codeBg));
                }
                continue;
            }

            // String literals
            if (c == '"' || c == '\'')
            {
                char quote = c;
                result.Add(new StyledRune(new Rune(c), TextStyle.Normal, SyntaxString, codeBg));
                i++;
                while (i < line.Length)
                {
                    if (line[i] == '\\' && i + 1 < line.Length)
                    {
                        result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, SyntaxString, codeBg));
                        i++;
                        if (char.IsHighSurrogate(line[i]) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
                        {
                            result.Add(new StyledRune(new Rune(line[i], line[i + 1]), TextStyle.Normal, SyntaxString, codeBg));
                            i += 2;
                        }
                        else
                        {
                            result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, SyntaxString, codeBg));
                            i++;
                        }
                        continue;
                    }
                    if (char.IsHighSurrogate(line[i]) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
                    {
                        result.Add(new StyledRune(new Rune(line[i], line[i + 1]), TextStyle.Normal, SyntaxString, codeBg));
                        i += 2;
                        continue;
                    }
                    result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, SyntaxString, codeBg));
                    if (line[i] == quote) { i++; break; }
                    i++;
                }
                continue;
            }

            // Numbers
            if (char.IsDigit(c) && (i == 0 || !char.IsLetter(line[i - 1])))
            {
                while (i < line.Length && (char.IsDigit(line[i]) || line[i] == '.' || line[i] == 'x' || line[i] == 'f' || line[i] == 'L' || line[i] == '_'))
                {
                    result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, SyntaxNumber, codeBg));
                    i++;
                }
                continue;
            }

            // Keywords / identifiers / types / functions
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_'))
                {
                    i++;
                }
                string word = line[start..i];

                // Determine token type
                Color? fg;
                TextStyle ws;

                if (Keywords.Contains(word))
                {
                    fg = SyntaxKeyword;
                    ws = TextStyle.Normal;
                }
                else if (i < line.Length && line[i] == '(')
                {
                    // Function call: word followed by (
                    fg = SyntaxFunction;
                    ws = TextStyle.Normal;
                }
                else if (BuiltinTypes.Contains(word) || (word.Length > 1 && char.IsUpper(word[0]) && word.Any(char.IsLower)))
                {
                    // Known types OR PascalCase identifiers (likely types/classes)
                    fg = SyntaxType;
                    ws = TextStyle.Normal;
                }
                else
                {
                    fg = null;
                    ws = TextStyle.Normal;
                }

                foreach (char ch in word)
                {
                    result.Add(new StyledRune(new Rune(ch), ws, fg, codeBg));
                }
                continue;
            }

            // Operators: =, ==, !=, <=, >=, =>, ->, &&, ||, ++, --
            if ("=!<>&|+-".Contains(c) && i + 1 < line.Length && "=>&|+-".Contains(line[i + 1]))
            {
                result.Add(new StyledRune(new Rune(line[i]), TextStyle.Normal, SyntaxOperator, codeBg));
                result.Add(new StyledRune(new Rune(line[i + 1]), TextStyle.Normal, SyntaxOperator, codeBg));
                i += 2;
                continue;
            }

            // Single-char operators and punctuation
            if ("=<>!&|+-*/%^~?:.;,".Contains(c))
            {
                result.Add(new StyledRune(new Rune(c), TextStyle.Normal, SyntaxOperator, codeBg));
                i++;
                continue;
            }

            // Brackets/braces/other — keep default color (handle surrogate pairs)
            if (char.IsHighSurrogate(c) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
            {
                result.Add(new StyledRune(new Rune(line[i], line[i + 1]), TextStyle.Normal, null, codeBg));
                i += 2;
            }
            else
            {
                result.Add(new StyledRune(new Rune(c), TextStyle.Normal, null, codeBg));
                i++;
            }
        }

        return result;
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
            MinWidth = MinWidth,
            MaxWidth = MaxWidth,
            MinHeight = MinHeight,
            MaxHeight = MaxHeight,
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
            FocusForegroundColor = FocusForegroundColor,
            Markdown = Markdown,
            CodeBackgroundColor = CodeBackgroundColor,
            CodeForegroundColor = CodeForegroundColor
        };

        return clone;
    }
}

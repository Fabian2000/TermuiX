using System.Diagnostics;
using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A text input widget that supports single-line and multi-line text entry.
/// </summary>
public class Input : IWidget
{
    private string _text = string.Empty;
    private int _cursorPosition = 0;
    private bool _cursorVisible = false;
    private DateTime _lastCursorBlink = DateTime.Now;
    private const int CursorBlinkIntervalMs = 500;
    private int _scrollOffsetY = 0;
    private char? _pendingSurrogate = null;

    /// <summary>
    /// Gets or sets the text value of the input.
    /// </summary>
    public string Value
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = Math.Min(_cursorPosition, _text.Length);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether multi-line input is enabled.
    /// </summary>
    public bool Multiline { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the input displays as a password (masked with asterisks).
    /// </summary>
    public bool IsPassword { get; set; } = false;

    /// <summary>
    /// Gets or sets the placeholder text shown when the input is empty.
    /// </summary>
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique name of the input.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the input.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the input.
    /// </summary>
    public string Width { get; set; } = "30ch";

    /// <summary>
    /// Gets or sets the height of the input.
    /// </summary>
    public string Height { get; set; } = "3ch";

    /// <summary>
    /// Gets or sets the left padding.
    /// </summary>
    public string PaddingLeft { get; set; } = "1ch";

    /// <summary>
    /// Gets or sets the top padding.
    /// </summary>
    public string PaddingTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right padding.
    /// </summary>
    public string PaddingRight { get; set; } = "1ch";

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
    /// Gets or sets a value indicating whether the input is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = true;

    private ConsoleColor _backgroundColor = ConsoleColor.DarkGray;
    private ConsoleColor _foregroundColor = ConsoleColor.White;
    private ConsoleColor _focusBackgroundColor = ConsoleColor.Gray;
    private ConsoleColor _focusForegroundColor = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public ConsoleColor BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public ConsoleColor ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public ConsoleColor FocusBackgroundColor
    {
        get => _focusBackgroundColor;
        set => _focusBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor
    {
        get => _focusForegroundColor;
        set => _focusForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public ConsoleColor BorderColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the border color when focused.
    /// </summary>
    public ConsoleColor FocusBorderColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the placeholder text color.
    /// </summary>
    public ConsoleColor PlaceholderColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the cursor color.
    /// </summary>
    public ConsoleColor CursorColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets a value indicating whether the input is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the background color when disabled.
    /// If null, the normal background color is used.
    /// </summary>
    public ConsoleColor? DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color when disabled.
    /// </summary>
    public ConsoleColor DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets a value indicating whether the input can receive focus.
    /// </summary>
    public bool CanFocus => !Disabled;

    /// <summary>
    /// Gets a value indicating whether the input is scrollable.
    /// </summary>
    public bool Scrollable => false;

    /// <summary>
    /// Occurs when the user submits the input (presses Enter in single-line mode).
    /// </summary>
    public event EventHandler<string>? EnterPressed;

    /// <summary>
    /// Occurs when the text value changes.
    /// </summary>
    public event EventHandler<string>? TextChanged;

    // Explicit interface implementation
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

    Rune[][] IWidget.GetRaw()
    {
        // Update cursor blink
        if (((IWidget)this).Focussed && !Disabled)
        {
            var elapsed = (DateTime.Now - _lastCursorBlink).TotalMilliseconds;
            if (elapsed >= CursorBlinkIntervalMs)
            {
                _cursorVisible = !_cursorVisible;
                _lastCursorBlink = DateTime.Now;
            }
        }
        else
        {
            _cursorVisible = false;
        }

        // Calculate size
        int width = CalculateSize(Width, ((IWidget)this).Parent, true);
        int height = Multiline ? CalculateSize(Height, ((IWidget)this).Parent, false) : 1;

        // Store computed values
        ((IWidget)this).ComputedWidth = width;
        ((IWidget)this).ComputedHeight = height;

        if (width <= 0 || height <= 0) return [];

        // Temporarily override colors if disabled
        ConsoleColor originalBg = _backgroundColor;
        ConsoleColor originalFg = _foregroundColor;
        ConsoleColor originalFocusBg = _focusBackgroundColor;
        ConsoleColor originalFocusFg = _focusForegroundColor;

        if (Disabled)
        {
            _foregroundColor = DisabledForegroundColor;
            _focusForegroundColor = DisabledForegroundColor;
        }

        // Create output
        var result = new Rune[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new Rune[width];
            Array.Fill(result[i], new Rune(' '));
        }

        // Render text or placeholder
        string displayText;
        if (string.IsNullOrEmpty(_text))
        {
            displayText = Placeholder;
        }
        else if (IsPassword)
        {
            // Mask password with asterisks
            displayText = new string('*', _text.Length);
        }
        else
        {
            displayText = _text;
        }

        if (!string.IsNullOrEmpty(displayText))
        {
            if (Multiline)
            {
                // Multiline: automatic word wrapping
                RenderMultiline(result, displayText, width, height);
            }
            else
            {
                // Single-line: horizontal scrolling to keep cursor visible
                RenderSingleLine(result, displayText, width);
            }
        }
        else if (_cursorVisible && ((IWidget)this).Focussed)
        {
            // Empty text, show cursor at start
            result[0][0] = new Rune('█');
        }

        // Restore original colors
        _backgroundColor = originalBg;
        _foregroundColor = originalFg;
        _focusBackgroundColor = originalFocusBg;
        _focusForegroundColor = originalFocusFg;

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Ignore input if disabled
        if (Disabled)
        {
            return;
        }

        // Handle special keys
        if (keyInfo.Key == ConsoleKey.Enter)
        {
            if (Multiline)
            {
                // Multiline: Enter adds new line
                InsertChar('\n');
            }
            else
            {
                // Single-line: Enter submits
                OnSubmit();
            }
        }
        else if (keyInfo.Key == ConsoleKey.Backspace)
        {
            if (_cursorPosition > 0)
            {
                // Convert to rune index to remove correctly
                var runes = _text.EnumerateRunes().ToArray();
                if (_cursorPosition - 1 < runes.Length)
                {
                    var newRunes = runes.Take(_cursorPosition - 1).Concat(runes.Skip(_cursorPosition)).ToArray();
                    _text = string.Concat(newRunes.Select(r => r.ToString()));
                    _cursorPosition--;
                    OnChanged();
                }
            }
        }
        else if (keyInfo.Key == ConsoleKey.Delete)
        {
            var runeCount = _text.EnumerateRunes().Count();
            if (_cursorPosition < runeCount)
            {
                var runes = _text.EnumerateRunes().ToArray();
                var newRunes = runes.Take(_cursorPosition).Concat(runes.Skip(_cursorPosition + 1)).ToArray();
                _text = string.Concat(newRunes.Select(r => r.ToString()));
                OnChanged();
            }
        }
        else if (keyInfo.Key == ConsoleKey.LeftArrow)
        {
            _cursorPosition = Math.Max(0, _cursorPosition - 1);
        }
        else if (keyInfo.Key == ConsoleKey.RightArrow)
        {
            var runeCount = _text.EnumerateRunes().Count();
            _cursorPosition = Math.Min(runeCount, _cursorPosition + 1);
        }
        else if (keyInfo.Key == ConsoleKey.Home)
        {
            _cursorPosition = 0;
        }
        else if (keyInfo.Key == ConsoleKey.End)
        {
            _cursorPosition = _text.EnumerateRunes().Count();
        }
        else if (keyInfo.Key == ConsoleKey.UpArrow)
        {
            if (Multiline)
            {
                MoveCursorVertical(-1);
            }
        }
        else if (keyInfo.Key == ConsoleKey.DownArrow)
        {
            if (Multiline)
            {
                MoveCursorVertical(1);
            }
        }
        else if (keyInfo.Key == ConsoleKey.V && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            // Paste from clipboard
            try
            {
                string? clipboardText = GetClipboardText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    if (!Multiline)
                    {
                        clipboardText = clipboardText.Replace("\r\n", " ").Replace("\n", " ");
                    }
                    InsertText(clipboardText);
                }
            }
            catch
            {
                // Clipboard access failed, ignore
            }
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            // Handle surrogate pairs (emojis, etc.)
            if (char.IsHighSurrogate(keyInfo.KeyChar))
            {
                // Store the high surrogate and wait for the low surrogate
                _pendingSurrogate = keyInfo.KeyChar;
            }
            else if (char.IsLowSurrogate(keyInfo.KeyChar) && _pendingSurrogate.HasValue)
            {
                // Combine with pending high surrogate
                string surrogatePair = new string(new[] { _pendingSurrogate.Value, keyInfo.KeyChar });
                InsertText(surrogatePair);
                _pendingSurrogate = null;
            }
            else
            {
                // Regular character input
                _pendingSurrogate = null;
                InsertChar(keyInfo.KeyChar);
            }
        }

        // Reset cursor blink on any key press
        _cursorVisible = true;
        _lastCursorBlink = DateTime.Now;
    }

    private void InsertChar(char c)
    {
        // Don't allow newlines in single-line mode
        if (!Multiline && (c == '\n' || c == '\r'))
            return;

        // Insert character as string (handles surrogate pairs correctly)
        InsertText(c.ToString());
    }

    private void InsertText(string text)
    {
        // Convert to runes to insert at correct position
        var runes = _text.EnumerateRunes().ToList();
        var insertRunes = text.EnumerateRunes().ToList();
        runes.InsertRange(_cursorPosition, insertRunes);
        _text = string.Concat(runes.Select(r => r.ToString()));
        _cursorPosition += insertRunes.Count;
        OnChanged();
    }

    private void MoveCursorVertical(int direction)
    {
        if (string.IsNullOrEmpty(_text)) return;

        var lines = _text.Split('\n');

        // Find current line and column
        int currentLine = 0;
        int currentCol = 0;
        int runesSoFar = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineRuneCount = lines[i].EnumerateRunes().Count();
            if (runesSoFar + lineRuneCount >= _cursorPosition)
            {
                currentLine = i;
                currentCol = _cursorPosition - runesSoFar;
                break;
            }
            runesSoFar += lineRuneCount + 1; // +1 for newline
        }

        // Calculate target line
        int targetLine = currentLine + direction;
        if (targetLine < 0 || targetLine >= lines.Length)
            return; // Out of bounds

        // Calculate new cursor position
        int newPosition = 0;
        for (int i = 0; i < targetLine; i++)
        {
            newPosition += lines[i].EnumerateRunes().Count() + 1; // +1 for newline
        }

        // Keep same column, or end of line if shorter
        int targetLineRuneCount = lines[targetLine].EnumerateRunes().Count();
        int targetCol = Math.Min(currentCol, targetLineRuneCount);
        newPosition += targetCol;

        _cursorPosition = newPosition;
    }

    private string? GetClipboardText()
    {
        // Try to get clipboard text using various methods
        try
        {
            // For Linux with xclip
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -o",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
        catch
        {
            // Clipboard not available
            return null;
        }
    }

    /// <summary>
    /// Raises the EnterPressed event.
    /// </summary>
    protected virtual void OnSubmit()
    {
        EnterPressed?.Invoke(this, _text);
    }

    /// <summary>
    /// Raises the TextChanged event.
    /// </summary>
    protected virtual void OnChanged()
    {
        TextChanged?.Invoke(this, _text);
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

    private void RenderSingleLine(Rune[][] result, string displayText, int width)
    {
        // Convert string to Rune array first
        var runes = new List<Rune>();
        foreach (var rune in displayText.EnumerateRunes())
        {
            runes.Add(rune);
        }

        // Calculate display width up to cursor position
        int cursorDisplayPos = 0;
        for (int i = 0; i < _cursorPosition && i < runes.Count; i++)
        {
            cursorDisplayPos += GetRuneDisplayWidth(runes[i]);
        }

        // Calculate horizontal scroll offset (in runes) to keep cursor visible
        int scrollOffsetRunes = 0;
        if (cursorDisplayPos >= width)
        {
            // Need to scroll - find the rune index where display starts
            int displayOffset = cursorDisplayPos - width + 1;
            int accumulatedWidth = 0;
            for (int i = 0; i < runes.Count; i++)
            {
                if (accumulatedWidth >= displayOffset)
                {
                    scrollOffsetRunes = i;
                    break;
                }
                accumulatedWidth += GetRuneDisplayWidth(runes[i]);
            }
        }

        // Render visible portion of text
        int displayX = 0;
        int runeIdx = scrollOffsetRunes;

        while (runeIdx < runes.Count && displayX < width)
        {
            Rune rune = runes[runeIdx];
            int runeWidth = GetRuneDisplayWidth(rune);

            // Check if this would exceed width
            if (displayX + runeWidth > width)
            {
                break;
            }

            // Check if cursor should be shown at this rune position (replaces the character)
            if (runeIdx == _cursorPosition && _cursorVisible && ((IWidget)this).Focussed)
            {
                // Show cursor - it replaces the character at cursor position
                result[0][displayX] = new Rune('█');
                displayX++;

                // Fill additional cells if the replaced character was wide
                for (int k = 1; k < runeWidth && displayX < width; k++)
                {
                    result[0][displayX] = new Rune(' ');
                    displayX++;
                }

                runeIdx++;
                continue;
            }

            result[0][displayX] = rune;
            displayX++;

            // Fill additional cells for wide characters
            for (int k = 1; k < runeWidth && displayX < width; k++)
            {
                result[0][displayX] = new Rune(' ');
                displayX++;
            }

            runeIdx++;
        }

        // Cursor at end of text
        if (_cursorPosition == runes.Count && displayX < width && _cursorVisible && ((IWidget)this).Focussed)
        {
            result[0][displayX] = new Rune('█');
        }
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

    private void RenderMultiline(Rune[][] result, string displayText, int width, int height)
    {
        // Convert to runes and wrap based on display width
        var wrappedLines = new List<List<Rune>>();
        var lines = displayText.Split('\n');

        // Track which wrapped line corresponds to which original line
        var wrappedLineToOriginalLine = new List<int>();
        int originalLineIdx = 0;

        foreach (var line in lines)
        {
            var lineRunes = new List<Rune>();
            foreach (var rune in line.EnumerateRunes())
            {
                lineRunes.Add(rune);
            }

            if (lineRunes.Count == 0)
            {
                wrappedLines.Add(new List<Rune>());
                wrappedLineToOriginalLine.Add(originalLineIdx);
                originalLineIdx++;
                continue;
            }

            // Wrap based on display width
            var currentLine = new List<Rune>();
            int currentDisplayWidth = 0;

            foreach (var rune in lineRunes)
            {
                int runeWidth = GetRuneDisplayWidth(rune);

                if (currentDisplayWidth + runeWidth > width && currentLine.Count > 0)
                {
                    // Start new line (still part of same original line)
                    wrappedLines.Add(currentLine);
                    wrappedLineToOriginalLine.Add(originalLineIdx);
                    currentLine = new List<Rune>();
                    currentDisplayWidth = 0;
                }

                currentLine.Add(rune);
                currentDisplayWidth += runeWidth;
            }

            if (currentLine.Count > 0)
            {
                wrappedLines.Add(currentLine);
                wrappedLineToOriginalLine.Add(originalLineIdx);
            }

            originalLineIdx++;
        }

        // Find cursor position in original lines first
        int cursorOrigLine = 0;
        int cursorOrigCol = 0;
        int runesInOrigText = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineRuneCount = lines[i].EnumerateRunes().Count();
            if (_cursorPosition <= runesInOrigText + lineRuneCount)
            {
                cursorOrigLine = i;
                cursorOrigCol = _cursorPosition - runesInOrigText;
                break;
            }
            runesInOrigText += lineRuneCount + 1; // +1 for \n
        }

        // Now find which wrapped line contains this cursor position
        int cursorLine = 0;
        int cursorRuneCol = 0;
        int runesCountedInOrig = 0;

        for (int wIdx = 0; wIdx < wrappedLines.Count; wIdx++)
        {
            if (wrappedLineToOriginalLine[wIdx] != cursorOrigLine)
            {
                // Skip this wrapped line, it's not in our original line
                continue;
            }

            // This wrapped line is part of the cursor's original line
            int runesInThisWrappedLine = wrappedLines[wIdx].Count;

            if (cursorOrigCol <= runesCountedInOrig + runesInThisWrappedLine)
            {
                cursorLine = wIdx;
                cursorRuneCol = cursorOrigCol - runesCountedInOrig;
                break;
            }

            runesCountedInOrig += runesInThisWrappedLine;
        }

        // Adjust vertical scroll to keep cursor visible
        if (cursorLine < _scrollOffsetY)
        {
            _scrollOffsetY = cursorLine;
        }
        else if (cursorLine >= _scrollOffsetY + height)
        {
            _scrollOffsetY = cursorLine - height + 1;
        }

        // Render visible lines with scroll offset
        for (int displayLineIdx = 0; displayLineIdx < height; displayLineIdx++)
        {
            int sourceLineIdx = _scrollOffsetY + displayLineIdx;
            if (sourceLineIdx >= wrappedLines.Count) break;

            var lineRunes = wrappedLines[sourceLineIdx];

            // Render runes with display width tracking
            int displayX = 0;
            int runeIdx = 0;

            while (runeIdx < lineRunes.Count && displayX < width)
            {
                Rune rune = lineRunes[runeIdx];
                int runeWidth = GetRuneDisplayWidth(rune);

                if (displayX + runeWidth > width)
                {
                    break;
                }

                // Check if cursor should be shown at this rune position (replaces the character)
                if (sourceLineIdx == cursorLine && runeIdx == cursorRuneCol && _cursorVisible && ((IWidget)this).Focussed)
                {
                    // Show cursor - it replaces the character at cursor position
                    result[displayLineIdx][displayX] = new Rune('█');
                    displayX++;

                    // Fill additional cells if the replaced character was wide
                    for (int k = 1; k < runeWidth && displayX < width; k++)
                    {
                        result[displayLineIdx][displayX] = new Rune(' ');
                        displayX++;
                    }

                    runeIdx++;
                    continue;
                }

                result[displayLineIdx][displayX] = rune;
                displayX++;

                // Fill additional cells for wide characters
                for (int k = 1; k < runeWidth && displayX < width; k++)
                {
                    result[displayLineIdx][displayX] = new Rune(' ');
                    displayX++;
                }

                runeIdx++;
            }

            // Cursor at end of line
            if (sourceLineIdx == cursorLine && cursorRuneCol == lineRunes.Count && displayX < width && _cursorVisible && ((IWidget)this).Focussed)
            {
                result[displayLineIdx][displayX] = new Rune('█');
            }
        }
    }

    /// <summary>
    /// Creates a copy of this input widget.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for Input).</param>
    /// <returns>A new Input instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Input
        {
            _text = _text,
            _cursorPosition = _cursorPosition,
            Value = Value,
            Multiline = Multiline,
            IsPassword = IsPassword,
            Placeholder = Placeholder,
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
            FocusForegroundColor = FocusForegroundColor,
            BorderColor = BorderColor,
            FocusBorderColor = FocusBorderColor,
            PlaceholderColor = PlaceholderColor,
            CursorColor = CursorColor,
            Disabled = Disabled,
            DisabledBackgroundColor = DisabledBackgroundColor,
            DisabledForegroundColor = DisabledForegroundColor
        };

        return clone;
    }
}

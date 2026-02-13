using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// Determines which key combination submits a multiline input.
/// </summary>
public enum SubmitKeyMode
{
    /// <summary>Enter submits, Ctrl+Enter inserts a newline.</summary>
    Enter,
    /// <summary>Ctrl+Enter submits, Enter inserts a newline (editor-style).</summary>
    CtrlEnter
}

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
    /// Gets or sets the key used to submit the input.
    /// CtrlEnter (default): Ctrl+Enter submits, Enter inserts a newline.
    /// Enter: Enter submits, Ctrl+Enter inserts a newline (chat-style).
    /// Only affects behavior when Multiline is true; singleline always submits on Enter.
    /// </summary>
    public SubmitKeyMode SubmitKey { get; set; } = SubmitKeyMode.CtrlEnter;

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
    /// Gets or sets a value indicating whether the input is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = true;

    private Color _backgroundColor = Color.Inherit;
    private Color _foregroundColor = Color.Inherit;
    private Color _focusBackgroundColor = Color.Inherit;
    private Color _focusForegroundColor = Color.Inherit;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor
    {
        get => _focusBackgroundColor;
        set => _focusBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor
    {
        get => _focusForegroundColor;
        set => _focusForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public Color BorderColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the border color when focused.
    /// </summary>
    public Color FocusBorderColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the placeholder text color.
    /// </summary>
    public Color PlaceholderColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the cursor color.
    /// </summary>
    public Color CursorColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets a value indicating whether the input is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the background color when disabled.
    /// If null, the normal background color is used.
    /// </summary>
    public Color? DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color when disabled.
    /// </summary>
    public Color DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets a value indicating whether the input can receive focus.
    /// When Disabled is true, the input cannot receive focus regardless of this value.
    /// </summary>
    public bool CanFocus { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets a value indicating whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

    /// <summary>
    /// Occurs when the user presses Enter.
    /// </summary>
    public event EventHandler<string>? EnterPressed;

    /// <summary>
    /// Occurs when the user presses Escape.
    /// </summary>
    public event EventHandler? EscapePressed;

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

        // Use ComputedWidth/Height when set by the Renderer/StackPanel layout.
        // Fall back to CalculateSize when no computed value is available.
        int computedW = ((IWidget)this).ComputedWidth;
        int width = computedW > 0 ? computedW : CalculateSize(Width, ((IWidget)this).Parent, true);

        int computedH = ((IWidget)this).ComputedHeight;
        int height = computedH > 0 ? computedH
            : (Multiline ? CalculateSize(Height, ((IWidget)this).Parent, false) : 1);

        // Store computed values
        ((IWidget)this).ComputedWidth = width;
        ((IWidget)this).ComputedHeight = height;

        if (width <= 0 || height <= 0) return [];

        // Temporarily override colors if disabled
        Color originalBg = _backgroundColor;
        Color originalFg = _foregroundColor;
        Color originalFocusBg = _focusBackgroundColor;
        Color originalFocusFg = _focusForegroundColor;

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
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            EscapePressed?.Invoke(this, EventArgs.Empty);
            return;
        }
        else if (keyInfo.Key == ConsoleKey.Enter)
        {
            bool hasCtrl = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;

            if (!Multiline)
            {
                // Singleline always submits on Enter
                OnSubmit();
            }
            else if (SubmitKey == SubmitKeyMode.CtrlEnter)
            {
                // Editor-style: Enter = newline, Ctrl+Enter = submit
                if (hasCtrl)
                    OnSubmit();
                else
                    InsertChar('\n');
            }
            else
            {
                // Chat-style (default): Enter = submit, Ctrl+Enter = newline
                if (hasCtrl)
                    InsertChar('\n');
                else
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

        if (size.Equals("fill", StringComparison.OrdinalIgnoreCase))
        {
            if (parent == null)
                return isWidth ? Console.WindowWidth : Console.WindowHeight;
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
            cursorDisplayPos += Text.GetRuneDisplayWidth(runes[i]);
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
                accumulatedWidth += Text.GetRuneDisplayWidth(runes[i]);
            }
        }

        // Render visible portion of text
        int displayX = 0;
        int runeIdx = scrollOffsetRunes;

        while (runeIdx < runes.Count && displayX < width)
        {
            Rune rune = runes[runeIdx];
            int runeWidth = Text.GetRuneDisplayWidth(rune);

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
                int runeWidth = Text.GetRuneDisplayWidth(rune);

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
                int runeWidth = Text.GetRuneDisplayWidth(rune);

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
            SubmitKey = SubmitKey,
            IsPassword = IsPassword,
            Placeholder = Placeholder,
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

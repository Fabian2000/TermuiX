using System.Text;

namespace Termui.Widgets;

public class Input : IWidget
{
    private string _text = string.Empty;
    private int _cursorPosition = 0;
    private bool _cursorVisible = false;
    private DateTime _lastCursorBlink = DateTime.Now;
    private const int CursorBlinkIntervalMs = 500;
    private int _scrollOffsetY = 0; // Vertical scroll for multiline

    public string Value
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = Math.Min(_cursorPosition, _text.Length);
        }
    }

    public bool Multiline { get; set; } = false;
    public bool IsPassword { get; set; } = false;
    public string Placeholder { get; set; } = string.Empty;

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "30ch";
    public string Height { get; set; } = "3ch";
    public string PaddingLeft { get; set; } = "1ch";
    public string PaddingTop { get; set; } = "0ch";
    public string PaddingRight { get; set; } = "1ch";
    public string PaddingBottom { get; set; } = "0ch";
    public string PositionX { get; set; } = "0ch";
    public string PositionY { get; set; } = "0ch";
    public bool Visible { get; set; } = true;
    public bool AllowWrapping { get; set; } = true;

    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    public ConsoleColor BorderColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusBorderColor { get; set; } = ConsoleColor.White;
    public ConsoleColor PlaceholderColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor CursorColor { get; set; } = ConsoleColor.White;

    public bool CanFocus => true;
    public bool Scrollable => false;

    // Events
    public event EventHandler<string>? Submit;
    public event EventHandler<string>? Changed;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    char[][] IWidget.GetRaw()
    {
        // Update cursor blink
        if (((IWidget)this).Focussed)
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
        int width = CalculateWidth();
        int height = Multiline ? CalculateHeight() : 1;

        if (width <= 0 || height <= 0) return [];

        // Create output
        var result = new char[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new char[width];
            Array.Fill(result[i], ' ');
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
            result[0][0] = '█';
        }

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
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
                _text = _text.Remove(_cursorPosition - 1, 1);
                _cursorPosition--;
                OnChanged();
            }
        }
        else if (keyInfo.Key == ConsoleKey.Delete)
        {
            if (_cursorPosition < _text.Length)
            {
                _text = _text.Remove(_cursorPosition, 1);
                OnChanged();
            }
        }
        else if (keyInfo.Key == ConsoleKey.LeftArrow)
        {
            _cursorPosition = Math.Max(0, _cursorPosition - 1);
        }
        else if (keyInfo.Key == ConsoleKey.RightArrow)
        {
            _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
        }
        else if (keyInfo.Key == ConsoleKey.Home)
        {
            _cursorPosition = 0;
        }
        else if (keyInfo.Key == ConsoleKey.End)
        {
            _cursorPosition = _text.Length;
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
            // Regular character input
            InsertChar(keyInfo.KeyChar);
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

        _text = _text.Insert(_cursorPosition, c.ToString());
        _cursorPosition++;
        OnChanged();
    }

    private void InsertText(string text)
    {
        _text = _text.Insert(_cursorPosition, text);
        _cursorPosition += text.Length;
        OnChanged();
    }

    private void MoveCursorVertical(int direction)
    {
        if (string.IsNullOrEmpty(_text)) return;

        var lines = _text.Split('\n');

        // Find current line and column
        int currentLine = 0;
        int currentCol = 0;
        int charsSoFar = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (charsSoFar + lines[i].Length >= _cursorPosition)
            {
                currentLine = i;
                currentCol = _cursorPosition - charsSoFar;
                break;
            }
            charsSoFar += lines[i].Length + 1; // +1 for newline
        }

        // Calculate target line
        int targetLine = currentLine + direction;
        if (targetLine < 0 || targetLine >= lines.Length)
            return; // Out of bounds

        // Calculate new cursor position
        int newPosition = 0;
        for (int i = 0; i < targetLine; i++)
        {
            newPosition += lines[i].Length + 1; // +1 for newline
        }

        // Keep same column, or end of line if shorter
        int targetCol = Math.Min(currentCol, lines[targetLine].Length);
        newPosition += targetCol;

        _cursorPosition = Math.Min(newPosition, _text.Length);
    }

    private string? GetClipboardText()
    {
        // Try to get clipboard text using various methods
        try
        {
            // For Linux with xclip
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
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

    protected virtual void OnSubmit()
    {
        Submit?.Invoke(this, _text);
    }

    protected virtual void OnChanged()
    {
        Changed?.Invoke(this, _text);
    }

    private int CalculateWidth()
    {
        if (Width.EndsWith("ch"))
        {
            var value = Width[..^2].Trim();
            if (int.TryParse(value, out int result))
                return result;
        }
        return 30; // Default
    }

    private int CalculateHeight()
    {
        if (Height.EndsWith("ch"))
        {
            var value = Height[..^2].Trim();
            if (int.TryParse(value, out int result))
                return result;
        }
        return 3; // Default
    }

    private void RenderSingleLine(char[][] result, string displayText, int width)
    {
        // Calculate horizontal scroll offset to keep cursor visible
        int scrollOffset = 0;
        if (_cursorPosition >= width)
        {
            // Cursor is beyond visible area, scroll left
            scrollOffset = _cursorPosition - width + 1;
        }

        // Render visible portion of text
        for (int x = 0; x < width && scrollOffset + x < displayText.Length; x++)
        {
            int textPos = scrollOffset + x;
            if (textPos == _cursorPosition && _cursorVisible && ((IWidget)this).Focussed)
            {
                result[0][x] = '█'; // Cursor block
            }
            else
            {
                result[0][x] = displayText[textPos];
            }
        }

        // Cursor at end of text (within visible area)
        int cursorDisplayX = _cursorPosition - scrollOffset;
        if (_cursorPosition == displayText.Length && cursorDisplayX >= 0 && cursorDisplayX < width && _cursorVisible && ((IWidget)this).Focussed)
        {
            result[0][cursorDisplayX] = '█';
        }
    }

    private void RenderMultiline(char[][] result, string displayText, int width, int height)
    {
        // Wrap text to fit width
        var wrappedLines = new List<string>();
        var lines = displayText.Split('\n');

        foreach (var line in lines)
        {
            if (line.Length <= width)
            {
                wrappedLines.Add(line);
            }
            else
            {
                // Break long lines into chunks
                for (int i = 0; i < line.Length; i += width)
                {
                    int len = Math.Min(width, line.Length - i);
                    wrappedLines.Add(line.Substring(i, len));
                }
            }
        }

        // Find cursor position in wrapped lines
        int cursorLine = 0;
        int cursorCol = 0;
        int charCount = 0;

        for (int i = 0; i < wrappedLines.Count; i++)
        {
            int lineLength = wrappedLines[i].Length;
            if (charCount + lineLength >= _cursorPosition)
            {
                cursorLine = i;
                cursorCol = _cursorPosition - charCount;
                break;
            }
            charCount += lineLength;
            // Account for original newlines
            if (i < wrappedLines.Count - 1 && displayText.IndexOf('\n', charCount) == charCount)
            {
                charCount++;
            }
        }

        // Adjust vertical scroll to keep cursor visible
        if (cursorLine < _scrollOffsetY)
        {
            // Cursor above visible area, scroll up
            _scrollOffsetY = cursorLine;
        }
        else if (cursorLine >= _scrollOffsetY + height)
        {
            // Cursor below visible area, scroll down
            _scrollOffsetY = cursorLine - height + 1;
        }

        // Render visible lines with scroll offset
        for (int displayLineIdx = 0; displayLineIdx < height; displayLineIdx++)
        {
            int sourceLineIdx = _scrollOffsetY + displayLineIdx;
            if (sourceLineIdx >= wrappedLines.Count) break;

            string line = wrappedLines[sourceLineIdx];
            for (int x = 0; x < Math.Min(line.Length, width); x++)
            {
                if (sourceLineIdx == cursorLine && x == cursorCol && _cursorVisible && ((IWidget)this).Focussed)
                {
                    result[displayLineIdx][x] = '█'; // Cursor block
                }
                else
                {
                    result[displayLineIdx][x] = line[x];
                }
            }

            // Cursor at end of line
            if (sourceLineIdx == cursorLine && cursorCol == line.Length && cursorCol < width && _cursorVisible && ((IWidget)this).Focussed)
            {
                result[displayLineIdx][cursorCol] = '█';
            }
        }
    }
}

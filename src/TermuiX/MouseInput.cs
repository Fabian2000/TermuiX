using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TermuiX;

/// <summary>
/// Provides cross-platform mouse input reading for console applications.
/// Windows: P/Invoke with ReadConsoleInput.
/// Unix/macOS: ANSI SGR mouse reporting escape sequences.
/// </summary>
internal static partial class MouseInput
{
    private static bool _enabled;

    /// <summary>
    /// Enables mouse input reporting for the current console.
    /// </summary>
    internal static void Enable()
    {
        if (_enabled) return;

        if (OperatingSystem.IsWindows())
        {
            EnableWindows();
        }
        else
        {
            EnableUnix();
        }

        _enabled = true;
    }

    /// <summary>
    /// Disables mouse input reporting and restores the console.
    /// </summary>
    internal static void Disable()
    {
        if (!_enabled) return;

        if (OperatingSystem.IsWindows())
        {
            DisableWindows();
        }
        else
        {
            DisableUnix();
        }

        _enabled = false;
    }

    /// <summary>
    /// Tries to read a mouse or keyboard event without blocking.
    /// </summary>
    /// <param name="mouseEvent">The mouse event if one was available.</param>
    /// <param name="keyEvent">The key event if a keyboard event was read instead.</param>
    /// <returns>1 if mouse event, 2 if key event, 0 if nothing available.</returns>
    internal static int TryRead(out MouseEventArgs mouseEvent, out ConsoleKeyInfo keyEvent)
    {
        mouseEvent = default;
        keyEvent = default;

        if (!_enabled) return 0;

        if (OperatingSystem.IsWindows())
        {
            return TryReadWindows(out mouseEvent, out keyEvent);
        }
        else
        {
            return TryReadUnix(out mouseEvent, out keyEvent);
        }
    }

    // ── Windows implementation ──────────────────────────────────────

    private static nint _windowsConsoleHandle;
    private static uint _windowsOriginalMode;

    [SupportedOSPlatform("windows")]
    private static void EnableWindows()
    {
        _windowsConsoleHandle = GetStdHandle(STD_INPUT_HANDLE);
        GetConsoleMode(_windowsConsoleHandle, out _windowsOriginalMode);

        uint mode = _windowsOriginalMode;
        mode |= ENABLE_MOUSE_INPUT;
        mode |= ENABLE_EXTENDED_FLAGS;
        mode &= ~ENABLE_QUICK_EDIT_MODE;
        SetConsoleMode(_windowsConsoleHandle, mode);
    }

    [SupportedOSPlatform("windows")]
    private static void DisableWindows()
    {
        if (_windowsConsoleHandle != 0)
        {
            SetConsoleMode(_windowsConsoleHandle, _windowsOriginalMode);
            _windowsConsoleHandle = 0;
        }
    }

    [SupportedOSPlatform("windows")]
    private static int TryReadWindows(out MouseEventArgs mouseEvent, out ConsoleKeyInfo keyEvent)
    {
        mouseEvent = default;
        keyEvent = default;

        if (!GetNumberOfConsoleInputEvents(_windowsConsoleHandle, out uint numEvents) || numEvents == 0)
            return 0;

        INPUT_RECORD record;
        unsafe
        {
            if (!ReadConsoleInput(_windowsConsoleHandle, &record, 1, out uint eventsRead) || eventsRead == 0)
                return 0;
        }

        if (record.EventType == MOUSE_EVENT)
        {
            ref var mouse = ref record.MouseEvent;
            MouseEventType? eventType = null;
            bool isShift = (mouse.ControlKeyState & SHIFT_PRESSED) != 0;

            if (mouse.EventFlags == 0) // Button pressed or released
            {
                if ((mouse.ButtonState & FROM_LEFT_1ST_BUTTON_PRESSED) != 0)
                    eventType = MouseEventType.LeftButtonPressed;
                else if ((mouse.ButtonState & RIGHTMOST_BUTTON_PRESSED) != 0)
                    eventType = MouseEventType.RightButtonPressed;
                else
                    eventType = MouseEventType.LeftButtonReleased;
            }
            else if (mouse.EventFlags == MOUSE_WHEELED)
            {
                int wheelDelta = (int)(mouse.ButtonState >> 16);
                eventType = wheelDelta > 0 ? MouseEventType.WheelUp : MouseEventType.WheelDown;
            }

            if (eventType.HasValue)
            {
                mouseEvent = new MouseEventArgs
                {
                    X = mouse.MousePosition.X,
                    Y = mouse.MousePosition.Y,
                    EventType = eventType.Value,
                    Shift = isShift
                };
                return 1;
            }
            return 0;
        }

        if (record.EventType == KEY_EVENT && record.KeyEvent.KeyDown)
        {
            ref var key = ref record.KeyEvent;
            var modifiers = (ConsoleModifiers)0;
            if ((key.ControlKeyState & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0)
                modifiers |= ConsoleModifiers.Control;
            if ((key.ControlKeyState & SHIFT_PRESSED) != 0)
                modifiers |= ConsoleModifiers.Shift;
            if ((key.ControlKeyState & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0)
                modifiers |= ConsoleModifiers.Alt;

            keyEvent = new ConsoleKeyInfo(
                key.UnicodeChar,
                (ConsoleKey)key.VirtualKeyCode,
                modifiers.HasFlag(ConsoleModifiers.Shift),
                modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control)
            );
            return 2;
        }

        return 0;
    }

    // ── Windows P/Invoke constants ──────────────────────────────────

    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_MOUSE_INPUT = 0x0010;
    private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
    private const ushort MOUSE_EVENT = 0x0002;
    private const ushort KEY_EVENT = 0x0001;
    private const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;
    private const uint RIGHTMOST_BUTTON_PRESSED = 0x0002;
    private const uint MOUSE_WHEELED = 0x0004;
    private const uint LEFT_CTRL_PRESSED = 0x0008;
    private const uint RIGHT_CTRL_PRESSED = 0x0004;
    private const uint SHIFT_PRESSED = 0x0010;
    private const uint LEFT_ALT_PRESSED = 0x0002;
    private const uint RIGHT_ALT_PRESSED = 0x0001;

    // ── Windows P/Invoke structs ────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSE_EVENT_RECORD
    {
        public COORD MousePosition;
        public uint ButtonState;
        public uint ControlKeyState;
        public uint EventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEY_EVENT_RECORD
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool KeyDown;
        public ushort RepeatCount;
        public ushort VirtualKeyCode;
        public ushort VirtualScanCode;
        public char UnicodeChar;
        public uint ControlKeyState;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUT_RECORD
    {
        [FieldOffset(0)] public ushort EventType;
        [FieldOffset(4)] public MOUSE_EVENT_RECORD MouseEvent;
        [FieldOffset(4)] public KEY_EVENT_RECORD KeyEvent;
    }

    // ── Windows P/Invoke methods ────────────────────────────────────

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumberOfConsoleInputEvents(nint hConsoleHandle, out uint lpcNumberOfEvents);

    [LibraryImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool ReadConsoleInput(
        nint hConsoleHandle,
        INPUT_RECORD* lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    // ── Unix/macOS implementation ───────────────────────────────────

    // Circular-ish byte buffer for Unix stdin parsing.
    // Instead of List.RemoveAt(0) which is O(n), we use an array with a read offset.
    // When all bytes are consumed, we reset. When space runs low, we compact.
    private static byte[] _unixBuf = new byte[256];
    private static int _unixBufStart; // read position
    private static int _unixBufEnd;   // write position (exclusive)
    private static char _pendingLowSurrogate; // for 4-byte UTF-8 emoji split across two TryRead calls

    private static int UnixBufCount => _unixBufEnd - _unixBufStart;

    private static unsafe void UnixBufAdd(byte* src, int count)
    {
        // Compact if we'd overflow
        if (_unixBufEnd + count > _unixBuf.Length)
        {
            int existing = UnixBufCount;
            if (existing > 0)
                Buffer.BlockCopy(_unixBuf, _unixBufStart, _unixBuf, 0, existing);
            _unixBufStart = 0;
            _unixBufEnd = existing;

            // Grow if still not enough space
            if (_unixBufEnd + count > _unixBuf.Length)
            {
                Array.Resize(ref _unixBuf, Math.Max(_unixBuf.Length * 2, _unixBufEnd + count));
            }
        }
        for (int i = 0; i < count; i++)
            _unixBuf[_unixBufEnd + i] = src[i];
        _unixBufEnd += count;
    }

    private static void UnixBufConsume(int count)
    {
        _unixBufStart += count;
        if (_unixBufStart >= _unixBufEnd)
        {
            // Buffer fully consumed — reset positions to avoid drift
            _unixBufStart = 0;
            _unixBufEnd = 0;
        }
    }

    private static void UnixBufClear()
    {
        _unixBufStart = 0;
        _unixBufEnd = 0;
    }

    // POSIX poll() to check for available data without blocking or modifying fd flags.
    // Unlike O_NONBLOCK (which affects the shared open file description and breaks .NET Console),
    // poll() only inspects readiness without side effects.
    [UnsupportedOSPlatform("windows")]
    [LibraryImport("libc", EntryPoint = "poll", SetLastError = true)]
    private static unsafe partial int Poll(PollFd* fds, nint nfds, int timeout);

    [UnsupportedOSPlatform("windows")]
    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    private static unsafe partial nint PosixRead(int fd, byte* buf, nint count);

    // We need tcgetattr/tcsetattr to put terminal in raw mode so read() returns bytes immediately.
    // Without raw mode, the terminal line discipline buffers input until Enter is pressed,
    // which means mouse escape sequences would not be delivered byte-by-byte.
    [UnsupportedOSPlatform("windows")]
    [LibraryImport("libc", EntryPoint = "tcgetattr", SetLastError = true)]
    private static unsafe partial int Tcgetattr(int fd, Termios* termios);

    [UnsupportedOSPlatform("windows")]
    [LibraryImport("libc", EntryPoint = "tcsetattr", SetLastError = true)]
    private static unsafe partial int Tcsetattr(int fd, int optionalActions, Termios* termios);

    private const int TCSANOW = 0;

    // Termios struct - using fixed-size arrays for portability
    // The actual layout varies between Linux and macOS, but the fields we care about
    // (c_iflag, c_oflag, c_cflag, c_lflag) are at the same offsets on both.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
        // cc_t c_cc[NCCS] + speed_t fields - we just need enough space
        public fixed byte _rest[128];
    }

    // termios flags
    private const uint ICANON = 0x0002; // Canonical mode (line buffering)
    private const uint ECHO = 0x0008;   // Echo input
    private const uint ISIG = 0x0001;   // Signal generation (Ctrl+C etc.)

    private static unsafe Termios _originalTermios;
    private static bool _termiosModified;

    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd
    {
        public int fd;
        public short events;
        public short revents;
    }

    private const short POLLIN = 0x0001;

    [UnsupportedOSPlatform("windows")]
    private static void EnableUnix()
    {
        // Put terminal in raw-ish mode so we get bytes immediately (no line buffering).
        // We keep ISIG so Ctrl+C still sends SIGINT.
        unsafe
        {
            Termios raw;
            if (Tcgetattr(0, &raw) == 0)
            {
                _originalTermios = raw;
                _termiosModified = true;

                raw.c_lflag &= ~ICANON; // Disable canonical mode (no line buffering)
                raw.c_lflag &= ~ECHO;   // Disable echo (terminal already handles display)

                Tcsetattr(0, TCSANOW, &raw);
            }
        }

        // 1003 = Any event tracking (press/release/wheel + all mouse movement)
        // 1006 = SGR extended coordinates (supports large terminals, distinguishes press vs release)
        Console.Write("\x1b[?1003h\x1b[?1006h");
        Console.Out.Flush();
    }

    [UnsupportedOSPlatform("windows")]
    private static void DisableUnix()
    {
        Console.Write("\x1b[?1006l\x1b[?1003l");
        Console.Out.Flush();

        // Restore original terminal settings
        unsafe
        {
            if (_termiosModified)
            {
                fixed (Termios* p = &_originalTermios)
                {
                    Tcsetattr(0, TCSANOW, p);
                }
                _termiosModified = false;
            }
        }

        UnixBufClear();
    }

    // Helper: read byte at offset from start of logical buffer
    private static byte BufAt(int index) => _unixBuf[_unixBufStart + index];

    // Helper: parse an integer directly from the byte buffer (avoids string allocation)
    private static bool TryParseIntFromBuf(int from, int to, out int result)
    {
        result = 0;
        if (from >= to) return false;
        for (int i = from; i < to; i++)
        {
            byte b = _unixBuf[_unixBufStart + i];
            if (b < (byte)'0' || b > (byte)'9') return false;
            result = result * 10 + (b - '0');
        }
        return true;
    }

    [UnsupportedOSPlatform("windows")]
    private static int TryReadUnix(out MouseEventArgs mouseEvent, out ConsoleKeyInfo keyEvent)
    {
        mouseEvent = default;
        keyEvent = default;

        // Return pending low surrogate from a previous 4-byte UTF-8 decode
        if (_pendingLowSurrogate != 0)
        {
            keyEvent = new ConsoleKeyInfo(_pendingLowSurrogate, 0, false, false, false);
            _pendingLowSurrogate = (char)0;
            return 2;
        }

        // Read available bytes from stdin using poll() + read().
        // poll() is a pure query — it doesn't change fd state.
        unsafe
        {
            PollFd pfd;
            pfd.fd = 0;
            pfd.events = POLLIN;
            pfd.revents = 0;

            byte* buf = stackalloc byte[64];
            while (Poll(&pfd, 1, 0) > 0 && (pfd.revents & POLLIN) != 0)
            {
                nint bytesRead = PosixRead(0, buf, 64);
                if (bytesRead > 0)
                {
                    UnixBufAdd(buf, (int)bytesRead);
                }
                else
                {
                    break;
                }

                pfd.revents = 0;
            }
        }

        int count = UnixBufCount;
        if (count == 0)
            return 0;

        // Check for SGR mouse sequence: ESC [ < Cb ; Cx ; Cy M/m
        if (count >= 3 &&
            BufAt(0) == 0x1b &&
            BufAt(1) == (byte)'[' &&
            BufAt(2) == (byte)'<')
        {
            // Find terminator (M = press, m = release)
            int terminatorIndex = -1;
            for (int i = 3; i < count; i++)
            {
                byte b = BufAt(i);
                if (b == (byte)'M' || b == (byte)'m')
                {
                    terminatorIndex = i;
                    break;
                }
            }

            if (terminatorIndex == -1)
            {
                // Incomplete sequence - if buffer is getting too large, discard it
                if (count > 32)
                    UnixBufClear();
                return 0;
            }

            char terminator = (char)BufAt(terminatorIndex);

            // Parse the three semicolon-separated integers directly from buffer (no string allocation)
            // Format: <Cb;Cx;Cy where indices 3..terminatorIndex-1 contain "Cb;Cx;Cy"
            int paramStart = 3;
            int paramEnd = terminatorIndex;

            // Find first and second semicolons
            int semi1 = -1, semi2 = -1;
            for (int i = paramStart; i < paramEnd; i++)
            {
                if (BufAt(i) == (byte)';')
                {
                    if (semi1 == -1) semi1 = i;
                    else { semi2 = i; break; }
                }
            }

            // Parse integers BEFORE consuming the buffer
            if (semi1 == -1 || semi2 == -1 ||
                !TryParseIntFromBuf(paramStart, semi1, out int cb) ||
                !TryParseIntFromBuf(semi1 + 1, semi2, out int cx) ||
                !TryParseIntFromBuf(semi2 + 1, paramEnd, out int cy))
            {
                UnixBufConsume(terminatorIndex + 1);
                return 0;
            }

            // Now consume the sequence
            UnixBufConsume(terminatorIndex + 1);

            bool isPress = terminator == 'M';
            bool isWheel = (cb & 64) != 0;
            bool isMotion = (cb & 32) != 0;
            bool isShift = (cb & 4) != 0;
            int button = cb & 0x03;

            int x = cx - 1;
            int y = cy - 1;

            MouseEventType? eventType = null;

            if (isMotion && !isWheel)
            {
                eventType = MouseEventType.Moved;
            }
            else if (isWheel)
            {
                eventType = button == 0 ? MouseEventType.WheelUp : MouseEventType.WheelDown;
            }
            else if (isPress)
            {
                eventType = button switch
                {
                    0 => MouseEventType.LeftButtonPressed,
                    2 => MouseEventType.RightButtonPressed,
                    _ => null
                };
            }
            else
            {
                eventType = button switch
                {
                    0 => MouseEventType.LeftButtonReleased,
                    2 => MouseEventType.RightButtonReleased,
                    _ => null
                };
            }

            if (eventType.HasValue)
            {
                mouseEvent = new MouseEventArgs
                {
                    X = x,
                    Y = y,
                    EventType = eventType.Value,
                    Shift = isShift
                };
                return 1;
            }

            return 0;
        }

        // Check for other ESC sequences (arrow keys, function keys, etc.)
        if (BufAt(0) == 0x1b)
        {
            if (count >= 3 && BufAt(1) == (byte)'[')
            {
                byte code = BufAt(2);
                UnixBufConsume(3);

                ConsoleKey arrowKey = code switch
                {
                    (byte)'A' => ConsoleKey.UpArrow,
                    (byte)'B' => ConsoleKey.DownArrow,
                    (byte)'C' => ConsoleKey.RightArrow,
                    (byte)'D' => ConsoleKey.LeftArrow,
                    (byte)'H' => ConsoleKey.Home,
                    (byte)'F' => ConsoleKey.End,
                    (byte)'Z' => ConsoleKey.Tab, // Shift+Tab (CSI Z / reverse tab)
                    _ => 0
                };

                if (arrowKey != 0)
                {
                    bool isShift = code == (byte)'Z';
                    keyEvent = new ConsoleKeyInfo('\0', arrowKey, isShift, false, false);
                    return 2;
                }

                // Extended sequences: ESC [ number ... (digits, semicolons, then a final letter or ~)
                if (code >= (byte)'0' && code <= (byte)'9')
                {
                    // Collect remaining parameter bytes and find the terminator
                    // Format examples:
                    //   ESC [ 5 ~           → PageUp
                    //   ESC [ 1 ; 2 D       → Shift+LeftArrow  (modifier=2)
                    //   ESC [ 1 ; 5 C       → Ctrl+RightArrow  (modifier=5)
                    int paramBuf = code - (byte)'0';
                    int modifier = 0;
                    bool inSecondParam = false;
                    byte finalByte = 0;

                    while (UnixBufCount > 0)
                    {
                        byte b = BufAt(0);
                        UnixBufConsume(1);

                        if (b == (byte)';')
                        {
                            inSecondParam = true;
                            modifier = 0;
                        }
                        else if (b >= (byte)'0' && b <= (byte)'9')
                        {
                            if (inSecondParam)
                                modifier = modifier * 10 + (b - (byte)'0');
                            else
                                paramBuf = paramBuf * 10 + (b - (byte)'0');
                        }
                        else
                        {
                            finalByte = b;
                            break;
                        }
                    }

                    bool isShiftMod = (modifier & 0x01) != 0;  // modifier 2 = shift (bit 0 of modifier-1)

                    if (finalByte == (byte)'~')
                    {
                        // Tilde-terminated: PageUp/Down, Insert, Delete, Home, End
                        ConsoleKey extKey = paramBuf switch
                        {
                            5 => ConsoleKey.PageUp,
                            6 => ConsoleKey.PageDown,
                            2 => ConsoleKey.Insert,
                            3 => ConsoleKey.Delete,
                            1 => ConsoleKey.Home,
                            4 => ConsoleKey.End,
                            _ => 0
                        };

                        if (extKey != 0)
                        {
                            keyEvent = new ConsoleKeyInfo('\0', extKey, isShiftMod, false, false);
                            return 2;
                        }
                    }
                    else if (finalByte >= (byte)'A' && finalByte <= (byte)'Z')
                    {
                        // Letter-terminated: modified arrow keys (ESC [ 1 ; modifier A/B/C/D)
                        ConsoleKey modArrowKey = finalByte switch
                        {
                            (byte)'A' => ConsoleKey.UpArrow,
                            (byte)'B' => ConsoleKey.DownArrow,
                            (byte)'C' => ConsoleKey.RightArrow,
                            (byte)'D' => ConsoleKey.LeftArrow,
                            (byte)'H' => ConsoleKey.Home,
                            (byte)'F' => ConsoleKey.End,
                            (byte)'Z' => ConsoleKey.Tab,
                            _ => 0
                        };

                        if (modArrowKey != 0)
                        {
                            bool isShift = isShiftMod || finalByte == (byte)'Z';
                            keyEvent = new ConsoleKeyInfo('\0', modArrowKey, isShift, false, false);
                            return 2;
                        }
                    }

                    // Unknown extended sequence — already consumed, just discard
                    return 0;
                }

                // Unknown CSI sequence - discard remaining until we find a letter
                while (UnixBufCount > 0 && BufAt(0) < (byte)'@')
                    UnixBufConsume(1);
                if (UnixBufCount > 0)
                    UnixBufConsume(1); // Remove the final letter

                return 0;
            }

            if (count >= 2 && BufAt(1) == (byte)'O')
            {
                if (count >= 3)
                {
                    byte ssCode = BufAt(2);
                    UnixBufConsume(3);

                    // SS3 (ESC O) sequences – Application Mode arrow keys & Home/End
                    ConsoleKey ss3Key = ssCode switch
                    {
                        (byte)'A' => ConsoleKey.UpArrow,
                        (byte)'B' => ConsoleKey.DownArrow,
                        (byte)'C' => ConsoleKey.RightArrow,
                        (byte)'D' => ConsoleKey.LeftArrow,
                        (byte)'H' => ConsoleKey.Home,
                        (byte)'F' => ConsoleKey.End,
                        _ => 0
                    };

                    if (ss3Key != 0)
                    {
                        keyEvent = new ConsoleKeyInfo('\0', ss3Key, false, false, false);
                        return 2;
                    }

                    return 0;
                }
                return 0;
            }

            if (count == 1)
            {
                UnixBufConsume(1);
                keyEvent = new ConsoleKeyInfo('\x1b', ConsoleKey.Escape, false, false, false);
                return 2;
            }

            UnixBufConsume(1);
            return 0;
        }

        // Regular character (not an escape sequence)
        byte ch = BufAt(0);

        // Decode UTF-8 multi-byte sequences (é = 0xC3 0xA9, emoji = up to 4 bytes)
        int seqLen = ch switch
        {
            < 0x80 => 1,                    // ASCII
            >= 0xC0 and < 0xE0 => 2,        // 2-byte (U+0080..U+07FF)
            >= 0xE0 and < 0xF0 => 3,        // 3-byte (U+0800..U+FFFF)
            >= 0xF0 and < 0xF8 => 4,        // 4-byte (U+10000..U+10FFFF)
            _ => 1                           // Invalid lead byte, treat as single
        };

        if (seqLen > 1)
        {
            if (UnixBufCount < seqLen)
                return 0; // Incomplete UTF-8 sequence, wait for more bytes

            // Decode UTF-8 to a Unicode codepoint
            int codepoint = seqLen switch
            {
                2 => ((BufAt(0) & 0x1F) << 6) | (BufAt(1) & 0x3F),
                3 => ((BufAt(0) & 0x0F) << 12) | ((BufAt(1) & 0x3F) << 6) | (BufAt(2) & 0x3F),
                4 => ((BufAt(0) & 0x07) << 18) | ((BufAt(1) & 0x3F) << 12) | ((BufAt(2) & 0x3F) << 6) | (BufAt(3) & 0x3F),
                _ => ch
            };

            UnixBufConsume(seqLen);

            // Convert to string for ConsoleKeyInfo (may be surrogate pair for emoji)
            string s = char.ConvertFromUtf32(codepoint);
            if (s.Length == 2)
            {
                // Surrogate pair: return high surrogate now, queue low surrogate for next call
                _pendingLowSurrogate = s[1];
            }
            keyEvent = new ConsoleKeyInfo(s[0], 0, false, false, false);
            return 2;
        }

        UnixBufConsume(1);

        char character = (char)ch;
        ConsoleKey consoleKey = character switch
        {
            '\r' or '\n' => ConsoleKey.Enter,
            '\t' => ConsoleKey.Tab,
            ' ' => ConsoleKey.Spacebar,
            '\x7f' => ConsoleKey.Backspace,
            '\b' => ConsoleKey.Backspace,
            _ when character >= 'a' && character <= 'z' => ConsoleKey.A + (character - 'a'),
            _ when character >= 'A' && character <= 'Z' => ConsoleKey.A + (character - 'A'),
            _ when character >= '0' && character <= '9' => ConsoleKey.D0 + (character - '0'),
            _ => 0
        };

        bool shift = character >= 'A' && character <= 'Z';

        bool control = false;
        if (ch >= 1 && ch <= 26 && ch != 0x09 && ch != 0x0A && ch != 0x0D && ch != 0x08)
        {
            control = true;
            consoleKey = ConsoleKey.A + (ch - 1);
            character = (char)(ch + 'a' - 1);
        }

        keyEvent = new ConsoleKeyInfo(character, consoleKey, shift, false, control);
        return 2;
    }
}

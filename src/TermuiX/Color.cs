namespace TermuiX;

/// <summary>
/// Represents a color that can be either a standard 16-color ConsoleColor or a 24-bit TrueColor RGB value.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>
    /// The raw packed value. For ConsoleColor: bits [0..3] = color index, bit 31 = 0.
    /// For RGB: bits [0..7] = B, [8..15] = G, [16..23] = R, bit 31 = 1.
    /// </summary>
    private readonly uint _value;

    private const uint RgbFlag = 0x8000_0000u;
    private const uint InheritFlag = 0x4000_0000u;

    /// <summary>
    /// Gets whether this color is an RGB TrueColor value.
    /// </summary>
    public bool IsRgb => (_value & RgbFlag) != 0;

    /// <summary>
    /// Gets whether this color inherits from its parent widget.
    /// </summary>
    public bool IsInherit => (_value & InheritFlag) != 0;

    /// <summary>
    /// Gets the red component (0-255). Only valid when IsRgb is true.
    /// </summary>
    public byte R => (byte)((_value >> 16) & 0xFF);

    /// <summary>
    /// Gets the green component (0-255). Only valid when IsRgb is true.
    /// </summary>
    public byte G => (byte)((_value >> 8) & 0xFF);

    /// <summary>
    /// Gets the blue component (0-255). Only valid when IsRgb is true.
    /// </summary>
    public byte B => (byte)(_value & 0xFF);

    /// <summary>
    /// Gets the ConsoleColor value. Only valid when IsRgb is false.
    /// </summary>
    public ConsoleColor ConsoleColor => (ConsoleColor)(_value & 0xF);

    private Color(uint value) => _value = value;

    /// <summary>
    /// Creates a Color from a ConsoleColor.
    /// </summary>
    public Color(ConsoleColor color) => _value = (uint)color;

    /// <summary>
    /// Creates a Color from RGB components.
    /// </summary>
    public Color(byte r, byte g, byte b) => _value = RgbFlag | ((uint)r << 16) | ((uint)g << 8) | b;

    /// <summary>
    /// Implicit conversion from ConsoleColor.
    /// </summary>
    public static implicit operator Color(ConsoleColor color) => new(color);

    /// <summary>
    /// Parses a color string. Supports ConsoleColor names (e.g. "Red"), hex (e.g. "#FF5500"),
    /// and rgb() syntax (e.g. "rgb(255,85,0)").
    /// </summary>
    public static Color Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Color(ConsoleColor.Black);

        value = value.Trim();

        // Hex: #RGB, #RRGGBB
        if (value.StartsWith('#'))
        {
            var hex = value.AsSpan(1);
            if (hex.Length == 3)
            {
                byte r = (byte)(ParseHexDigit(hex[0]) * 17);
                byte g = (byte)(ParseHexDigit(hex[1]) * 17);
                byte b = (byte)(ParseHexDigit(hex[2]) * 17);
                return new Color(r, g, b);
            }
            if (hex.Length == 6)
            {
                byte r = (byte)((ParseHexDigit(hex[0]) << 4) | ParseHexDigit(hex[1]));
                byte g = (byte)((ParseHexDigit(hex[2]) << 4) | ParseHexDigit(hex[3]));
                byte b = (byte)((ParseHexDigit(hex[4]) << 4) | ParseHexDigit(hex[5]));
                return new Color(r, g, b);
            }
            throw new FormatException($"Invalid hex color: {value}");
        }

        // rgb(R, G, B)
        if (value.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(')'))
        {
            var inner = value.AsSpan(4, value.Length - 5);
            int first = inner.IndexOf(',');
            if (first < 0) throw new FormatException($"Invalid rgb color: {value}");
            var rest = inner[(first + 1)..];
            int second = rest.IndexOf(',');
            if (second < 0) throw new FormatException($"Invalid rgb color: {value}");

            byte r = byte.Parse(inner[..first].Trim());
            byte g = byte.Parse(rest[..second].Trim());
            byte b = byte.Parse(rest[(second + 1)..].Trim());
            return new Color(r, g, b);
        }

        // Inherit
        if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
            return Inherit;

        // ConsoleColor name
        if (Enum.TryParse<ConsoleColor>(value, ignoreCase: true, out var cc))
            return new Color(cc);

        throw new FormatException($"Unknown color: {value}");
    }

    private static int ParseHexDigit(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => throw new FormatException($"Invalid hex digit: {c}")
    };

    public bool Equals(Color other) => _value == other._value;
    public override bool Equals(object? obj) => obj is Color c && Equals(c);
    public override int GetHashCode() => (int)_value;
    public static bool operator ==(Color left, Color right) => left._value == right._value;
    public static bool operator !=(Color left, Color right) => left._value != right._value;

    public override string ToString() => IsInherit ? "Inherit" : IsRgb ? $"#{R:X2}{G:X2}{B:X2}" : ConsoleColor.ToString();

    // Pre-defined colors matching ConsoleColor
    public static readonly Color Black = new(ConsoleColor.Black);
    public static readonly Color DarkBlue = new(ConsoleColor.DarkBlue);
    public static readonly Color DarkGreen = new(ConsoleColor.DarkGreen);
    public static readonly Color DarkCyan = new(ConsoleColor.DarkCyan);
    public static readonly Color DarkRed = new(ConsoleColor.DarkRed);
    public static readonly Color DarkMagenta = new(ConsoleColor.DarkMagenta);
    public static readonly Color DarkYellow = new(ConsoleColor.DarkYellow);
    public static readonly Color Gray = new(ConsoleColor.Gray);
    public static readonly Color DarkGray = new(ConsoleColor.DarkGray);
    public static readonly Color Blue = new(ConsoleColor.Blue);
    public static readonly Color Green = new(ConsoleColor.Green);
    public static readonly Color Cyan = new(ConsoleColor.Cyan);
    public static readonly Color Red = new(ConsoleColor.Red);
    public static readonly Color Magenta = new(ConsoleColor.Magenta);
    public static readonly Color Yellow = new(ConsoleColor.Yellow);
    public static readonly Color White = new(ConsoleColor.White);

    /// <summary>
    /// Special value indicating the color should be inherited from the parent widget.
    /// </summary>
    public static readonly Color Inherit = new(InheritFlag);
}

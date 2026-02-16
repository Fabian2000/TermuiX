namespace TermuiX.Widgets;

/// <summary>
/// Specifies the visual style of text content using ANSI SGR escape sequences.
/// </summary>
public enum TextStyle
{
    /// <summary>
    /// Normal text with no special styling.
    /// </summary>
    Normal,

    /// <summary>
    /// Bold text (ANSI SGR 1).
    /// </summary>
    Bold,

    /// <summary>
    /// Italic text (ANSI SGR 3).
    /// </summary>
    Italic,

    /// <summary>
    /// Bold and italic text (ANSI SGR 1;3).
    /// </summary>
    BoldItalic,

    /// <summary>
    /// Underlined text (ANSI SGR 4).
    /// </summary>
    Underline,

    /// <summary>
    /// Strikethrough text (ANSI SGR 9).
    /// </summary>
    Strikethrough
}

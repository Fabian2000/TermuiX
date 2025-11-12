namespace TermuiX.Widgets;

/// <summary>
/// Specifies the visual style of text content using Unicode characters.
/// </summary>
public enum TextStyle
{
    /// <summary>
    /// Normal text with no special styling.
    /// </summary>
    Normal,

    /// <summary>
    /// Bold text using Unicode Mathematical Alphanumeric Symbols.
    /// </summary>
    Bold,

    /// <summary>
    /// Italic text using Unicode Mathematical Alphanumeric Symbols.
    /// </summary>
    Italic,

    /// <summary>
    /// Bold and italic text using Unicode Mathematical Alphanumeric Symbols.
    /// </summary>
    BoldItalic,

    /// <summary>
    /// Underlined text using combining underline character (U+0332).
    /// </summary>
    Underline,

    /// <summary>
    /// Strikethrough text using combining strikethrough character (U+0336).
    /// </summary>
    Strikethrough
}

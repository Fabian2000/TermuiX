using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// Represents a cell in a table that can contain text or a widget.
/// </summary>
public class TableCell
{
    /// <summary>
    /// Gets or sets the text content of the cell.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the widget content of the cell.
    /// If set, this takes precedence over Text.
    /// </summary>
    public IWidget? Widget { get; set; }

    /// <summary>
    /// Gets or sets the text style for the cell content.
    /// </summary>
    public TextStyle Style { get; set; } = TextStyle.Normal;

    /// <summary>
    /// Gets or sets the foreground color of the cell.
    /// </summary>
    public Color? ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of the cell.
    /// </summary>
    public Color? BackgroundColor { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCell"/> class.
    /// </summary>
    public TableCell() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCell"/> class with text.
    /// </summary>
    /// <param name="text">The text content.</param>
    public TableCell(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCell"/> class with a widget.
    /// </summary>
    /// <param name="widget">The widget content.</param>
    public TableCell(IWidget widget)
    {
        Widget = widget;
    }
}

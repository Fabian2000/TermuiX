namespace TermuiX.Widgets;

/// <summary>
/// Represents a row in a table containing cells.
/// </summary>
public class TableRow
{
    /// <summary>
    /// Gets the collection of cells in this row.
    /// </summary>
    public List<TableCell> Cells { get; } = new List<TableCell>();

    /// <summary>
    /// Initializes a new instance of the <see cref="TableRow"/> class.
    /// </summary>
    public TableRow() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableRow"/> class with cells.
    /// </summary>
    /// <param name="cells">The cells to add to the row.</param>
    public TableRow(params TableCell[] cells)
    {
        Cells.AddRange(cells);
    }

    /// <summary>
    /// Adds a cell to the row.
    /// </summary>
    /// <param name="cell">The cell to add.</param>
    public void AddCell(TableCell cell)
    {
        Cells.Add(cell);
    }
}

using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A table widget that displays data in rows and columns with optional borders.
/// </summary>
public class Table : IWidget
{
    private readonly List<TableRow> _rows = new List<TableRow>();
    private TextStyle[][]? _rawStyles;

    /// <summary>
    /// Initializes a new instance of the <see cref="Table"/> class.
    /// </summary>
    public Table() { }

    /// <summary>
    /// Gets the collection of rows in the table.
    /// </summary>
    public List<TableRow> Rows => _rows;

    /// <summary>
    /// Gets or sets the border style for the table.
    /// </summary>
    public BorderStyle? BorderStyle { get; set; } = Widgets.BorderStyle.Single;

    /// <summary>
    /// Gets or sets a value indicating whether borders are shown.
    /// </summary>
    public bool HasBorder { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the outer border has rounded corners.
    /// </summary>
    public bool RoundedCorners { get; set; } = false;

    /// <summary>
    /// Gets or sets the unique name of the table widget.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the table widget.
    /// </summary>
    public string? Group { get; set; }

    private string _height = "1ch";
    private string _width = "100%";

    /// <summary>
    /// Gets or sets the width of the table widget.
    /// Width is automatically calculated based on column widths.
    /// </summary>
    public string Width
    {
        get
        {
            if (_rows.Count == 0) return "1ch";

            int columnCount = _rows.Max(r => r.Cells.Count);
            if (columnCount == 0) return "1ch";

            int[] columnWidths = CalculateColumnWidths(columnCount);
            int width = columnWidths.Sum() + (HasBorder ? columnCount + 1 : columnCount - 1);
            return $"{width}ch";
        }
        set => _width = value; // Store but ignore
    }

    /// <summary>
    /// Gets or sets the height of the table widget.
    /// Height is automatically calculated based on row count.
    /// </summary>
    public string Height
    {
        get
        {
            if (_rows.Count == 0) return "1ch";

            int[] rowHeights = CalculateRowHeights();
            int contentHeight = rowHeights.Sum();
            int borderHeight = HasBorder ? _rows.Count + 1 : _rows.Count - 1;
            int height = contentHeight + borderHeight;
            return $"{height}ch";
        }
        set => _height = value; // Store but ignore
    }

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
    /// Gets or sets the background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public Color BorderColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets a value indicating whether the table widget can receive focus.
    /// </summary>
    public bool CanFocus { get; set; } = false;

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
    bool IWidget.AllowWrapping { get; set; } = false;
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }
    bool IWidget.Disabled { get; set; }
    Color? IWidget.DisabledBackgroundColor { get; set; }
    Color IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Adds a row to the table.
    /// </summary>
    /// <param name="row">The row to add.</param>
    public void AddRow(TableRow row)
    {
        _rows.Add(row);
    }

    Rune[][] IWidget.GetRaw()
    {
        if (_rows.Count == 0)
        {
            return [];
        }

        // Calculate column count
        int columnCount = _rows.Max(r => r.Cells.Count);
        if (columnCount == 0)
        {
            return [];
        }

        // Calculate column widths and row heights based on content
        int[] columnWidths = CalculateColumnWidths(columnCount);
        int[] rowHeights = CalculateRowHeights();

        // Calculate total table dimensions
        int totalWidth = columnWidths.Sum() + (HasBorder ? columnCount + 1 : columnCount - 1);
        int contentHeight = rowHeights.Sum();
        int borderHeight = HasBorder ? _rows.Count + 1 : _rows.Count - 1;
        int totalHeight = contentHeight + borderHeight;

        if (totalWidth <= 0 || totalHeight <= 0)
        {
            return [];
        }

        var result = new Rune[totalHeight][];
        _rawStyles = new TextStyle[totalHeight][];
        for (int i = 0; i < totalHeight; i++)
        {
            result[i] = new Rune[totalWidth];
            Array.Fill(result[i], new Rune(' '));
            _rawStyles[i] = new TextStyle[totalWidth];
        }

        // Render the table
        int currentY = 0;

        for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
        {
            var row = _rows[rowIndex];
            int rowHeight = rowHeights[rowIndex];

            // Render top border
            if (HasBorder && rowIndex == 0)
            {
                RenderHorizontalBorder(result[currentY], columnWidths, true, false);
                currentY++;
            }

            // Render row content (multiple lines)
            RenderRow(result, currentY, row, columnWidths, rowHeight);
            currentY += rowHeight;

            // Render separator or bottom border
            if (HasBorder)
            {
                bool isLast = rowIndex == _rows.Count - 1;
                RenderHorizontalBorder(result[currentY], columnWidths, false, isLast);
                currentY++;
            }
        }

        return result;
    }

    private int[] CalculateColumnWidths(int columnCount)
    {
        int[] widths = new int[columnCount];

        foreach (var row in _rows)
        {
            for (int i = 0; i < row.Cells.Count && i < columnCount; i++)
            {
                var cell = row.Cells[i];
                // Text width (longest line if multi-line)
                var lines = cell.Text.Split('\n');
                int contentWidth = lines.Max(line => line.Length);
                widths[i] = Math.Max(widths[i], contentWidth);
            }
        }

        // Add padding
        for (int i = 0; i < widths.Length; i++)
        {
            widths[i] += 2; // 1 space on each side
        }

        return widths;
    }

    private int[] CalculateRowHeights()
    {
        int[] heights = new int[_rows.Count];

        for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
        {
            var row = _rows[rowIndex];
            int maxHeight = 1; // Minimum height is 1

            foreach (var cell in row.Cells)
            {
                // Count lines in text
                int cellHeight = cell.Text.Split('\n').Length;
                maxHeight = Math.Max(maxHeight, cellHeight);
            }

            heights[rowIndex] = maxHeight;
        }

        return heights;
    }

    private void RenderHorizontalBorder(Rune[] line, int[] columnWidths, bool isTop, bool isBottom)
    {
        if (BorderStyle == null) return;

        char left, right, mid, horizontal;

        if (BorderStyle == Widgets.BorderStyle.Double)
        {
            left = isTop ? '╔' : (isBottom ? '╚' : '╠');
            right = isTop ? '╗' : (isBottom ? '╝' : '╣');
            mid = isTop ? '╦' : (isBottom ? '╩' : '╬');
            horizontal = '═';
        }
        else
        {
            left = isTop ? '┌' : (isBottom ? '└' : '├');
            right = isTop ? '┐' : (isBottom ? '┘' : '┤');
            mid = isTop ? '┬' : (isBottom ? '┴' : '┼');
            horizontal = '─';
        }

        // Apply rounded corners if enabled (only for outer border)
        if (RoundedCorners && BorderStyle == Widgets.BorderStyle.Single)
        {
            if (isTop)
            {
                left = '╭';
                right = '╮';
            }
            else if (isBottom)
            {
                left = '╰';
                right = '╯';
            }
        }

        int x = 0;
        line[x++] = new Rune(left);

        for (int col = 0; col < columnWidths.Length; col++)
        {
            for (int i = 0; i < columnWidths[col]; i++)
            {
                line[x++] = new Rune(horizontal);
            }

            if (col < columnWidths.Length - 1)
            {
                line[x++] = new Rune(mid);
            }
        }

        line[x] = new Rune(right);
    }

    private void RenderRow(Rune[][] output, int startY, TableRow row, int[] columnWidths, int rowHeight)
    {
        char vertical = BorderStyle == Widgets.BorderStyle.Double ? '║' : '│';

        // Render each line of the row
        for (int line = 0; line < rowHeight; line++)
        {
            int y = startY + line;
            int x = 0;

            if (HasBorder)
            {
                output[y][x++] = new Rune(vertical);
            }

            for (int col = 0; col < columnWidths.Length; col++)
            {
                int cellWidth = columnWidths[col];
                TableCell? cell = col < row.Cells.Count ? row.Cells[col] : null;

                if (cell != null)
                {
                    RenderCellLine(output, y, x, cellWidth, cell, line);
                }
                else
                {
                    // Empty cell
                    for (int i = 0; i < cellWidth; i++)
                    {
                        output[y][x + i] = new Rune(' ');
                    }
                }

                x += cellWidth;

                if (col < columnWidths.Length - 1)
                {
                    if (HasBorder)
                    {
                        output[y][x++] = new Rune(vertical);
                    }
                    else
                    {
                        output[y][x++] = new Rune(' ');
                    }
                }
            }

            if (HasBorder)
            {
                output[y][x] = new Rune(vertical);
            }
        }
    }

    private void RenderCellLine(Rune[][] output, int y, int x, int width, TableCell cell, int lineIndex)
    {
        // Add left padding
        output[y][x++] = new Rune(' ');
        int contentWidth = width - 2;

        // Render text (support multi-line)
        var lines = cell.Text.Split('\n');
        if (lineIndex < lines.Length)
        {
            int charIdx = 0;
            foreach (Rune rune in lines[lineIndex].EnumerateRunes())
            {
                if (charIdx >= contentWidth) break;
                output[y][x + charIdx] = rune;
                if (cell.Style != TextStyle.Normal && _rawStyles != null)
                    _rawStyles[y][x + charIdx] = cell.Style;
                charIdx++;
            }
        }
        // Lines beyond text height remain as spaces

        // Add right padding
        output[y][x + contentWidth] = new Rune(' ');
    }

    TextStyle[][]? IWidget.GetRawStyles() => _rawStyles;

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Table does not handle key presses
    }

    /// <summary>
    /// Creates a copy of this table.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (clone rows and cells).</param>
    /// <returns>A new Table instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Table
        {
            BorderStyle = BorderStyle,
            HasBorder = HasBorder,
            RoundedCorners = RoundedCorners,
            Name = Name,
            Group = Group,
            _width = _width,
            _height = _height,
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
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            BorderColor = BorderColor,
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor,
            CanFocus = CanFocus
        };

        // Deep clone rows if requested
        if (deep)
        {
            foreach (var row in _rows)
            {
                var clonedRow = new TableRow();
                foreach (var cell in row.Cells)
                {
                    var clonedCell = new TableCell
                    {
                        Text = cell.Text,
                        Style = cell.Style,
                        ForegroundColor = cell.ForegroundColor,
                        BackgroundColor = cell.BackgroundColor,
                        Widget = cell.Widget // Note: Widget is not cloned, just referenced
                    };
                    clonedRow.Cells.Add(clonedCell);
                }
                clone.Rows.Add(clonedRow);
            }
        }

        return clone;
    }
}

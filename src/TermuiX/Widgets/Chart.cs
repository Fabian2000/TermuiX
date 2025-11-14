using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// Represents a data series for display in a chart.
/// </summary>
public class ChartDataSeries
{
    /// <summary>
    /// Gets or sets the label for this data series.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data points for this series.
    /// </summary>
    public List<double> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the color for rendering this series.
    /// </summary>
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
}

/// <summary>
/// A chart widget for visualizing data series with axes and legends.
/// </summary>
public class Chart : IWidget
{
    private readonly List<ChartDataSeries> _series = [];

    /// <summary>
    /// Gets the collection of data series to display in the chart.
    /// </summary>
    public List<ChartDataSeries> Series => _series;

    /// <summary>
    /// Gets or sets the minimum Y-axis value (null for auto-scaling).
    /// </summary>
    public double? MinY { get; set; } = null;

    /// <summary>
    /// Gets or sets the maximum Y-axis value (null for auto-scaling).
    /// </summary>
    public double? MaxY { get; set; } = null;

    /// <summary>
    /// Gets or sets the X-axis labels.
    /// </summary>
    public List<string> XLabels { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to show the legend.
    /// </summary>
    public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show axes.
    /// </summary>
    public bool ShowAxes { get; set; } = true;

    /// <summary>
    /// Gets or sets the width reserved for Y-axis labels.
    /// </summary>
    public int YAxisWidth { get; set; } = 6;

    /// <summary>
    /// Gets or sets the unique name of the chart.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the chart.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the chart.
    /// </summary>
    public string Width { get; set; } = "60ch";

    /// <summary>
    /// Gets or sets the height of the chart.
    /// </summary>
    public string Height { get; set; } = "15ch";

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
    /// Gets or sets the X position.
    /// </summary>
    public string PositionX { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the Y position.
    /// </summary>
    public string PositionY { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets a value indicating whether the chart is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets a value indicating whether the chart can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether the chart is scrollable.
    /// </summary>
    public bool Scrollable => false;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    /// <summary>
    /// Adds a data series to the chart.
    /// </summary>
    /// <param name="series">The series to add.</param>
    public void AddSeries(ChartDataSeries series)
    {
        _series.Add(series);
    }

    /// <summary>
    /// Removes all data series from the chart.
    /// </summary>
    public void ClearSeries()
    {
        _series.Clear();
    }

    Rune[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        int height = GetHeightInChars();

        if (width <= 0 || height <= 0 || _series.Count == 0)
        {
            return CreateEmptyResult(width, height);
        }

        // Calculate dimensions
        // Layout: [Legend                  ]
        //         [Y-axis labels][Chart area]
        //         [           ][X-axis line]
        int legendHeight = ShowLegend && _series.Count > 0 ? 1 : 0; // 1 line for legend at top
        int xAxisHeight = ShowAxes ? 2 : 0; // 1 for line, 1 for labels
        int yAxisWidth = ShowAxes ? YAxisWidth : 0;

        int chartHeight = height - legendHeight - xAxisHeight;
        int chartWidth = width - yAxisWidth;

        if (chartHeight < 3 || chartWidth < 10)
        {
            return CreateEmptyResult(width, height);
        }

        // Create result array
        var result = new Rune[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new Rune[width];
            Array.Fill(result[i], new Rune(' '));
        }

        // Render legend at the TOP
        if (ShowLegend && _series.Count > 0)
        {
            RenderLegend(result, 0, width);
        }

        // Calculate Y-axis range
        double minY = MinY ?? CalculateMinY();
        double maxY = MaxY ?? CalculateMaxY();

        if (Math.Abs(maxY - minY) < 0.0001)
        {
            maxY = minY + 1;
        }

        // Render Y-axis (offset by legend height)
        if (ShowAxes)
        {
            RenderYAxis(result, minY, maxY, chartHeight, yAxisWidth, legendHeight);
        }

        // Render chart area (ONLY dots, no lines)
        RenderChartData(result, minY, maxY, chartWidth, chartHeight, yAxisWidth, legendHeight);

        // Render X-axis (offset by legend height)
        if (ShowAxes)
        {
            RenderXAxis(result, legendHeight + chartHeight, yAxisWidth, chartWidth);
        }

        return result;
    }

    private Rune[][] CreateEmptyResult(int width, int height)
    {
        var result = new Rune[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new Rune[width];
            Array.Fill(result[i], new Rune(' '));
        }
        return result;
    }

    private double CalculateMinY()
    {
        if (_series.Count == 0) return 0;

        double min = double.MaxValue;
        foreach (var series in _series)
        {
            if (series.Data.Count > 0)
            {
                double seriesMin = series.Data.Min();
                if (seriesMin < min) min = seriesMin;
            }
        }

        return min == double.MaxValue ? 0 : min;
    }

    private double CalculateMaxY()
    {
        if (_series.Count == 0) return 100;

        double max = double.MinValue;
        foreach (var series in _series)
        {
            if (series.Data.Count > 0)
            {
                double seriesMax = series.Data.Max();
                if (seriesMax > max) max = seriesMax;
            }
        }

        return max == double.MinValue ? 100 : max;
    }

    private void RenderYAxis(Rune[][] result, double minY, double maxY, int chartHeight, int yAxisWidth, int yOffset)
    {
        // Draw 4 Y-axis labels evenly distributed
        for (int i = 0; i < 4; i++)
        {
            int y = yOffset + (chartHeight - 1) * i / 3;
            double value = maxY - (maxY - minY) * i / 3;
            string label = FormatNumber(value);

            // Right-align label
            int startX = Math.Max(0, yAxisWidth - label.Length - 1);

            for (int x = 0; x < label.Length && startX + x < yAxisWidth - 1; x++)
            {
                result[y][startX + x] = new Rune(label[x]);
            }
        }

        // Draw vertical line
        for (int y = 0; y < chartHeight; y++)
        {
            result[yOffset + y][yAxisWidth - 1] = new Rune('│');
        }
    }

    private void RenderXAxis(Rune[][] result, int yPos, int xOffset, int chartWidth)
    {
        // Draw horizontal line
        for (int x = 0; x < chartWidth; x++)
        {
            result[yPos][xOffset + x] = new Rune('─');
        }

        // Draw corner
        if (xOffset > 0)
        {
            result[yPos][xOffset - 1] = new Rune('└');
        }

        // X-axis labels on line below (if there's space)
        if (XLabels.Count > 0 && yPos + 1 < result.Length)
        {
            int spacing = Math.Max(1, chartWidth / Math.Min(XLabels.Count, 12));
            for (int i = 0; i < XLabels.Count && i < 12; i++)
            {
                string label = XLabels[i];
                int x = xOffset + i * spacing;

                for (int j = 0; j < label.Length && x + j < xOffset + chartWidth; j++)
                {
                    result[yPos + 1][x + j] = new Rune(label[j]);
                }
            }
        }
    }

    private void RenderChartData(Rune[][] result, double minY, double maxY, int chartWidth, int chartHeight, int xOffset, int yOffset)
    {
        // Draw ONLY data points, no lines
        // Use different symbols for different series
        char[] symbols = ['●', '○', '■', '□', '▲', '△'];

        int seriesIndex = 0;
        foreach (var series in _series)
        {
            if (series.Data.Count == 0) continue;

            char symbol = symbols[seriesIndex % symbols.Length];
            seriesIndex++;

            for (int i = 0; i < series.Data.Count; i++)
            {
                double value = series.Data[i];
                int x = (int)(i * (double)(chartWidth - 1) / Math.Max(1, series.Data.Count - 1));
                int y = MapValueToY(value, minY, maxY, chartHeight);

                int screenX = xOffset + x;
                int screenY = yOffset + y;

                if (screenX >= xOffset && screenX < xOffset + chartWidth &&
                    screenY >= yOffset && screenY < yOffset + chartHeight)
                {
                    result[screenY][screenX] = new Rune(symbol);
                }
            }
        }
    }

    private int MapValueToY(double value, double minY, double maxY, int chartHeight)
    {
        double range = maxY - minY;
        double normalized = (value - minY) / range;

        // Invert Y (0 is top of screen)
        int y = chartHeight - 1 - (int)(normalized * (chartHeight - 1));

        return Math.Clamp(y, 0, chartHeight - 1);
    }

    private void DrawLine(Rune[][] result, int x1, int y1, int x2, int y2, int xOffset, int chartWidth, int chartHeight)
    {
        // Simple Bresenham's line algorithm
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        int x = x1;
        int y = y1;

        while (true)
        {
            // Plot point
            int screenX = xOffset + x;
            int screenY = y;

            if (screenX >= xOffset && screenX < xOffset + chartWidth &&
                screenY >= 0 && screenY < chartHeight &&
                result[screenY][screenX].Value != '●') // Don't overwrite data points
            {
                // Choose character based on direction
                if (dx > dy * 2)
                    result[screenY][screenX] = new Rune('─');
                else if (dy > dx * 2)
                    result[screenY][screenX] = new Rune('│');
                else if ((sx > 0 && sy > 0) || (sx < 0 && sy < 0))
                    result[screenY][screenX] = new Rune('\\');
                else
                    result[screenY][screenX] = new Rune('/');
            }

            if (x == x2 && y == y2) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private void RenderLegend(Rune[][] result, int y, int width)
    {
        if (y >= result.Length) return;

        int x = 0;

        // Use same symbols as in RenderChartData
        char[] symbols = ['●', '○', '■', '□', '▲', '△'];

        // Write series labels with markers
        for (int i = 0; i < _series.Count; i++)
        {
            var series = _series[i];

            // Add separator if not first
            if (i > 0)
            {
                if (x < width) result[y][x++] = new Rune(' ');
                if (x < width) result[y][x++] = new Rune(' ');
            }

            // Add marker (matching symbol from chart)
            char symbol = symbols[i % symbols.Length];
            if (x < width) result[y][x++] = new Rune(symbol);
            if (x < width) result[y][x++] = new Rune(' ');

            // Add label
            for (int j = 0; j < series.Label.Length && x < width; j++, x++)
            {
                result[y][x] = new Rune(series.Label[j]);
            }
        }
    }

    private string FormatNumber(double value)
    {
        if (Math.Abs(value) >= 1000000)
            return (value / 1000000.0).ToString("0.#") + "M";
        else if (Math.Abs(value) >= 1000)
            return (value / 1000.0).ToString("0.#") + "K";
        else if (Math.Abs(value) < 0.01 && value != 0)
            return value.ToString("0.##E+0");
        else
            return value.ToString("0.##");
    }

    private int GetWidthInChars()
    {
        if (Width.EndsWith("ch"))
        {
            var value = Width[..^2].Trim();
            if (int.TryParse(value, out int result))
                return result;
        }
        return 60;
    }

    private int GetHeightInChars()
    {
        if (Height.EndsWith("ch"))
        {
            var value = Height[..^2].Trim();
            if (int.TryParse(value, out int result))
                return result;
        }
        return 15;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }

    /// <summary>
    /// Creates a copy of this chart.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (clone series data).</param>
    /// <returns>A new Chart instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Chart
        {
            MinY = MinY,
            MaxY = MaxY,
            ShowLegend = ShowLegend,
            ShowAxes = ShowAxes,
            YAxisWidth = YAxisWidth,
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
            FocusForegroundColor = FocusForegroundColor
        };

        // Clone XLabels
        clone.XLabels = new List<string>(XLabels);

        // Deep clone series if requested
        if (deep)
        {
            foreach (var series in _series)
            {
                var clonedSeries = new ChartDataSeries
                {
                    Label = series.Label,
                    Color = series.Color,
                    Data = new List<double>(series.Data)
                };
                clone.Series.Add(clonedSeries);
            }
        }

        return clone;
    }
}

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
    public Color Color { get; set; } = ConsoleColor.White;
}

/// <summary>
/// A chart widget for visualizing data series using Braille characters for high-resolution rendering.
/// Each terminal cell contains a 2×4 Braille dot grid, providing 8× the resolution of character-based charts.
/// </summary>
public class Chart : IWidget
{
    private readonly List<ChartDataSeries> _series = [];
    private Color[][]? _rawFg;

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
    /// <summary>Gets or sets the minimum width constraint.</summary>
    public string MinWidth { get; set; } = "";
    /// <summary>Gets or sets the maximum width constraint.</summary>
    public string MaxWidth { get; set; } = "";
    /// <summary>Gets or sets the minimum height constraint.</summary>
    public string MinHeight { get; set; } = "";
    /// <summary>Gets or sets the maximum height constraint.</summary>
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
    public Color BackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets a value indicating whether the chart can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets a value indicating whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

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
    bool IWidget.Disabled { get; set; }
    Color? IWidget.DisabledBackgroundColor { get; set; }
    Color IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

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
        // Use ComputedWidth/Height set by the Renderer (supports fill, %, etc.)
        int computedW = ((IWidget)this).ComputedWidth;
        int computedH = ((IWidget)this).ComputedHeight;
        int width = computedW > 0 ? computedW : ParseSize(Width, 60);
        int height = computedH > 0 ? computedH : ParseSize(Height, 15);

        if (width <= 0 || height <= 0 || _series.Count == 0)
        {
            _rawFg = null;
            return CreateEmptyResult(width, height);
        }

        // Initialize per-cell foreground color array
        _rawFg = new Color[height][];
        for (int i = 0; i < height; i++)
        {
            _rawFg[i] = new Color[width];
        }

        // Layout: [Legend                  ]
        //         [Y-axis labels][Chart area]
        //         [             ][X-axis line]
        int legendHeight = ShowLegend && _series.Count > 0 ? 1 : 0;
        int xAxisHeight = ShowAxes ? 2 : 0;
        int yAxisWidth = ShowAxes ? YAxisWidth : 0;

        int chartHeight = height - legendHeight - xAxisHeight;
        int chartWidth = width - yAxisWidth;

        if (chartHeight < 3 || chartWidth < 10)
        {
            _rawFg = null;
            return CreateEmptyResult(width, height);
        }

        // Create result array
        var result = new Rune[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new Rune[width];
            Array.Fill(result[i], new Rune(' '));
        }

        // Render legend at the top
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

        // Render Y-axis
        if (ShowAxes)
        {
            RenderYAxis(result, minY, maxY, chartHeight, yAxisWidth, legendHeight);
        }

        // Render chart data using Braille
        RenderBrailleData(result, minY, maxY, chartWidth, chartHeight, yAxisWidth, legendHeight);

        // Render X-axis
        if (ShowAxes)
        {
            RenderXAxis(result, legendHeight + chartHeight, yAxisWidth, chartWidth);
        }

        return result;
    }

    private static int ParseSize(string size, int fallback)
    {
        if (size.EndsWith("ch"))
        {
            var value = size[..^2].Trim();
            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }
        return fallback;
    }

    private Rune[][] CreateEmptyResult(int width, int height)
    {
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);
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
        if (_series.Count == 0) { return 0; }

        double min = double.MaxValue;
        foreach (var series in _series)
        {
            if (series.Data.Count > 0)
            {
                double seriesMin = series.Data.Min();
                if (seriesMin < min) { min = seriesMin; }
            }
        }

        return min == double.MaxValue ? 0 : min;
    }

    private double CalculateMaxY()
    {
        if (_series.Count == 0) { return 100; }

        double max = double.MinValue;
        foreach (var series in _series)
        {
            if (series.Data.Count > 0)
            {
                double seriesMax = series.Data.Max();
                if (seriesMax > max) { max = seriesMax; }
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

    private void RenderBrailleData(Rune[][] result, double minY, double maxY, int chartWidth, int chartHeight, int xOffset, int yOffset)
    {
        // Braille pixel resolution: each cell is 2 wide × 4 tall
        int pixelWidth = chartWidth * 2;
        int pixelHeight = chartHeight * 4;

        // Braille dot grid (one byte per cell, bits map to dots)
        var brailleGrid = new byte[chartHeight][];
        // Color per cell (last series to touch a cell wins)
        var cellColors = new Color[chartHeight][];
        for (int i = 0; i < chartHeight; i++)
        {
            brailleGrid[i] = new byte[chartWidth];
            cellColors[i] = new Color[chartWidth];
        }

        // Render each series
        foreach (var series in _series)
        {
            if (series.Data.Count == 0) { continue; }

            // Map data points to Braille pixel coordinates
            var points = new (int px, int py)[series.Data.Count];
            for (int i = 0; i < series.Data.Count; i++)
            {
                int px;
                if (series.Data.Count == 1)
                {
                    px = pixelWidth / 2;
                }
                else
                {
                    px = (int)((long)i * (pixelWidth - 1) / (series.Data.Count - 1));
                }

                double normalized = (series.Data[i] - minY) / (maxY - minY);
                int py = (pixelHeight - 1) - (int)(normalized * (pixelHeight - 1));
                py = Math.Clamp(py, 0, pixelHeight - 1);

                points[i] = (px, py);
            }

            // Draw lines between consecutive points using Bresenham
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawBrailleLine(brailleGrid, cellColors, series.Color, chartWidth, chartHeight,
                    points[i].px, points[i].py, points[i + 1].px, points[i + 1].py);
            }

            // If only one point, just set it
            if (points.Length == 1)
            {
                SetBraillePixel(brailleGrid, cellColors, series.Color, chartWidth, chartHeight, points[0].px, points[0].py);
            }
        }

        // Convert Braille grid to Runes and set colors
        for (int cy = 0; cy < chartHeight; cy++)
        {
            for (int cx = 0; cx < chartWidth; cx++)
            {
                if (brailleGrid[cy][cx] != 0)
                {
                    result[yOffset + cy][xOffset + cx] = new Rune((char)(0x2800 + brailleGrid[cy][cx]));
                    if (_rawFg is not null)
                    {
                        _rawFg[yOffset + cy][xOffset + cx] = cellColors[cy][cx];
                    }
                }
            }
        }
    }

    private static void SetBraillePixel(byte[][] grid, Color[][] cellColors, Color color, int cellWidth, int cellHeight, int px, int py)
    {
        int cx = px / 2;
        int cy = py / 4;

        if (cx < 0 || cx >= cellWidth || cy < 0 || cy >= cellHeight)
        {
            return;
        }

        int subX = px % 2;
        int subY = py % 4;

        // Map (subX, subY) to Braille dot bit
        byte bit = (subX, subY) switch
        {
            (0, 0) => 0x01, // dot 1
            (0, 1) => 0x02, // dot 2
            (0, 2) => 0x04, // dot 3
            (0, 3) => 0x40, // dot 7
            (1, 0) => 0x08, // dot 4
            (1, 1) => 0x10, // dot 5
            (1, 2) => 0x20, // dot 6
            (1, 3) => 0x80, // dot 8
            _ => 0
        };

        grid[cy][cx] |= bit;
        cellColors[cy][cx] = color;
    }

    private static void DrawBrailleLine(byte[][] grid, Color[][] cellColors, Color color, int cellWidth, int cellHeight, int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            SetBraillePixel(grid, cellColors, color, cellWidth, cellHeight, x0, y0);

            if (x0 == x1 && y0 == y1) { break; }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void RenderLegend(Rune[][] result, int y, int width)
    {
        if (y >= result.Length) { return; }

        int x = 0;

        for (int i = 0; i < _series.Count; i++)
        {
            var series = _series[i];

            // Add separator if not first
            if (i > 0)
            {
                if (x < width) { result[y][x++] = new Rune(' '); }
                if (x < width) { result[y][x++] = new Rune(' '); }
            }

            // Add Braille marker (small filled block) in series color
            if (x < width)
            {
                result[y][x] = new Rune('⣿');
                if (_rawFg is not null) { _rawFg[y][x] = series.Color; }
                x++;
            }
            if (x < width) { result[y][x++] = new Rune(' '); }

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
        {
            return (value / 1000000.0).ToString("0.#") + "M";
        }
        else if (Math.Abs(value) >= 1000)
        {
            return (value / 1000.0).ToString("0.#") + "K";
        }
        else if (Math.Abs(value) < 0.01 && value != 0)
        {
            return value.ToString("0.##E+0");
        }
        else
        {
            return value.ToString("0.##");
        }
    }

    (Color[][] fg, Color[][] bg)? IWidget.GetRawColors()
    {
        if (_rawFg is null) { return null; }
        // Only fg overrides needed — bg stays default (no override)
        var emptyBg = new Color[_rawFg.Length][];
        for (int i = 0; i < _rawFg.Length; i++)
        {
            emptyBg[i] = new Color[_rawFg[i].Length];
        }
        return (_rawFg, emptyBg);
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

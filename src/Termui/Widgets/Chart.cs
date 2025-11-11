namespace Termui.Widgets;

public class ChartDataSeries
{
    public string Label { get; set; } = string.Empty;
    public List<double> Data { get; set; } = [];
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
}

public class Chart : IWidget
{
    private readonly List<ChartDataSeries> _series = [];

    public List<ChartDataSeries> Series => _series;

    public double? MinY { get; set; } = null; // Auto-scale if null
    public double? MaxY { get; set; } = null; // Auto-scale if null
    public List<string> XLabels { get; set; } = []; // Optional X-axis labels
    public bool ShowLegend { get; set; } = true;
    public bool ShowAxes { get; set; } = true;
    public int YAxisWidth { get; set; } = 6; // Width for Y-axis labels

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "60ch";
    public string Height { get; set; } = "15ch";
    public string PaddingLeft { get; set; } = "0ch";
    public string PaddingTop { get; set; } = "0ch";
    public string PaddingRight { get; set; } = "0ch";
    public string PaddingBottom { get; set; } = "0ch";
    public string PositionX { get; set; } = "0ch";
    public string PositionY { get; set; } = "0ch";
    public bool Visible { get; set; } = true;
    public bool AllowWrapping { get; set; } = false;

    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    public bool CanFocus => false;
    public bool Scrollable => false;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    public void AddSeries(ChartDataSeries series)
    {
        _series.Add(series);
    }

    public void ClearSeries()
    {
        _series.Clear();
    }

    char[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        int height = GetHeightInChars();

        if (width <= 0 || height <= 0 || _series.Count == 0)
            return CreateEmptyResult(width, height);

        // Calculate dimensions
        int legendHeight = ShowLegend && _series.Count > 0 ? 1 : 0;
        int xAxisHeight = ShowAxes ? 1 : 0;
        int yAxisWidth = ShowAxes ? YAxisWidth : 0;

        int chartHeight = height - legendHeight - xAxisHeight;
        int chartWidth = width - yAxisWidth;

        if (chartHeight < 3 || chartWidth < 10)
            return CreateEmptyResult(width, height);

        // Create result array
        var result = new char[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new char[width];
            Array.Fill(result[i], ' ');
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
            RenderYAxis(result, minY, maxY, chartHeight, yAxisWidth);
        }

        // Render chart area (just the plot)
        RenderChartData(result, minY, maxY, chartWidth, chartHeight, yAxisWidth);

        // Render X-axis
        if (ShowAxes)
        {
            RenderXAxis(result, chartHeight, yAxisWidth, chartWidth);
        }

        // Render legend at the bottom
        if (ShowLegend && _series.Count > 0)
        {
            RenderLegend(result, height - 1, width);
        }

        return result;
    }

    private char[][] CreateEmptyResult(int width, int height)
    {
        var result = new char[height][];
        for (int i = 0; i < height; i++)
        {
            result[i] = new char[width];
            Array.Fill(result[i], ' ');
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

    private void RenderYAxis(char[][] result, double minY, double maxY, int chartHeight, int yAxisWidth)
    {
        // Draw 4 Y-axis labels evenly distributed
        for (int i = 0; i < 4; i++)
        {
            int y = (chartHeight - 1) * i / 3;
            double value = maxY - (maxY - minY) * i / 3;
            string label = FormatNumber(value);

            // Right-align label
            int startX = Math.Max(0, yAxisWidth - label.Length - 1);

            for (int x = 0; x < label.Length && startX + x < yAxisWidth - 1; x++)
            {
                result[y][startX + x] = label[x];
            }
        }

        // Draw vertical line
        for (int y = 0; y < chartHeight; y++)
        {
            result[y][yAxisWidth - 1] = '│';
        }
    }

    private void RenderXAxis(char[][] result, int yPos, int xOffset, int chartWidth)
    {
        // Draw horizontal line
        for (int x = 0; x < chartWidth; x++)
        {
            result[yPos][xOffset + x] = '─';
        }

        // Draw corner
        if (xOffset > 0)
        {
            result[yPos][xOffset - 1] = '└';
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
                    result[yPos + 1][x + j] = label[j];
                }
            }
        }
    }

    private void RenderChartData(char[][] result, double minY, double maxY, int chartWidth, int chartHeight, int xOffset)
    {
        foreach (var series in _series)
        {
            if (series.Data.Count == 0) continue;

            // Draw lines connecting data points
            for (int i = 0; i < series.Data.Count - 1; i++)
            {
                double val1 = series.Data[i];
                double val2 = series.Data[i + 1];

                int x1 = (int)(i * (double)(chartWidth - 1) / Math.Max(1, series.Data.Count - 1));
                int x2 = (int)((i + 1) * (double)(chartWidth - 1) / Math.Max(1, series.Data.Count - 1));

                int y1 = MapValueToY(val1, minY, maxY, chartHeight);
                int y2 = MapValueToY(val2, minY, maxY, chartHeight);

                DrawLine(result, x1, y1, x2, y2, xOffset, chartWidth, chartHeight);
            }

            // Draw data points on top
            for (int i = 0; i < series.Data.Count; i++)
            {
                double value = series.Data[i];
                int x = (int)(i * (double)(chartWidth - 1) / Math.Max(1, series.Data.Count - 1));
                int y = MapValueToY(value, minY, maxY, chartHeight);

                int screenX = xOffset + x;
                int screenY = y;

                if (screenX >= xOffset && screenX < xOffset + chartWidth &&
                    screenY >= 0 && screenY < chartHeight)
                {
                    result[screenY][screenX] = '●';
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

    private void DrawLine(char[][] result, int x1, int y1, int x2, int y2, int xOffset, int chartWidth, int chartHeight)
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
                result[screenY][screenX] != '●') // Don't overwrite data points
            {
                // Choose character based on direction
                if (dx > dy * 2)
                    result[screenY][screenX] = '─';
                else if (dy > dx * 2)
                    result[screenY][screenX] = '│';
                else if ((sx > 0 && sy > 0) || (sx < 0 && sy < 0))
                    result[screenY][screenX] = '\\';
                else
                    result[screenY][screenX] = '/';
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

    private void RenderLegend(char[][] result, int y, int width)
    {
        if (y >= result.Length) return;

        int x = 0;

        // Write series labels with markers
        for (int i = 0; i < _series.Count; i++)
        {
            var series = _series[i];

            // Add separator if not first
            if (i > 0)
            {
                if (x < width) result[y][x++] = ' ';
                if (x < width) result[y][x++] = ' ';
            }

            // Add marker
            if (x < width) result[y][x++] = '●';
            if (x < width) result[y][x++] = ' ';

            // Add label
            for (int j = 0; j < series.Label.Length && x < width; j++, x++)
            {
                result[y][x] = series.Label[j];
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
        // Chart doesn't handle key presses
    }
}

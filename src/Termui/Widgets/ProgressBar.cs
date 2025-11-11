namespace Termui.Widgets;

public enum ProgressBarMode
{
    Progress,   // Normal progress bar with percentage
    Marquee     // Animated loading bar
}

public class ProgressBar : IWidget
{
    private double _value = 0.0; // 0.0 to 1.0
    private int _marqueePosition = 0;
    private DateTime _lastMarqueeUpdate = DateTime.Now;
    private const int MarqueeUpdateIntervalMs = 100;
    private const int MarqueeWidth = 5;

    public ProgressBarMode Mode { get; set; } = ProgressBarMode.Progress;

    public double Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0.0, 1.0);
    }

    public char FilledChar { get; set; } = '█';
    public char EmptyChar { get; set; } = '░';
    public bool ShowPercentage { get; set; } = true;

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "30ch";
    public string Height { get; set; } = "1ch";
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

    char[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        var result = new char[1][];
        result[0] = new char[width];

        if (Mode == ProgressBarMode.Progress)
        {
            RenderProgress(result[0], width);
        }
        else
        {
            RenderMarquee(result[0], width);
        }

        return result;
    }

    private void RenderProgress(char[] line, int width)
    {
        int barWidth = width;
        string percentText = "";

        if (ShowPercentage)
        {
            int percentage = (int)(_value * 100);
            percentText = $" {percentage}%";
            barWidth = Math.Max(1, width - percentText.Length);
        }

        int filledWidth = (int)(barWidth * _value);

        // Render bar
        for (int i = 0; i < barWidth; i++)
        {
            line[i] = i < filledWidth ? FilledChar : EmptyChar;
        }

        // Render percentage text
        if (ShowPercentage)
        {
            for (int i = 0; i < percentText.Length && barWidth + i < width; i++)
            {
                line[barWidth + i] = percentText[i];
            }
        }
    }

    private void RenderMarquee(char[] line, int width)
    {
        // Update marquee position
        if ((DateTime.Now - _lastMarqueeUpdate).TotalMilliseconds >= MarqueeUpdateIntervalMs)
        {
            _marqueePosition = (_marqueePosition + 1) % (width + MarqueeWidth);
            _lastMarqueeUpdate = DateTime.Now;
        }

        // Fill with empty chars
        Array.Fill(line, EmptyChar);

        // Draw moving filled section
        int startPos = _marqueePosition - MarqueeWidth;
        for (int i = 0; i < MarqueeWidth; i++)
        {
            int pos = startPos + i;
            if (pos >= 0 && pos < width)
            {
                line[pos] = FilledChar;
            }
        }
    }

    private int GetWidthInChars()
    {
        if (Width.EndsWith("ch"))
        {
            var value = Width[..^2].Trim();
            if (int.TryParse(value, out int result))
                return result;
        }
        return 30; // Default
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // ProgressBar doesn't handle key presses
    }
}

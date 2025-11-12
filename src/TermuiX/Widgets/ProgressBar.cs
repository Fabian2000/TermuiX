namespace TermuiX.Widgets;

/// <summary>
/// Specifies the mode of operation for a progress bar.
/// </summary>
public enum ProgressBarMode
{
    /// <summary>
    /// Normal progress bar showing a percentage.
    /// </summary>
    Progress,

    /// <summary>
    /// Animated loading bar (marquee style).
    /// </summary>
    Marquee
}

/// <summary>
/// A progress bar widget that can display determinate or indeterminate progress.
/// </summary>
public class ProgressBar : IWidget
{
    private double _value = 0.0;
    private int _marqueePosition = 0;
    private DateTime _lastMarqueeUpdate = DateTime.Now;
    private const int MarqueeUpdateIntervalMs = 100;
    private const int MarqueeWidth = 5;

    /// <summary>
    /// Gets or sets the progress bar mode.
    /// </summary>
    public ProgressBarMode Mode { get; set; } = ProgressBarMode.Progress;

    /// <summary>
    /// Gets or sets the progress value (0.0 to 1.0).
    /// </summary>
    public double Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Gets or sets the character used for filled portions of the bar.
    /// </summary>
    public char FilledChar { get; set; } = '█';

    /// <summary>
    /// Gets or sets the character used for empty portions of the bar.
    /// </summary>
    public char EmptyChar { get; set; } = '░';

    /// <summary>
    /// Gets or sets a value indicating whether to show the percentage text.
    /// </summary>
    public bool ShowPercentage { get; set; } = true;

    /// <summary>
    /// Gets or sets the unique name of the progress bar.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the progress bar.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the progress bar.
    /// </summary>
    public string Width { get; set; } = "30ch";

    /// <summary>
    /// Gets or sets the height of the progress bar.
    /// </summary>
    public string Height { get; set; } = "1ch";

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
    /// Gets or sets a value indicating whether the progress bar is visible.
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
    /// Gets a value indicating whether the progress bar can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether the progress bar is scrollable.
    /// </summary>
    public bool Scrollable => false;

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

        for (int i = 0; i < barWidth; i++)
        {
            line[i] = i < filledWidth ? FilledChar : EmptyChar;
        }

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
        if ((DateTime.Now - _lastMarqueeUpdate).TotalMilliseconds >= MarqueeUpdateIntervalMs)
        {
            _marqueePosition = (_marqueePosition + 1) % (width + MarqueeWidth);
            _lastMarqueeUpdate = DateTime.Now;
        }

        Array.Fill(line, EmptyChar);

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
            {
                return result;
            }
        }
        return 30;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }
}

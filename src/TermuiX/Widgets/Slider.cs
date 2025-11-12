namespace TermuiX.Widgets;

/// <summary>
/// A slider widget for selecting numeric values within a range.
/// </summary>
public class Slider : IWidget
{
    private double _value = 0;
    private double _min = 0;
    private double _max = 100;

    /// <summary>
    /// Gets or sets the current value of the slider.
    /// </summary>
    public double Value
    {
        get => _value;
        set
        {
            double newValue = Math.Clamp(value, _min, _max);
            if (Math.Abs(_value - newValue) > 0.0001)
            {
                _value = newValue;
                OnChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Min
    {
        get => _min;
        set
        {
            _min = value;
            if (_value < _min)
            {
                Value = _min;
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Max
    {
        get => _max;
        set
        {
            _max = value;
            if (_value > _max)
            {
                Value = _max;
            }
        }
    }

    /// <summary>
    /// Gets or sets the step increment for value changes.
    /// </summary>
    public double Step { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets a value indicating whether to display the current value.
    /// </summary>
    public bool ShowValue { get; set; } = true;

    /// <summary>
    /// Gets or sets the unique name of the slider.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the slider.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the slider.
    /// </summary>
    public string Width { get; set; } = "20ch";

    /// <summary>
    /// Gets or sets the height of the slider.
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
    /// Gets or sets a value indicating whether the slider is visible.
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
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets a value indicating whether the slider can receive focus.
    /// </summary>
    public bool CanFocus => true;

    /// <summary>
    /// Gets a value indicating whether the slider is scrollable.
    /// </summary>
    public bool Scrollable => false;

    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    /// <summary>
    /// Occurs when the slider value changes.
    /// </summary>
    public event EventHandler<double>? Changed;

    char[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        if (width < 5)
        {
            width = 5;
        }

        var result = new char[1][];
        result[0] = new char[width];
        Array.Fill(result[0], ' ');

        double range = _max - _min;
        double normalizedValue = range > 0 ? (_value - _min) / range : 0;

        int valueTextLength = 0;
        string valueText = "";
        if (ShowValue)
        {
            valueText = $" {_value:0.##}";
            valueTextLength = valueText.Length;
        }

        int trackWidth = width - valueTextLength;
        if (trackWidth < 3)
        {
            trackWidth = 3;
        }

        result[0][0] = '[';
        result[0][trackWidth - 1] = ']';

        int thumbPos = (int)((trackWidth - 2) * normalizedValue);
        thumbPos = Math.Clamp(thumbPos, 0, trackWidth - 2);

        for (int i = 1; i < trackWidth - 1; i++)
        {
            if (i - 1 == thumbPos)
            {
                result[0][i] = '●';
            }
            else if (i - 1 < thumbPos)
            {
                result[0][i] = '━';
            }
            else
            {
                result[0][i] = '─';
            }
        }

        if (ShowValue && valueTextLength > 0)
        {
            for (int i = 0; i < valueText.Length && trackWidth + i < width; i++)
            {
                result[0][trackWidth + i] = valueText[i];
            }
        }

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        double step = Step;

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
            step *= 10;
        }

        if (keyInfo.Key == ConsoleKey.LeftArrow)
        {
            Value -= step;
        }
        else if (keyInfo.Key == ConsoleKey.RightArrow)
        {
            Value += step;
        }
        else if (keyInfo.Key == ConsoleKey.Home)
        {
            Value = _min;
        }
        else if (keyInfo.Key == ConsoleKey.End)
        {
            Value = _max;
        }
    }

    protected virtual void OnChanged()
    {
        Changed?.Invoke(this, _value);
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
        return 20;
    }
}

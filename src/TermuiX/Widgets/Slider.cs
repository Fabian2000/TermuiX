namespace TermuiX.Widgets;

public class Slider : IWidget
{
    private double _value = 0;
    private double _min = 0;
    private double _max = 100;

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

    public double Min
    {
        get => _min;
        set
        {
            _min = value;
            if (_value < _min) Value = _min;
        }
    }

    public double Max
    {
        get => _max;
        set
        {
            _max = value;
            if (_value > _max) Value = _max;
        }
    }

    public double Step { get; set; } = 1.0;
    public bool ShowValue { get; set; } = true;

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "20ch";
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
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    public bool CanFocus => true;
    public bool Scrollable => false;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    // Events
    public event EventHandler<double>? Changed;

    char[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        if (width < 5) width = 5; // Minimum width

        var result = new char[1][];
        result[0] = new char[width];
        Array.Fill(result[0], ' ');

        // Calculate value position
        double range = _max - _min;
        double normalizedValue = range > 0 ? (_value - _min) / range : 0;

        // Reserve space for value display if enabled
        int valueTextLength = 0;
        string valueText = "";
        if (ShowValue)
        {
            valueText = $" {_value:0.##}";
            valueTextLength = valueText.Length;
        }

        int trackWidth = width - valueTextLength;
        if (trackWidth < 3) trackWidth = 3;

        // Draw track: [────●────]
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

        // Draw value text
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

        // Shift increases step size by 10x
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
                return result;
        }
        return 20;
    }
}

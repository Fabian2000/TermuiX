using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A slider widget for selecting numeric values within a range.
/// </summary>
public class Slider : IWidget
{
    private double _value = 0;
    private double _min = 0;
    private double _max = 100;
    private Color _backgroundColor = ConsoleColor.Black;
    private Color _foregroundColor = ConsoleColor.White;
    private int _lastTrackWidth = 0;

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
    public Color BackgroundColor
    {
        get => Disabled && DisabledBackgroundColor.HasValue ? DisabledBackgroundColor.Value : _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor
    {
        get => Disabled ? DisabledForegroundColor : _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets a value indicating whether the slider is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the background color when disabled.
    /// </summary>
    public Color? DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color when disabled.
    /// </summary>
    public Color DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets a value indicating whether the slider can receive focus.
    /// </summary>
    public bool CanFocus => true;

    /// <summary>
    /// Gets a value indicating whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets a value indicating whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

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

    /// <summary>
    /// Occurs when the slider value changes.
    /// </summary>
    public event EventHandler<double>? ValueChanged;

    void IWidget.MousePress(MouseEventArgs args)
    {
        if (args.EventType == MouseEventType.LeftButtonPressed || args.EventType == MouseEventType.Moved)
        {
            SetValueFromLocalX(args.LocalX);
        }
    }

    Rune[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        if (width < 5)
        {
            width = 5;
        }

        var result = new Rune[1][];
        result[0] = new Rune[width];
        Array.Fill(result[0], new Rune(' '));

        double range = _max - _min;
        double normalizedValue = range > 0 ? (_value - _min) / range : 0;

        int valueTextLength = 0;
        string valueText = "";
        if (ShowValue)
        {
            // Always reserve space for the widest possible value text so the track
            // doesn't shift when the number of digits changes (e.g. 0 → 10 → 100).
            string minText = $" {_min:0.##}";
            string maxText = $" {_max:0.##}";
            int maxLen = Math.Max(minText.Length, maxText.Length);

            valueText = $" {_value:0.##}";
            valueTextLength = maxLen;
        }

        int trackWidth = width - valueTextLength;
        if (trackWidth < 3)
        {
            trackWidth = 3;
        }

        _lastTrackWidth = trackWidth;

        result[0][0] = new Rune('[');
        result[0][trackWidth - 1] = new Rune(']');

        int thumbPos = (int)((trackWidth - 3) * normalizedValue);
        thumbPos = Math.Clamp(thumbPos, 0, trackWidth - 3);

        for (int i = 1; i < trackWidth - 1; i++)
        {
            if (i - 1 == thumbPos)
            {
                result[0][i] = new Rune('●');
            }
            else if (i - 1 < thumbPos)
            {
                result[0][i] = new Rune('━');
            }
            else
            {
                result[0][i] = new Rune('─');
            }
        }

        if (ShowValue && valueTextLength > 0)
        {
            // Right-align the value text within the reserved space
            int padLen = valueTextLength - valueText.Length;
            for (int i = 0; i < valueText.Length && trackWidth + padLen + i < width; i++)
            {
                result[0][trackWidth + padLen + i] = new Rune(valueText[i]);
            }
        }

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        if (Disabled)
        {
            return;
        }

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

    /// <summary>
    /// Raises the ValueChanged event.
    /// </summary>
    protected virtual void OnChanged()
    {
        ValueChanged?.Invoke(this, _value);
    }

    /// <summary>
    /// Sets the slider value based on a local X coordinate relative to the widget's left edge.
    /// Called by the framework for mouse click/drag support.
    /// </summary>
    internal void SetValueFromLocalX(int localX)
    {
        if (Disabled || _lastTrackWidth < 3) return;

        // localX 0 = '[', localX trackWidth-1 = ']', inner positions are 1..trackWidth-2
        double ratio = Math.Clamp((localX - 1.0) / (_lastTrackWidth - 3), 0.0, 1.0);
        double raw = _min + ratio * (_max - _min);
        // Snap to Step grid so mouse input produces the same clean values as keyboard
        if (Step > 0)
            raw = Math.Round(raw / Step) * Step;
        Value = raw;
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

    /// <summary>
    /// Creates a copy of this slider.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for Slider).</param>
    /// <returns>A new Slider instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Slider
        {
            _value = _value,
            _min = _min,
            _max = _max,
            _backgroundColor = _backgroundColor,
            _foregroundColor = _foregroundColor,
            Step = Step,
            ShowValue = ShowValue,
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
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor,
            Disabled = Disabled,
            DisabledBackgroundColor = DisabledBackgroundColor,
            DisabledForegroundColor = DisabledForegroundColor
        };

        return clone;
    }
}

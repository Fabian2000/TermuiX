using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A checkbox widget that can be toggled on or off.
/// </summary>
public class Checkbox : IWidget
{
    private bool _checked = false;
    private ConsoleColor _backgroundColor = ConsoleColor.Black;
    private ConsoleColor _foregroundColor = ConsoleColor.White;

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is checked.
    /// </summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                OnChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the unique name of the checkbox.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the checkbox.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the checkbox.
    /// </summary>
    public string Width { get; set; } = "1ch";

    /// <summary>
    /// Gets or sets the height of the checkbox.
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
    /// Gets or sets a value indicating whether the checkbox is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public ConsoleColor BackgroundColor
    {
        get => Disabled && DisabledBackgroundColor.HasValue ? DisabledBackgroundColor.Value : _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public ConsoleColor ForegroundColor
    {
        get => Disabled ? DisabledForegroundColor : _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the background color when disabled.
    /// </summary>
    public ConsoleColor? DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color when disabled.
    /// </summary>
    public ConsoleColor DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets a value indicating whether the checkbox can receive focus.
    /// </summary>
    public bool CanFocus => true;

    /// <summary>
    /// Gets a value indicating whether the checkbox is scrollable.
    /// </summary>
    public bool Scrollable => false;

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
    /// Occurs when the checked state changes.
    /// </summary>
    public event EventHandler<bool>? CheckedChanged;

    Rune[][] IWidget.GetRaw()
    {
        var result = new Rune[1][];
        result[0] = new Rune[1];

        result[0][0] = _checked ? new Rune('☑') : new Rune('☐');

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        if (!Disabled && (keyInfo.Key == ConsoleKey.Spacebar || keyInfo.Key == ConsoleKey.Enter))
        {
            Checked = !Checked;
        }
    }

    void IWidget.MousePress(MouseEventArgs args)
    {
        if (!Disabled && args.EventType == MouseEventType.LeftButtonPressed)
        {
            Checked = !Checked;
        }
    }

    /// <summary>
    /// Raises the CheckedChanged event.
    /// </summary>
    protected virtual void OnChanged()
    {
        CheckedChanged?.Invoke(this, _checked);
    }

    /// <summary>
    /// Creates a copy of this checkbox.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for Checkbox).</param>
    /// <returns>A new Checkbox instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Checkbox
        {
            _checked = _checked,
            _backgroundColor = _backgroundColor,
            _foregroundColor = _foregroundColor,
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
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor,
            Disabled = Disabled,
            DisabledBackgroundColor = DisabledBackgroundColor,
            DisabledForegroundColor = DisabledForegroundColor
        };

        return clone;
    }
}

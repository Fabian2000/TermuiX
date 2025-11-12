using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A radio button widget that allows single selection within a group.
/// </summary>
public class RadioButton : IWidget
{
    private bool _selected = false;

    /// <summary>
    /// Gets or sets a value indicating whether the radio button is selected.
    /// </summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                OnChanged();

                if (_selected)
                {
                    UnselectSiblings();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the unique name of the radio button.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the radio button.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the radio button.
    /// </summary>
    public string Width { get; set; } = "1ch";

    /// <summary>
    /// Gets or sets the height of the radio button.
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
    /// Gets or sets a value indicating whether the radio button is visible.
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
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets a value indicating whether the radio button can receive focus.
    /// </summary>
    public bool CanFocus => true;

    /// <summary>
    /// Gets a value indicating whether the radio button is scrollable.
    /// </summary>
    public bool Scrollable => false;

    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    /// <summary>
    /// Occurs when the selected state changes.
    /// </summary>
    public event EventHandler<bool>? SelectionChanged;

    Rune[][] IWidget.GetRaw()
    {
        var result = new Rune[1][];
        result[0] = new Rune[1];

        result[0][0] = _selected ? new Rune('◉') : new Rune('○');

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Spacebar || keyInfo.Key == ConsoleKey.Enter)
        {
            Selected = true;
        }
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnChanged()
    {
        SelectionChanged?.Invoke(this, _selected);
    }

    private void UnselectSiblings()
    {
        var parent = ((IWidget)this).Parent;
        if (parent is null)
        {
            return;
        }

        foreach (var child in parent.Children)
        {
            if (child is RadioButton radio && radio != this)
            {
                radio._selected = false;
            }
        }
    }

    /// <summary>
    /// Creates a copy of this radio button.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (not applicable for RadioButton).</param>
    /// <returns>A new RadioButton instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new RadioButton
        {
            _selected = _selected,
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

        return clone;
    }
}

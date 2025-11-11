namespace Termui.Widgets;

public class RadioButton : IWidget
{
    private bool _selected = false;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                OnChanged();

                // If this radio button is selected, unselect all siblings
                if (_selected)
                {
                    UnselectSiblings();
                }
            }
        }
    }

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "1ch";  // ○ or ◉
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
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.DarkGray;
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
    public event EventHandler<bool>? Changed;

    char[][] IWidget.GetRaw()
    {
        var result = new char[1][];
        result[0] = new char[1];

        // Render radio button: ◉ or ○
        result[0][0] = _selected ? '◉' : '○';

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Space or Enter selects radio button
        if (keyInfo.Key == ConsoleKey.Spacebar || keyInfo.Key == ConsoleKey.Enter)
        {
            Selected = true;
        }
    }

    protected virtual void OnChanged()
    {
        Changed?.Invoke(this, _selected);
    }

    private void UnselectSiblings()
    {
        // Find parent container
        var parent = ((IWidget)this).Parent;
        if (parent == null) return;

        // Unselect all other RadioButtons in the same parent container
        foreach (var child in parent.Children)
        {
            if (child is RadioButton radio && radio != this)
            {
                radio._selected = false; // Set directly to avoid triggering Changed event
            }
        }
    }
}

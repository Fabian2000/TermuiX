namespace TermuiX.Widgets;

public class Checkbox : IWidget
{
    private bool _checked = false;

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

    // IWidget properties
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "1ch";  // ☐ or ☑
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

        // Render checkbox: ☑ or ☐
        result[0][0] = _checked ? '☑' : '☐';

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Space or Enter toggles checkbox
        if (keyInfo.Key == ConsoleKey.Spacebar || keyInfo.Key == ConsoleKey.Enter)
        {
            Checked = !Checked;
        }
    }

    protected virtual void OnChanged()
    {
        Changed?.Invoke(this, _checked);
    }
}

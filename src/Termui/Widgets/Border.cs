namespace Termui.Widgets;

public enum BorderStyle
{
    Single,
    Double
}

public class Border : IWidget
{
    private IWidget? _child;

    public Border()
    {
    }

    public Border(IWidget child)
    {
        _child = child;
    }

    public BorderStyle Style { get; set; } = BorderStyle.Single;

    public IWidget? Child
    {
        get => _child;
        set => _child = value;
    }

    public string? Name { get; set; }
    public string? Group { get; set; }
    public string Width { get; set; } = "100%";
    public string Height { get; set; } = "100%";
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
    List<IWidget> IWidget.Children => _child is not null ? [_child] : [];
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    char[][] IWidget.GetRaw()
    {
        // Border renders nothing itself - children render inside
        // The border should be drawn by having padding of 1ch on all sides
        // and the container itself draws the border characters
        // But we need to know the size... this is tricky

        // Actually, borders should be drawn by Container with a border property
        // Let's just return empty for now
        return [];
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Border doesn't handle key presses
    }

    // Helper to get border characters
    public (char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical) GetBorderChars()
    {
        return Style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            _ => ('┌', '┐', '└', '┘', '─', '│')
        };
    }
}

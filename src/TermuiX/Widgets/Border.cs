using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// Specifies the style of border to draw around a widget.
/// </summary>
public enum BorderStyle
{
    /// <summary>
    /// No border.
    /// </summary>
    None,

    /// <summary>
    /// Single-line border characters.
    /// </summary>
    Single,

    /// <summary>
    /// Double-line border characters.
    /// </summary>
    Double
}

/// <summary>
/// A border widget that wraps a single child widget with a border.
/// </summary>
public class Border : IWidget
{
    private IWidget? _child;

    /// <summary>
    /// Initializes a new instance of the <see cref="Border"/> class.
    /// </summary>
    public Border()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Border"/> class with a child widget.
    /// </summary>
    /// <param name="child">The child widget to wrap.</param>
    public Border(IWidget child)
    {
        _child = child;
    }

    /// <summary>
    /// Gets or sets the border style.
    /// </summary>
    public BorderStyle Style { get; set; } = BorderStyle.Single;

    /// <summary>
    /// Gets or sets the child widget.
    /// </summary>
    public IWidget? Child
    {
        get => _child;
        set => _child = value;
    }

    /// <summary>
    /// Gets or sets the unique name of the border.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the border.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the border.
    /// </summary>
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Gets or sets the height of the border.
    /// </summary>
    public string Height { get; set; } = "100%";
    /// <summary>Gets or sets the minimum width constraint.</summary>
    public string MinWidth { get; set; } = "";
    /// <summary>Gets or sets the maximum width constraint.</summary>
    public string MaxWidth { get; set; } = "";
    /// <summary>Gets or sets the minimum height constraint.</summary>
    public string MinHeight { get; set; } = "";
    /// <summary>Gets or sets the maximum height constraint.</summary>
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
    /// Gets or sets a value indicating whether the border is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets a value indicating whether the border can receive focus.
    /// </summary>
    public bool CanFocus => false;

    /// <summary>
    /// Gets a value indicating whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets a value indicating whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => _child is not null ? [_child] : [];
    bool IWidget.Focussed { get; set; }
    bool IWidget.Hovered { get; set; }
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }
    bool IWidget.Disabled { get; set; }
    Color? IWidget.DisabledBackgroundColor { get; set; }
    Color IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    Rune[][] IWidget.GetRaw()
    {
        return [];
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }

    /// <summary>
    /// Gets the border characters for the current style.
    /// </summary>
    /// <returns>A tuple containing the border characters.</returns>
    public (char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical) GetBorderChars()
    {
        return Style switch
        {
            BorderStyle.None => (' ', ' ', ' ', ' ', ' ', ' '),
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            _ => ('┌', '┐', '└', '┘', '─', '│')
        };
    }

    /// <summary>
    /// Creates a copy of this border widget.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (clone child widget).</param>
    /// <returns>A new Border instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Border
        {
            Style = Style,
            Child = deep && _child is not null ? null : _child, // Note: Child widget is not cloned in deep mode, just referenced
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
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor
        };

        return clone;
    }
}

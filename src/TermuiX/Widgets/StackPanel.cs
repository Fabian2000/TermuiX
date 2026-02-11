using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// Specifies the layout direction for a StackPanel.
/// </summary>
public enum StackDirection
{
    /// <summary>
    /// Children are stacked vertically (top to bottom).
    /// </summary>
    Vertical,

    /// <summary>
    /// Children are stacked horizontally (left to right).
    /// </summary>
    Horizontal
}

/// <summary>
/// Specifies how children are distributed along the stack direction.
/// </summary>
public enum StackJustify
{
    /// <summary>
    /// Children are packed at the start (default).
    /// </summary>
    Start,

    /// <summary>
    /// Children are packed at the end.
    /// </summary>
    End,

    /// <summary>
    /// Children are centered.
    /// </summary>
    Center,

    /// <summary>
    /// Equal spacing between children, no space at edges.
    /// </summary>
    SpaceBetween,

    /// <summary>
    /// Equal spacing around children (half-size space at edges).
    /// </summary>
    SpaceAround,

    /// <summary>
    /// Equal spacing between and around children.
    /// </summary>
    SpaceEvenly
}

/// <summary>
/// Specifies how children are aligned on the cross axis.
/// </summary>
public enum StackAlign
{
    /// <summary>
    /// Children are aligned at the start of the cross axis (default).
    /// </summary>
    Start,

    /// <summary>
    /// Children are centered on the cross axis.
    /// </summary>
    Center,

    /// <summary>
    /// Children are aligned at the end of the cross axis.
    /// </summary>
    End
}

/// <summary>
/// A container that automatically arranges its children in a vertical or horizontal stack.
/// Children do not need explicit PositionX/PositionY — the StackPanel computes them
/// based on each child's size and margin.
/// </summary>
public class StackPanel : Container
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StackPanel"/> class with auto-sizing defaults.
    /// </summary>
    public StackPanel()
    {
        Width = "auto";
        Height = "auto";
    }

    /// <summary>
    /// Gets or sets the direction in which children are stacked.
    /// Default is Vertical.
    /// </summary>
    public StackDirection Direction { get; set; } = StackDirection.Vertical;

    /// <summary>
    /// Gets or sets how children are distributed along the stack direction.
    /// Requires a fixed size (not auto) to have effect.
    /// Default is Start.
    /// </summary>
    public StackJustify Justify { get; set; } = StackJustify.Start;

    /// <summary>
    /// Gets or sets how children are aligned on the cross axis.
    /// Default is Start.
    /// </summary>
    public StackAlign Align { get; set; } = StackAlign.Start;

    /// <summary>
    /// Gets or sets whether children wrap to the next line/column when they exceed the available space.
    /// Default is false.
    /// </summary>
    public bool Wrap { get; set; } = false;

    /// <summary>
    /// Creates a deep or shallow copy of the stack panel.
    /// </summary>
    public new IWidget Clone(bool deep = true)
    {
        var clone = new StackPanel
        {
            Name = Name,
            Group = Group,
            Width = Width,
            Height = Height,
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
            FocusForegroundColor = FocusForegroundColor,
            BorderStyle = BorderStyle,
            RoundedCorners = RoundedCorners,
            ScrollX = ScrollX,
            ScrollY = ScrollY,
            Direction = Direction,
            Justify = Justify,
            Align = Align,
            Wrap = Wrap
        };

        if (deep)
        {
            foreach (var child in ((IWidget)this).Children)
            {
                clone.Add(child.Clone(true));
            }
        }

        return clone;
    }
}

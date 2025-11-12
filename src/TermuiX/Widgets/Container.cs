using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A container widget that can hold and organize child widgets.
/// </summary>
public class Container : IWidget
{
    private readonly List<IWidget> _children = [];

    /// <summary>
    /// Gets or sets the unique name of the container.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the container.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the container.
    /// </summary>
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Gets or sets the height of the container.
    /// </summary>
    public string Height { get; set; } = "100%";

    /// <summary>
    /// Gets or sets the left padding of the container.
    /// </summary>
    public string PaddingLeft { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the top padding of the container.
    /// </summary>
    public string PaddingTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right padding of the container.
    /// </summary>
    public string PaddingRight { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the bottom padding of the container.
    /// </summary>
    public string PaddingBottom { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the X position of the container.
    /// </summary>
    public string PositionX { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the Y position of the container.
    /// </summary>
    public string PositionY { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets a value indicating whether the container is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the background color of the container.
    /// </summary>
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground color of the container.
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
    /// Gets or sets a value indicating whether the container can receive focus.
    /// </summary>
    public bool CanFocus { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the container is scrollable.
    /// </summary>
    public bool Scrollable { get; set; } = false;

    /// <summary>
    /// Gets or sets the border style for the container.
    /// </summary>
    public BorderStyle? BorderStyle { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the border corners should be rounded.
    /// </summary>
    public bool RoundedCorners { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the container has a border.
    /// </summary>
    public bool HasBorder => BorderStyle.HasValue;

    // Explicit interface implementation to hide these members
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => _children;
    bool IWidget.Focussed { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }

    /// <summary>
    /// Adds a child widget to the container.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    public void Add(IWidget widget)
    {
        widget.Parent = this;
        _children.Add(widget);
    }

    /// <summary>
    /// Adds child widgets from an XML string to the container.
    /// The XML can contain multiple root-level widgets which will all be added.
    /// Each widget is cloned to ensure no shared references.
    /// </summary>
    /// <param name="xml">The XML string containing widget definitions.</param>
    public void Add(string xml)
    {
        // Wrap in a temporary container to allow multiple root elements
        var wrappedXml = $"<Container>{xml}</Container>";
        var tempContainer = XmlParser.Parse(wrappedXml) as Container;

        if (tempContainer != null)
        {
            foreach (var child in ((IWidget)tempContainer).Children)
            {
                // Clone to avoid shared references
                Add(child.Clone(true));
            }
        }
    }

    /// <summary>
    /// Removes a child widget from the container.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    public void Remove(IWidget widget)
    {
        widget.Parent = null;
        _children.Remove(widget);
    }

    /// <summary>
    /// Removes all child widgets from the container.
    /// </summary>
    public void Clear()
    {
        _children.Clear();
    }

    Rune[][] IWidget.GetRaw()
    {
        int actualWidth = CalculateSize(Width, ((IWidget)this).Parent?.Width, true);
        int actualHeight = CalculateSize(Height, ((IWidget)this).Parent?.Height, false);

        if (actualWidth <= 0 || actualHeight <= 0)
        {
            return [];
        }

        var result = new Rune[actualHeight][];
        for (int i = 0; i < actualHeight; i++)
        {
            result[i] = new Rune[actualWidth];
            Array.Fill(result[i], new Rune(' '));
        }

        if (HasBorder)
        {
            var (topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical) = GetBorderChars();

            result[0][0] = new Rune(topLeft);
            result[0][actualWidth - 1] = new Rune(topRight);
            for (int x = 1; x < actualWidth - 1; x++)
            {
                result[0][x] = new Rune(horizontal);
            }

            int lastY = actualHeight - 1;
            result[lastY][0] = new Rune(bottomLeft);
            result[lastY][actualWidth - 1] = new Rune(bottomRight);
            for (int x = 1; x < actualWidth - 1; x++)
            {
                result[lastY][x] = new Rune(horizontal);
            }

            for (int y = 1; y < actualHeight - 1; y++)
            {
                result[y][0] = new Rune(vertical);
                result[y][actualWidth - 1] = new Rune(vertical);
            }
        }

        return result;
    }

    private int CalculateSize(string size, string? parentSize, bool isWidth)
    {
        if (string.IsNullOrEmpty(size))
        {
            return 0;
        }

        size = size.Trim();

        if (size.EndsWith("ch"))
        {
            var value = size[..^2].Trim();
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return 0;
        }
        else if (size.EndsWith('%'))
        {
            if (string.IsNullOrEmpty(parentSize))
            {
                return 0;
            }

            var parent = ((IWidget)this).Parent;
            int parentSizeValue = CalculateSize(parentSize, parent?.Parent?.Width, isWidth);

            if (parent is not null)
            {
                if (isWidth)
                {
                    int padLeft = ParsePadding(parent.PaddingLeft);
                    int padRight = ParsePadding(parent.PaddingRight);
                    parentSizeValue = Math.Max(0, parentSizeValue - padLeft - padRight);
                }
                else
                {
                    int padTop = ParsePadding(parent.PaddingTop);
                    int padBottom = ParsePadding(parent.PaddingBottom);
                    parentSizeValue = Math.Max(0, parentSizeValue - padTop - padBottom);
                }
            }

            var value = size[..^1].Trim();
            if (float.TryParse(value, out float percent))
            {
                return (int)(parentSizeValue * percent / 100.0f);
            }
            return 0;
        }

        return 0;
    }

    private static int ParsePadding(string padding)
    {
        if (string.IsNullOrEmpty(padding))
        {
            return 0;
        }

        padding = padding.Trim();
        if (padding.EndsWith("ch"))
        {
            var value = padding[..^2].Trim();
            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }

        return 0;
    }

    private (char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical) GetBorderChars()
    {
        if (RoundedCorners)
        {
            return BorderStyle switch
            {
                Widgets.BorderStyle.Single => ('╭', '╮', '╰', '╯', '─', '│'),
                Widgets.BorderStyle.Double => ('╭', '╮', '╰', '╯', '═', '║'),
                _ => ('╭', '╮', '╰', '╯', '─', '│')
            };
        }

        return BorderStyle switch
        {
            Widgets.BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            Widgets.BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            _ => ('┌', '┐', '└', '┘', '─', '│')
        };
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
    }

    /// <summary>
    /// Creates a deep or shallow copy of the container.
    /// </summary>
    /// <param name="deep">If true, recursively clones all children. If false, children are not cloned.</param>
    /// <returns>A new container instance with copied properties and no parent reference.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Container
        {
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
            FocusForegroundColor = FocusForegroundColor,
            BorderStyle = BorderStyle,
            RoundedCorners = RoundedCorners,
            Scrollable = Scrollable
        };

        // Deep clone: recursively clone all children
        if (deep)
        {
            foreach (var child in _children)
            {
                clone.Add(child.Clone(true));
            }
        }

        return clone;
    }
}

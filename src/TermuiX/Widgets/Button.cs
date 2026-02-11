using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A clickable button widget with customizable appearance and event handling.
/// </summary>
public class Button : IWidget
{
    private readonly Container _container;
    private readonly Text _textWidget;

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class with optional text.
    /// </summary>
    /// <param name="text">The button text.</param>
    public Button(string text = "")
    {
        _container = new Container
        {
            BorderStyle = Widgets.BorderStyle.Single,
            PaddingLeft = "1ch",
            PaddingRight = "1ch",
            PaddingTop = "1ch",
            PaddingBottom = "1ch",
            Width = "100%",
            Height = "100%"
        };

        _textWidget = new Text(text)
        {
            AllowWrapping = false,
            TextAlign = TextAlign.Center
        };

        _textWidget.FocusBackgroundColor = _container.FocusBackgroundColor;
        _textWidget.FocusForegroundColor = _container.FocusForegroundColor;
        _textWidget.BackgroundColor = _container.BackgroundColor;
        _textWidget.ForegroundColor = _container.ForegroundColor;

        _container.Add(_textWidget);
    }

    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string Text
    {
        get => _textWidget.Content;
        set => _textWidget.Content = value;
    }

    /// <summary>
    /// Gets or sets the border style of the button.
    /// </summary>
    public BorderStyle? BorderStyle
    {
        get => _container.BorderStyle;
        set => _container.BorderStyle = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the border corners should be rounded.
    /// </summary>
    public bool RoundedCorners
    {
        get => _container.RoundedCorners;
        set => _container.RoundedCorners = value;
    }

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public ConsoleColor BorderColor
    {
        get => _container.ForegroundColor;
        set => _container.ForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    public ConsoleColor TextColor
    {
        get => _textWidget.ForegroundColor;
        set => _textWidget.ForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the border color when focused.
    /// </summary>
    public ConsoleColor FocusBorderColor
    {
        get => _container.FocusForegroundColor;
        set => _container.FocusForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the text color when focused.
    /// </summary>
    public ConsoleColor FocusTextColor
    {
        get => _textWidget.FocusForegroundColor;
        set
        {
            _textWidget.FocusForegroundColor = value;
            _container.FocusForegroundColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the visual style of the button text.
    /// </summary>
    public TextStyle TextStyle
    {
        get => _textWidget.Style;
        set => _textWidget.Style = value;
    }

    /// <summary>
    /// Gets or sets the text alignment of the button.
    /// </summary>
    public TextAlign TextAlign
    {
        get => _textWidget.TextAlign;
        set => _textWidget.TextAlign = value;
    }

    /// <summary>
    /// Gets or sets the unique name of the button.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name of the button.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width of the button.
    /// </summary>
    public string Width
    {
        get => _container.Width;
        set => _container.Width = value;
    }

    /// <summary>
    /// Gets or sets the height of the button.
    /// </summary>
    public string Height
    {
        get => _container.Height;
        set => _container.Height = value;
    }

    /// <summary>
    /// Gets or sets the left padding.
    /// </summary>
    public string PaddingLeft
    {
        get => _container.PaddingLeft;
        set => _container.PaddingLeft = value;
    }

    /// <summary>
    /// Gets or sets the top padding.
    /// </summary>
    public string PaddingTop
    {
        get => _container.PaddingTop;
        set => _container.PaddingTop = value;
    }

    /// <summary>
    /// Gets or sets the right padding.
    /// </summary>
    public string PaddingRight
    {
        get => _container.PaddingRight;
        set => _container.PaddingRight = value;
    }

    /// <summary>
    /// Gets or sets the bottom padding.
    /// </summary>
    public string PaddingBottom
    {
        get => _container.PaddingBottom;
        set => _container.PaddingBottom = value;
    }

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
    public string PositionX
    {
        get => _container.PositionX;
        set => _container.PositionX = value;
    }

    /// <summary>
    /// Gets or sets the Y position.
    /// </summary>
    public string PositionY
    {
        get => _container.PositionY;
        set => _container.PositionY = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the button is visible.
    /// </summary>
    public bool Visible
    {
        get => _container.Visible;
        set => _container.Visible = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping
    {
        get => _container.AllowWrapping;
        set => _container.AllowWrapping = value;
    }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public ConsoleColor BackgroundColor
    {
        get => _container.BackgroundColor;
        set
        {
            _container.BackgroundColor = value;
            _textWidget.BackgroundColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public ConsoleColor ForegroundColor
    {
        get => _container.ForegroundColor;
        set => _container.ForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public ConsoleColor FocusBackgroundColor
    {
        get => _container.FocusBackgroundColor;
        set
        {
            _container.FocusBackgroundColor = value;
            _textWidget.FocusBackgroundColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public ConsoleColor FocusForegroundColor
    {
        get => _container.FocusForegroundColor;
        set => _container.FocusForegroundColor = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the button is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the background color when disabled.
    /// If null, the normal background color is used.
    /// </summary>
    public ConsoleColor? DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color when disabled.
    /// </summary>
    public ConsoleColor DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets a value indicating whether the button can receive focus.
    /// </summary>
    public bool CanFocus => !Disabled;

    /// <summary>
    /// Gets a value indicating whether the button is scrollable.
    /// </summary>
    public bool Scrollable => false;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => ((IWidget)_container).Children;
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }

    bool IWidget.Focussed
    {
        get => ((IWidget)_container).Focussed;
        set
        {
            ((IWidget)_container).Focussed = value;
            ((IWidget)_textWidget).Focussed = value;
        }
    }

    bool IWidget.Hovered
    {
        get => ((IWidget)_container).Hovered;
        set
        {
            ((IWidget)_container).Hovered = value;
            ((IWidget)_textWidget).Hovered = value;
        }
    }

    long IWidget.ScrollOffsetX
    {
        get => ((IWidget)_container).ScrollOffsetX;
        set => ((IWidget)_container).ScrollOffsetX = value;
    }

    long IWidget.ScrollOffsetY
    {
        get => ((IWidget)_container).ScrollOffsetY;
        set => ((IWidget)_container).ScrollOffsetY = value;
    }

    Rune[][] IWidget.GetRaw()
    {
        // Set container's parent to this widget's parent for proper size calculation
        ((IWidget)_container).Parent = ((IWidget)this).Parent;

        // If the StackPanel measurement pass already computed our size, use that
        // instead of letting the inner container recalculate from parent %
        if (((IWidget)this).ComputedWidth > 0)
            _container.Width = $"{((IWidget)this).ComputedWidth}ch";
        if (((IWidget)this).ComputedHeight > 0)
            _container.Height = $"{((IWidget)this).ComputedHeight}ch";

        // Propagate disabled state to inner widgets so they render with disabled colors
        ((IWidget)_container).Disabled = Disabled;
        ((IWidget)_container).DisabledForegroundColor = DisabledForegroundColor;
        ((IWidget)_container).DisabledBackgroundColor = DisabledBackgroundColor;

        ((IWidget)_textWidget).Disabled = Disabled;
        ((IWidget)_textWidget).DisabledForegroundColor = DisabledForegroundColor;
        ((IWidget)_textWidget).DisabledBackgroundColor = DisabledBackgroundColor;

        var result = ((IWidget)_container).GetRaw();

        // Store computed values from container
        ((IWidget)this).ComputedWidth = ((IWidget)_container).ComputedWidth;
        ((IWidget)this).ComputedHeight = ((IWidget)_container).ComputedHeight;

        return result;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        if (!Disabled && (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Spacebar))
        {
            OnClick();
        }
    }

    void IWidget.MousePress(MouseEventArgs args)
    {
        if (!Disabled && args.EventType == MouseEventType.LeftButtonPressed)
        {
            OnClick();
        }
        else if (!Disabled && args.EventType == MouseEventType.RightButtonPressed)
        {
            OnRightClick(args);
        }
    }

    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    public event EventHandler? Click;

    /// <summary>
    /// Occurs when the button is right-clicked. Provides mouse coordinates for context menu positioning.
    /// </summary>
    public event EventHandler<MouseEventArgs>? RightClick;

    /// <summary>
    /// Raises the Click event.
    /// </summary>
    protected virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the RightClick event.
    /// </summary>
    protected virtual void OnRightClick(MouseEventArgs args)
    {
        RightClick?.Invoke(this, args);
    }

    /// <summary>
    /// Creates a copy of this button.
    /// </summary>
    /// <param name="deep">Whether to perform a deep clone (clone child widgets).</param>
    /// <returns>A new Button instance with copied properties.</returns>
    public IWidget Clone(bool deep = true)
    {
        var clone = new Button(Text)
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
            BorderColor = BorderColor,
            TextColor = TextColor,
            FocusBorderColor = FocusBorderColor,
            FocusTextColor = FocusTextColor,
            TextStyle = TextStyle,
            TextAlign = TextAlign,
            Disabled = Disabled,
            DisabledBackgroundColor = DisabledBackgroundColor,
            DisabledForegroundColor = DisabledForegroundColor
        };

        return clone;
    }
}

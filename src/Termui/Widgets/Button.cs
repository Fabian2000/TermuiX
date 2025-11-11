namespace Termui.Widgets;

public class Button : IWidget
{
    private readonly Container _container;
    private readonly Text _textWidget;

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

        // Sync focus colors between container and text
        _textWidget.FocusBackgroundColor = _container.FocusBackgroundColor;
        _textWidget.FocusForegroundColor = _container.FocusForegroundColor;
        _textWidget.BackgroundColor = _container.BackgroundColor;
        _textWidget.ForegroundColor = _container.ForegroundColor;

        _container.Add(_textWidget);
    }

    public string Text
    {
        get => _textWidget.Content;
        set => _textWidget.Content = value;
    }

    public BorderStyle BorderStyle
    {
        get => _container.BorderStyle ?? Widgets.BorderStyle.Single;
        set => _container.BorderStyle = value;
    }

    public ConsoleColor BorderColor
    {
        get => _container.ForegroundColor;
        set => _container.ForegroundColor = value;
    }

    public ConsoleColor TextColor
    {
        get => _textWidget.ForegroundColor;
        set => _textWidget.ForegroundColor = value;
    }

    public ConsoleColor FocusBorderColor
    {
        get => _container.FocusForegroundColor;
        set => _container.FocusForegroundColor = value;
    }

    public ConsoleColor FocusTextColor
    {
        get => _textWidget.FocusForegroundColor;
        set
        {
            _textWidget.FocusForegroundColor = value;
            _container.FocusForegroundColor = value;
        }
    }

    // IWidget implementation - delegate to container
    public string? Name { get; set; }
    public string? Group { get; set; }

    public string Width
    {
        get => _container.Width;
        set => _container.Width = value;
    }

    public string Height
    {
        get => _container.Height;
        set => _container.Height = value;
    }

    public string PaddingLeft
    {
        get => _container.PaddingLeft;
        set => _container.PaddingLeft = value;
    }

    public string PaddingTop
    {
        get => _container.PaddingTop;
        set => _container.PaddingTop = value;
    }

    public string PaddingRight
    {
        get => _container.PaddingRight;
        set => _container.PaddingRight = value;
    }

    public string PaddingBottom
    {
        get => _container.PaddingBottom;
        set => _container.PaddingBottom = value;
    }

    public string PositionX
    {
        get => _container.PositionX;
        set => _container.PositionX = value;
    }

    public string PositionY
    {
        get => _container.PositionY;
        set => _container.PositionY = value;
    }

    public bool Visible
    {
        get => _container.Visible;
        set => _container.Visible = value;
    }

    public bool AllowWrapping
    {
        get => _container.AllowWrapping;
        set => _container.AllowWrapping = value;
    }

    public ConsoleColor BackgroundColor
    {
        get => _container.BackgroundColor;
        set
        {
            _container.BackgroundColor = value;
            _textWidget.BackgroundColor = value;
        }
    }

    public ConsoleColor ForegroundColor
    {
        get => _container.ForegroundColor;
        set => _container.ForegroundColor = value;
    }

    public ConsoleColor FocusBackgroundColor
    {
        get => _container.FocusBackgroundColor;
        set
        {
            _container.FocusBackgroundColor = value;
            _textWidget.FocusBackgroundColor = value;
        }
    }

    public ConsoleColor FocusForegroundColor
    {
        get => _container.FocusForegroundColor;
        set => _container.FocusForegroundColor = value;
    }

    public bool CanFocus => true;
    public bool Scrollable => false;

    // Explicit interface implementation
    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => ((IWidget)_container).Children;

    bool IWidget.Focussed
    {
        get => ((IWidget)_container).Focussed;
        set
        {
            ((IWidget)_container).Focussed = value;
            ((IWidget)_textWidget).Focussed = value;
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

    char[][] IWidget.GetRaw()
    {
        return ((IWidget)_container).GetRaw();
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        // Button can handle clicks/enter in the future
        if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Spacebar)
        {
            // Trigger button action
            OnClick();
        }
    }

    // Event for button clicks
    public event EventHandler? Click;

    protected virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}

using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager.Components;

public class Sidebar
{
    private const int SidebarWidth = 40;
    private const int AnimationStep = 1;
    private const int AnimationDelayMs = 16;

    private readonly TermuiXLib _termui;
    private Container? _container;
    private Button? _closeButton;

    public bool IsOpen { get; private set; }

    public Sidebar(TermuiXLib termui)
    {
        _termui = termui;
        IsOpen = false;
    }

    public string BuildXml()
    {
        return $@"
<Container
    Name='sidebar'
    Width='{SidebarWidth}ch'
    Height='100%'
    PositionX='-{SidebarWidth}ch'
    PositionY='0ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    BorderStyle='Single'
    RoundedCorners='true'>

    <Button
        Name='btnCloseSidebar'
        PositionX='27ch'
        PositionY='0ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'
        RoundedCorners='true'>
        ✕ Close
    </Button>
</Container>";
    }

    public void Initialize()
    {
        _container = _termui.GetWidget<Container>("sidebar");
        _closeButton = _termui.GetWidget<Button>("btnCloseSidebar");

        if (_closeButton is not null)
        {
            _closeButton.Click += OnClose;
        }
    }

    public async Task OpenAsync()
    {
        if (IsOpen || _container is null)
        {
            return;
        }

        IsOpen = true;
        int currentPos = -SidebarWidth;
        int targetPos = 0;

        while (currentPos < targetPos)
        {
            currentPos += AnimationStep;
            if (currentPos > targetPos)
            {
                currentPos = targetPos;
            }

            _container.PositionX = $"{currentPos}ch";
            await Task.Delay(AnimationDelayMs);
        }
    }

    public async Task CloseAsync()
    {
        if (!IsOpen || _container is null)
        {
            return;
        }

        IsOpen = false;
        int currentPos = 0;
        int targetPos = -SidebarWidth;

        while (currentPos > targetPos)
        {
            currentPos -= AnimationStep;
            if (currentPos < targetPos)
            {
                currentPos = targetPos;
            }

            _container.PositionX = $"{currentPos}ch";
            await Task.Delay(AnimationDelayMs);
        }
    }

    public void UpdateLayout()
    {
    }

    public Button? GetCloseButton() => _closeButton;

    private void OnClose(object? sender, EventArgs e)
    {
        _ = CloseAsync();
    }
}

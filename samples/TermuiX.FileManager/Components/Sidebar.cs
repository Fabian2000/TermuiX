using System.Timers;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;
using Timer = System.Timers.Timer;

namespace TermuiX.FileManager.Components;

public class Sidebar
{
    private const int SidebarWidth = 40;
    private const int AnimationStep = 1;
    private const int AnimationDelayMs = 16;

    private readonly TermuiXLib _termui;
    private Container? _container;
    private Container? _contentContainer;
    private Button? _closeButton;
    private Input? _searchDriveInput;
    private List<string> _cachedDrives = [];
    private Timer? _driveWatcher;
    private readonly Button? _burgerButton;

    public bool IsOpen { get; private set; }

    public Sidebar(TermuiXLib termui, Button? burgerButton = null)
    {
        _termui = termui;
        _burgerButton = burgerButton;
        IsOpen = false;
        InitializeDriveWatcher();
    }

    private void InitializeDriveWatcher()
    {
        UpdateDriveCache();

        _driveWatcher = new Timer(2000); // Check every 2 seconds
        _driveWatcher.Elapsed += OnDriveCheck;
        _driveWatcher.AutoReset = true;
        _driveWatcher.Start();
    }

    private void UpdateDriveCache()
    {
        _cachedDrives = [.. DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => d.Name)
            .OrderBy(n => n)];
    }

    private void OnDriveCheck(object? sender, ElapsedEventArgs e)
    {
        List<string> currentDrives = [.. DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => d.Name)
            .OrderBy(n => n)];

        if (!_cachedDrives.SequenceEqual(currentDrives))
        {
            _cachedDrives = currentDrives;
            RefreshDriveButtons();
        }
    }

    private void RefreshDriveButtons(string? searchFilter = null)
    {
        if (_contentContainer is null)
        {
            return;
        }

        // Remove all existing drive buttons
        List<IWidget> existingButtons = [.. ((IWidget)_contentContainer).Children
            .Where(w => w is Button btn && btn.Name?.StartsWith("driveBtn") == true)];

        foreach (var button in existingButtons)
        {
            _contentContainer.Remove(button);
        }

        // Filter drives based on search text
        var filteredDrives = _cachedDrives;
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            filteredDrives = [.. _cachedDrives
                .Where(driveName =>
                {
                    var driveInfo = new DriveInfo(driveName);
                    string label = driveName;
                    if (!string.IsNullOrEmpty(driveInfo.VolumeLabel))
                    {
                        label = $"{driveInfo.VolumeLabel} ({driveName})";
                    }
                    return label.Contains(searchFilter, StringComparison.OrdinalIgnoreCase);
                })];
        }

        // Add new drive buttons
        int yPosition = 0;
        int buttonIndex = 0;

        foreach (var driveName in filteredDrives)
        {
            var driveInfo = new DriveInfo(driveName);
            string label = driveName;

            if (!string.IsNullOrEmpty(driveInfo.VolumeLabel))
            {
                label = $"{driveInfo.VolumeLabel} ({driveName})";
            }

            var button = new Button(label)
            {
                Name = $"driveBtn{buttonIndex}",
                PositionX = "0ch",
                PositionY = $"{yPosition}ch",
                Width = "37ch",
                Height = "3ch",
                TextAlign = TextAlign.Left,
                BorderStyle = BorderStyle.None,
                BackgroundColor = ConsoleColor.Black,
                ForegroundColor = ConsoleColor.White,
                FocusBackgroundColor = ConsoleColor.DarkGray,
                FocusForegroundColor = ConsoleColor.White
            };

            _contentContainer.Add(button);

            yPosition += 3;
            buttonIndex++;
        }
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

    <Text
        Name='lblTitle'
        PositionX='2ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        Style='Bold'>
        FileManager
    </Text>

    <Text
        Name='lblShortcutClose'
        PositionX='21ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+B
    </Text>

    <Container
        Name='searchDriveContainer'
        Width='38ch'
        Height='3ch'
        PositionX='0ch'
        PositionY='4ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'>

        <Input
            Name='inputSearchDrive'
            Width='35ch'
            Height='1ch'
            PositionX='0ch'
            PositionY='0ch'
            BackgroundColor='Black'
            ForegroundColor='White'
            FocusBackgroundColor='Black'
            FocusForegroundColor='White'
            Placeholder='Search in Drives (Ctrl+F)'
            MultiLine='false'>
        </Input>
    </Container>

    <Line
        PositionX='0ch'
        PositionY='7ch'
        Width='100%'
        BackgroundColor='Black'
        ForegroundColor='White' />

    <Container
        Name='sidebarContent'
        PositionX='0ch'
        PositionY='8ch'
        Width='100%'
        Height='10ch'
        BackgroundColor='Black'
        Scrollable='true'>
    </Container>
</Container>";
    }

    public void Initialize()
    {
        _container = _termui.GetWidget<Container>("sidebar");
        _contentContainer = _termui.GetWidget<Container>("sidebarContent");
        _closeButton = _termui.GetWidget<Button>("btnCloseSidebar");
        _searchDriveInput = _termui.GetWidget<Input>("inputSearchDrive");

        if (_closeButton is not null)
        {
            _closeButton.Click += OnClose;
        }

        if (_searchDriveInput is not null)
        {
            _searchDriveInput.TextChanged += OnSearchTextChanged;
        }

        UpdateLayout();
        RefreshDriveButtons();
    }

    public async Task OpenAsync()
    {
        if (IsOpen || _container is null)
        {
            return;
        }

        IsOpen = true;
        UpdateFocusability();

        if (_closeButton is not null)
        {
            _termui.SetFocus(_closeButton);
        }

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

        if (_burgerButton is not null)
        {
            _termui.SetFocus(_burgerButton);
        }

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

        UpdateFocusability();
    }

    public void UpdateLayout()
    {
        if (_contentContainer is not null)
        {
            // Console.WindowHeight - Header (8ch) - Top Border (1ch) - Bottom Border (1ch)
            int contentHeight = Console.WindowHeight - 8 - 2;
            _contentContainer.Height = $"{contentHeight}ch";
        }
    }

    public Button? GetCloseButton() => _closeButton;

    public Input? GetSearchDriveInput() => _searchDriveInput;

    public bool ContainsWidget(IWidget widget)
    {
        if (_container is null)
        {
            return false;
        }

        IWidget? current = widget;
        while (current is not null)
        {
            if (current == _container)
            {
                return true;
            }
            current = current.Parent;
        }

        return false;
    }

    private void OnClose(object? sender, EventArgs e)
    {
        _ = CloseAsync();
    }

    private void OnSearchTextChanged(object? sender, string newValue)
    {
        RefreshDriveButtons(newValue);
    }

    private void UpdateFocusability()
    {
        if (_container is null)
        {
            return;
        }

        // Make container invisible when closed to prevent focus
        _container.Visible = IsOpen;
    }
}

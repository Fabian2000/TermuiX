using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager.Components;

public class TopBar
{
    private const int TopBarHeight = 3;
    private const int SearchInputWidth = 30;

    private readonly TermuiXLib _termui;
    private Button? _burgerButton;
    private Input? _searchInput;
    private Button? _filterButton;
    private Button? _mouseToggleButton;
    private Text? _fpsLabel;

    public TopBar(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return $@"
<Container
    Name='topBar'
    Width='100%'
    Height='{TopBarHeight}ch'
    PositionX='0ch'
    PositionY='0ch'
    BackgroundColor='Black'
    ForegroundColor='White'>

    <Button
        Name='btnBurger'
        PositionX='0ch'
        PositionY='0ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='Black'
        RoundedCorners='true'>
        ≡ Menu
    </Button>

    <Text
        Name='lblShortcut'
        PositionX='10ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+B
    </Text>

    <Button
        Name='btnMouseToggle'
        PositionX='18ch'
        PositionY='0ch'
        BackgroundColor='Black'
        ForegroundColor='Green'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='Green'
        RoundedCorners='true'>
        🖱 Mouse: OFF
    </Button>

    <Text
        Name='lblFps'
        PositionX='39ch'
        PositionY='1ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkYellow'>
        -- FPS
    </Text>

    <Container
        Name='searchContainer'
        Width='{SearchInputWidth + 2}ch'
        Height='3ch'
        PositionX='50ch'
        PositionY='0ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'>

        <Input
            Name='inputSearch'
            Width='{SearchInputWidth}ch'
            Height='1ch'
            PositionX='0ch'
            PositionY='0ch'
            BackgroundColor='Black'
            ForegroundColor='White'
            FocusBackgroundColor='Black'
            FocusForegroundColor='White'
            Placeholder='Search...'
            MultiLine='false'>
        </Input>
    </Container>

    <Text
        Name='lblShortcutSearch'
        PositionX='50ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+F
    </Text>

    <Button
        Name='btnFilter'
        PositionX='85ch'
        PositionY='0ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='Black'
        RoundedCorners='true'>
        ⧉ Filter
    </Button>

    <Text
        Name='lblShortcutFilter'
        PositionX='85ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+E
    </Text>
</Container>";
    }

    public void Initialize()
    {
        _burgerButton = _termui.GetWidget<Button>("btnBurger");
        _searchInput = _termui.GetWidget<Input>("inputSearch");
        _filterButton = _termui.GetWidget<Button>("btnFilter");
        _mouseToggleButton = _termui.GetWidget<Button>("btnMouseToggle");
        _fpsLabel = _termui.GetWidget<Text>("lblFps");

        if (_mouseToggleButton is not null)
        {
            _mouseToggleButton.Click += (_, _) => ToggleMouse();
        }

        UpdateMouseToggleLabel();
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        var searchContainer = _termui.GetWidget<Container>("searchContainer");
        var lblShortcutFilter = _termui.GetWidget<Text>("lblShortcutFilter");
        var lblShortcutSearch = _termui.GetWidget<Text>("lblShortcutSearch");

        if (searchContainer is null || _filterButton is null || lblShortcutFilter is null || lblShortcutSearch is null)
        {
            return;
        }

        int terminalWidth = Console.WindowWidth;
        const int searchContainerWidth = SearchInputWidth + 2;
        const int topBarPadding = 0;
        const int filterButtonWidth = 12;

        int filterButtonX = terminalWidth - filterButtonWidth - topBarPadding;
        int lblFilterX = filterButtonX - 6;
        int searchContainerX = lblFilterX - searchContainerWidth - 1;

        if (searchContainerX < 15)
        {
            searchContainerX = 15;
        }

        int lblSearchX = searchContainerX - 6;

        searchContainer.PositionX = $"{searchContainerX}ch";
        _filterButton.PositionX = $"{filterButtonX}ch";
        lblShortcutFilter.PositionX = $"{lblFilterX}ch";
        lblShortcutSearch.PositionX = $"{lblSearchX}ch";
    }

    public Button? GetBurgerButton() => _burgerButton;
    public Input? GetSearchInput() => _searchInput;
    public Button? GetFilterButton() => _filterButton;

    public void ToggleMouse()
    {
        _termui.MouseEnabled = !_termui.MouseEnabled;
        UpdateMouseToggleLabel();
    }

    public void UpdateFps(int fps)
    {
        if (_fpsLabel is not null)
        {
            _fpsLabel.Content = $"{fps} FPS";
        }
    }

    private void UpdateMouseToggleLabel()
    {
        if (_mouseToggleButton is null) return;

        if (_termui.MouseEnabled)
        {
            _mouseToggleButton.Text = "🖱 Mouse: ON";
            _mouseToggleButton.ForegroundColor = ConsoleColor.Green;
            _mouseToggleButton.FocusForegroundColor = ConsoleColor.Green;
        }
        else
        {
            _mouseToggleButton.Text = "🖱 Mouse: OFF";
            _mouseToggleButton.ForegroundColor = ConsoleColor.Red;
            _mouseToggleButton.FocusForegroundColor = ConsoleColor.Red;
        }
    }
}

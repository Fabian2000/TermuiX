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

    <Text
        Name='lblShortcutSearch'
        PositionX='50ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+F
    </Text>

    <Text
        Name='lblShortcut'
        PositionX='10ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='DarkGray'>
        Ctrl+B
    </Text>
</Container>";
    }

    public void Initialize()
    {
        _burgerButton = _termui.GetWidget<Button>("btnBurger");
        _searchInput = _termui.GetWidget<Input>("inputSearch");
        _filterButton = _termui.GetWidget<Button>("btnFilter");

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
        int searchContainerWidth = SearchInputWidth + 2;
        int topBarPadding = 0;
        int filterButtonWidth = 12;

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
}

using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager.Components;

public class FileExplorer
{
    private const int TopBarHeight = 3;

    private readonly TermuiXLib _termui;
    private Container? _leftColumn;
    private Container? _rightColumn;
    private Text? _lblNameValue;
    private Text? _lblTypeValue;
    private Text? _lblSizeValue;
    private Text? _lblCreatedValue;
    private Text? _lblModifiedValue;
    private Button? _btnCopy;

    public FileExplorer(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        int contentHeight = Console.WindowHeight - TopBarHeight;
        int leftColumnWidth = (int)(Console.WindowWidth * 0.7);
        int rightColumnWidth = Console.WindowWidth - leftColumnWidth;

        return $@"
<Container
    Name='leftColumn'
    Width='{leftColumnWidth}ch'
    Height='{contentHeight}ch'
    PositionX='0ch'
    PositionY='{TopBarHeight}ch'
    BackgroundColor='DarkBlue'
    ForegroundColor='White'
    Scrollable='true'>
</Container>

<Container
    Name='rightColumn'
    Width='{rightColumnWidth}ch'
    Height='{contentHeight}ch'
    PositionX='{leftColumnWidth}ch'
    PositionY='{TopBarHeight}ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    Scrollable='true'>

    <Text
        Name='propertiesTitle'
        PositionX='1ch'
        PositionY='1ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        Style='Bold'>
        Properties
    </Text>

    <Text
        Name='lblNameLabel'
        PositionX='1ch'
        PositionY='3ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Name:
    </Text>
    <Text
        Name='lblNameValue'
        PositionX='7ch'
        PositionY='3ch'
        Width='{rightColumnWidth - 7}ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
    </Text>
    <Text
        Name='lblTypeLabel'
        PositionX='1ch'
        PositionY='4ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Type:
    </Text>
    <Text
        Name='lblTypeValue'
        PositionX='7ch'
        PositionY='4ch'
        Width='{rightColumnWidth - 7}ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
    </Text>
    <Text
        Name='lblSizeLabel'
        PositionX='1ch'
        PositionY='5ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Size:
    </Text>
    <Text
        Name='lblSizeValue'
        PositionX='7ch'
        PositionY='5ch'
        Width='{rightColumnWidth - 7}ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
    </Text>
    <Text
        Name='lblCreatedLabel'
        PositionX='1ch'
        PositionY='6ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Created:
    </Text>
    <Text
        Name='lblCreatedValue'
        PositionX='10ch'
        PositionY='6ch'
        Width='{rightColumnWidth - 10}ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
    </Text>
    <Text
        Name='lblModifiedLabel'
        PositionX='1ch'
        PositionY='7ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Modified:
    </Text>
    <Text
        Name='lblModifiedValue'
        PositionX='11ch'
        PositionY='7ch'
        Width='{rightColumnWidth - 11}ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
    </Text>

    <Text
        Name='actionsTitle'
        PositionX='1ch'
        PositionY='9ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        Style='Bold'>
        Actions
    </Text>

    <Button
        Name='btnCopy'
        PositionX='0ch'
        PositionY='11ch'
        Width='100%'
        Height='3ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'
        Disabled='true'>
        Copy
    </Button>

    <Button
        Name='btnMove'
        PositionX='0ch'
        PositionY='14ch'
        Width='100%'
        Height='3ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'
        Disabled='true'>
        Move
    </Button>

    <Button
        Name='btnDelete'
        PositionX='0ch'
        PositionY='17ch'
        Width='100%'
        Height='3ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'
        Disabled='true'>
        Delete
    </Button>

    <Button
        Name='btnRename'
        PositionX='0ch'
        PositionY='20ch'
        Width='100%'
        Height='3ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'
        Disabled='true'>
        Rename
    </Button>

    <Button
        Name='btnProperties'
        PositionX='0ch'
        PositionY='23ch'
        Width='100%'
        Height='3ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'
        Disabled='true'>
        Properties
    </Button>

    <Text
        Name='lblActionsShortcut'
        PositionX='10ch'
        PositionY='9ch'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='Gray'
        Style='Bold'>
        Ctrl+P
    </Text>
</Container>
";
    }

    public void Initialize()
    {
        _leftColumn = _termui.GetWidget<Container>("leftColumn");
        _rightColumn = _termui.GetWidget<Container>("rightColumn");
        _lblNameValue = _termui.GetWidget<Text>("lblNameValue");
        _lblTypeValue = _termui.GetWidget<Text>("lblTypeValue");
        _lblSizeValue = _termui.GetWidget<Text>("lblSizeValue");
        _lblCreatedValue = _termui.GetWidget<Text>("lblCreatedValue");
        _lblModifiedValue = _termui.GetWidget<Text>("lblModifiedValue");
        _btnCopy = _termui.GetWidget<Button>("btnCopy");
    }

    public Button? GetCopyButton() => _btnCopy;

    public void UpdateLayout()
    {
        int contentHeight = Console.WindowHeight - TopBarHeight;
        int leftColumnWidth = (int)(Console.WindowWidth * 0.7);
        int rightColumnWidth = Console.WindowWidth - leftColumnWidth;

        if (_leftColumn is not null)
        {
            _leftColumn.Width = $"{leftColumnWidth}ch";
            _leftColumn.Height = $"{contentHeight}ch";
        }

        if (_rightColumn is not null)
        {
            _rightColumn.Width = $"{rightColumnWidth}ch";
            _rightColumn.Height = $"{contentHeight}ch";
            _rightColumn.PositionX = $"{leftColumnWidth}ch";
        }
    }
}

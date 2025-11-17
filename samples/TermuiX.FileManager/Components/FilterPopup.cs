using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager.Components;

public enum FilterType
{
    NameAsc,
    NameDesc,
    DateAsc,
    DateDesc,
    SizeAsc,
    SizeDesc
}

public class FilterPopup
{
    private const int PopupWidth = 14;
    private const int PopupHeight = 8;

    private readonly TermuiXLib _termui;
    private Container? _container;
    private Button? _filterButton;
    private Button? _nameAscButton;
    private Button? _nameDescButton;
    private Button? _dateAscButton;
    private Button? _dateDescButton;
    private Button? _sizeAscButton;
    private Button? _sizeDescButton;
    private FileExplorer? _fileExplorer;

    public bool IsOpen { get; private set; }

    public FilterPopup(TermuiXLib termui, Button? filterButton = null)
    {
        _termui = termui;
        _filterButton = filterButton;
        IsOpen = false;
    }

    public void SetFileExplorer(FileExplorer fileExplorer)
    {
        _fileExplorer = fileExplorer;
    }

    public string BuildXml()
    {
        return $@"
<Container
    Name='filterPopup'
    Width='{PopupWidth}ch'
    Height='{PopupHeight}ch'
    PositionX='0ch'
    PositionY='0ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    BorderStyle='Single'
    RoundedCorners='true'
    Visible='false'>

    <Button
        Name='btnNameAsc'
        PositionX='0ch'
        PositionY='0ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        A-Z Name
    </Button>

    <Button
        Name='btnNameDesc'
        PositionX='0ch'
        PositionY='1ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        Z-A Name
    </Button>

    <Button
        Name='btnDateAsc'
        PositionX='0ch'
        PositionY='2ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        1-9 Date
    </Button>

    <Button
        Name='btnDateDesc'
        PositionX='0ch'
        PositionY='3ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        9-1 Date
    </Button>

    <Button
        Name='btnSizeAsc'
        PositionX='0ch'
        PositionY='4ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        1-9 Size
    </Button>

    <Button
        Name='btnSizeDesc'
        PositionX='0ch'
        PositionY='5ch'
        Width='12ch'
        Height='1ch'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'
        TextAlign='Left'
        BorderStyle='None'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'>
        9-1 Size
    </Button>
</Container>";
    }

    public void Initialize()
    {
        _container = _termui.GetWidget<Container>("filterPopup");
        _nameAscButton = _termui.GetWidget<Button>("btnNameAsc");
        _nameDescButton = _termui.GetWidget<Button>("btnNameDesc");
        _dateAscButton = _termui.GetWidget<Button>("btnDateAsc");
        _dateDescButton = _termui.GetWidget<Button>("btnDateDesc");
        _sizeAscButton = _termui.GetWidget<Button>("btnSizeAsc");
        _sizeDescButton = _termui.GetWidget<Button>("btnSizeDesc");

        // Attach click handlers
        if (_nameAscButton is not null)
        {
            _nameAscButton.Click += (sender, e) => OnFilterSelected(FilterType.NameAsc);
        }
        if (_nameDescButton is not null)
        {
            _nameDescButton.Click += (sender, e) => OnFilterSelected(FilterType.NameDesc);
        }
        if (_dateAscButton is not null)
        {
            _dateAscButton.Click += (sender, e) => OnFilterSelected(FilterType.DateAsc);
        }
        if (_dateDescButton is not null)
        {
            _dateDescButton.Click += (sender, e) => OnFilterSelected(FilterType.DateDesc);
        }
        if (_sizeAscButton is not null)
        {
            _sizeAscButton.Click += (sender, e) => OnFilterSelected(FilterType.SizeAsc);
        }
        if (_sizeDescButton is not null)
        {
            _sizeDescButton.Click += (sender, e) => OnFilterSelected(FilterType.SizeDesc);
        }

        UpdateLayout();
    }

    private void OnFilterSelected(FilterType filterType)
    {
        // Apply filter to file explorer
        _fileExplorer?.ApplySort(filterType);

        // Close popup and return focus to filter button
        Close();
    }

    public void Open()
    {
        if (IsOpen || _container is null)
        {
            return;
        }

        IsOpen = true;
        UpdateFocusability();

        if (_nameAscButton is not null)
        {
            _termui.SetFocus(_nameAscButton);
        }
    }

    public void Close()
    {
        if (!IsOpen || _container is null)
        {
            return;
        }

        IsOpen = false;

        if (_filterButton is not null)
        {
            _termui.SetFocus(_filterButton);
        }

        UpdateFocusability();
    }

    public void UpdateLayout()
    {
        if (_container is null)
        {
            return;
        }

        int terminalWidth = Console.WindowWidth;
        int popupX = terminalWidth - PopupWidth;
        int popupY = 3;

        _container.PositionX = $"{popupX}ch";
        _container.PositionY = $"{popupY}ch";
    }

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

    private void UpdateFocusability()
    {
        if (_container is null)
        {
            return;
        }

        _container.Visible = IsOpen;
    }
}

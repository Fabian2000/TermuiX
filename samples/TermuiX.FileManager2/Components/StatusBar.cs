using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class StatusBar
{
    private readonly TermuiXLib _termui;
    private Text? _itemsText;
    private Text? _selectedText;
    private Text? _sizeText;
    private Button? _previewToggle;
    private bool _previewEnabled;

    // Sort
    private Button? _sortButton;
    private Container? _sortDropdown;
    private readonly List<Button> _sortButtons = [];

    public event EventHandler<bool>? PreviewToggled;
    public event EventHandler<SortMode>? SortSelected;
    public bool IsPreviewEnabled => _previewEnabled;
    public bool IsSortDropdownOpen => _sortDropdown?.Visible is true;

    public StatusBar(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='statusBar' Direction='Horizontal' Width='100%' Height='1ch'
    BackgroundColor='#141414' PaddingLeft='1ch' PaddingRight='1ch' Align='Center'>
    <Text Name='sbItems' BackgroundColor='#141414' ForegroundColor='#909090'>0 items</Text>
    <Text BackgroundColor='#141414' ForegroundColor='#333333' MarginLeft='1ch' MarginRight='1ch'>│</Text>
    <Text Name='sbSelected' BackgroundColor='#141414' ForegroundColor='#909090'>0 selected</Text>
    <Text BackgroundColor='#141414' ForegroundColor='#333333' MarginLeft='1ch' MarginRight='1ch'>│</Text>
    <Text Name='sbSize' Width='fill' BackgroundColor='#141414' ForegroundColor='#909090'>0 B</Text>
    <StackPanel Width='fill' Direction='Horizontal' Justify='End'>
        <Button Name='btnSort' Height='1ch' BorderStyle='None'
            PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='0ch' PaddingBottom='0ch'
            BackgroundColor='#141414' FocusBackgroundColor='#2a2a2a'
            TextColor='#909090' FocusTextColor='#d0d0d0'>⧉ Sort</Button>
        <Text BackgroundColor='#141414' ForegroundColor='#333333'>│</Text>
        <Button Name='sbPreview' Height='1ch' BorderStyle='None'
            PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
            BackgroundColor='#141414' FocusBackgroundColor='#2a2a2a'
            TextColor='#808080' FocusTextColor='#d0d0d0'>👁 Preview</Button>
    </StackPanel>
</StackPanel>";
    }

    public string BuildOverlayXml()
    {
        return @"
<Container Name='sortDropdown' Width='16ch' Height='8ch' PositionX='0ch' PositionY='0ch'
    BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0' BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='sortNameAsc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='0ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▲ Name</Button>
    <Button Name='sortNameDesc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▼ Name</Button>
    <Button Name='sortDateAsc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='2ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▲ Date</Button>
    <Button Name='sortDateDesc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='3ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▼ Date</Button>
    <Button Name='sortSizeAsc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='4ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▲ Size</Button>
    <Button Name='sortSizeDesc' Width='14ch' Height='1ch' PositionX='0ch' PositionY='5ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>▼ Size</Button>
</Container>";
    }

    public void Initialize()
    {
        _itemsText = _termui.GetWidget<Text>("sbItems");
        _selectedText = _termui.GetWidget<Text>("sbSelected");
        _sizeText = _termui.GetWidget<Text>("sbSize");
        _previewToggle = _termui.GetWidget<Button>("sbPreview");
        _sortButton = _termui.GetWidget<Button>("btnSort");
        _sortDropdown = _termui.GetWidget<Container>("sortDropdown");

        if (_previewToggle is not null)
        {
            _previewToggle.Click += (_, _) =>
            {
                _previewEnabled = !_previewEnabled;
                UpdateToggleAppearance();
                PreviewToggled?.Invoke(this, _previewEnabled);
            };
        }

        if (_sortButton is not null)
            _sortButton.Click += (_, _) => ToggleSortDropdown();

        // Sort dropdown buttons
        var sortNames = new[] { "sortNameAsc", "sortNameDesc", "sortDateAsc", "sortDateDesc", "sortSizeAsc", "sortSizeDesc" };
        var sortModes = new[] { SortMode.NameAsc, SortMode.NameDesc, SortMode.DateAsc, SortMode.DateDesc, SortMode.SizeAsc, SortMode.SizeDesc };

        for (int i = 0; i < sortNames.Length; i++)
        {
            var btn = _termui.GetWidget<Button>(sortNames[i]);
            if (btn is not null)
            {
                _sortButtons.Add(btn);
                var mode = sortModes[i];
                btn.Click += (_, _) => { SortSelected?.Invoke(this, mode); CloseSortDropdown(); };
            }
        }
    }

    private void ToggleSortDropdown()
    {
        if (_sortDropdown is null) return;

        if (_sortDropdown.Visible)
        {
            CloseSortDropdown();
        }
        else
        {
            // Position above the sort button using actual rendered position
            int sortW = _sortButton is not null ? ((IWidget)_sortButton).ComputedWidth : 8;
            int previewW = _previewToggle is not null ? ((IWidget)_previewToggle).ComputedWidth : 11;
            int sepW = 1; // "│" separator
            int padR = 1; // StatusBar PaddingRight
            int x = Console.WindowWidth - padR - previewW - sepW - sortW;
            int y = Console.WindowHeight - 9; // 8ch dropdown + 1ch statusbar
            _sortDropdown.PositionX = $"{x}ch";
            _sortDropdown.PositionY = $"{y}ch";
            _sortDropdown.Visible = true;
            if (_sortButtons.Count > 0) _termui.SetFocus(_sortButtons[0]);
        }
    }

    public void CloseSortDropdown()
    {
        if (_sortDropdown is not null) _sortDropdown.Visible = false;
    }

    private void UpdateToggleAppearance()
    {
        if (_previewToggle is null) return;
        if (_previewEnabled)
        {
            _previewToggle.TextColor = global::TermuiX.Color.Parse("#5588cc");
            _previewToggle.FocusTextColor = global::TermuiX.Color.Parse("#5588cc");
            _previewToggle.Text = "👁 Preview ✓";
        }
        else
        {
            _previewToggle.TextColor = global::TermuiX.Color.Parse("#808080");
            _previewToggle.FocusTextColor = global::TermuiX.Color.Parse("#d0d0d0");
            _previewToggle.Text = "👁 Preview";
        }
    }

    public void Update(FileListStats stats)
    {
        if (_itemsText is not null)
            _itemsText.Content = $"{stats.TotalItems} items";
        if (_selectedText is not null)
            _selectedText.Content = $"{stats.SelectedItems} selected";
        if (_sizeText is not null)
            _sizeText.Content = FormatSize(stats.TotalSize);
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return $"{size:0.#} {units[i]}";
    }
}

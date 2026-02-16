using System.Security;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class Toolbar
{
    private readonly TermuiXLib _termui;

    // Tabs
    private StackPanel? _tabBar;
    private Line? _tabLine;
    private Button? _addTabButton;
    private Text? _tabSpacer;
    private readonly List<TabInfo> _tabs = [];
    private int _activeTabIndex = 0;
    private int _tabCounter;

    // Nav
    private Button? _btnBack;
    private Button? _btnForward;
    private Button? _btnUp;
    private Button? _breadcrumb;
    private Container? _cmdContainer;
    private Input? _cmdInput;
    private Input? _searchInput;
    private string _currentPath = "/";

    private const int MaxTabTextLength = 19;

    public event EventHandler<int>? TabSelected;
    public event EventHandler<int>? TabCloseRequested;
    public event EventHandler? NewTabRequested;
    public event EventHandler? BackRequested;
    public event EventHandler? ForwardRequested;
    public event EventHandler? UpRequested;
    public event EventHandler<string>? SearchChanged;
    public event EventHandler<string>? CommandSubmitted;

    public int ActiveTabIndex => _activeTabIndex;
    public int TabCount => _tabs.Count;

    public Toolbar(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='toolbar' Direction='Vertical' Width='100%' Height='auto' BackgroundColor='#141414'>
    <StackPanel Name='tabBar' Direction='Horizontal' Width='100%' Height='auto'
        BackgroundColor='#141414' PaddingLeft='1ch' Align='Center' ScrollX='true' />
    <Line Name='tabLine' Orientation='Horizontal' Type='Dotted' Width='100%'
        BackgroundColor='#141414' ForegroundColor='#ffffff' MarginTop='-1ch' />
    <StackPanel Name='navBar' Direction='Horizontal' Width='100%'
        BackgroundColor='#141414' PaddingLeft='1ch' PaddingRight='1ch' Align='Center'>
        <Button Name='btnBack' Width='5ch'
            PaddingLeft='1ch' PaddingRight='1ch'
            BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
            TextColor='#808080' FocusTextColor='#ffffff'
            BorderColor='#808080' FocusBorderColor='#ffffff'
            BorderStyle='Single' RoundedCorners='true'>←</Button>
        <Button Name='btnForward' Width='5ch' MarginLeft='1ch'
            PaddingLeft='1ch' PaddingRight='1ch'
            BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
            TextColor='#808080' FocusTextColor='#ffffff'
            BorderColor='#808080' FocusBorderColor='#ffffff'
            BorderStyle='Single' RoundedCorners='true'>→</Button>
        <Button Name='btnUp' Width='5ch' MarginLeft='1ch'
            PaddingLeft='1ch' PaddingRight='1ch'
            BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
            TextColor='#808080' FocusTextColor='#ffffff'
            BorderColor='#808080' FocusBorderColor='#ffffff'
            BorderStyle='Single' RoundedCorners='true'>↑</Button>
        <StackPanel Name='navContent' Width='fill' Direction='Horizontal' MarginLeft='1ch' Align='Center'>
            <Button Name='breadcrumb' Width='fill'
                PaddingLeft='1ch' PaddingRight='1ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                TextColor='#d0d0d0' FocusTextColor='#d0d0d0'
                BorderColor='#808080' FocusBorderColor='#808080'
                BorderStyle='Single' RoundedCorners='true'
                TextAlign='Left'>📂 /</Button>
            <Container Name='cmdContainer' Width='fill' Height='3ch'
                BackgroundColor='#141414'
                BorderColor='#5588cc' FocusBorderColor='#5588cc'
                BorderStyle='Single' RoundedCorners='true' Visible='false'>
                <Input Name='cmdInput' Width='100%' Height='1ch'
                    Placeholder='Enter path or command...'
                    BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                    ForegroundColor='#d0d0d0' CursorColor='#d0d0d0' />
            </Container>
            <Container Name='searchContainer' Width='30ch' Height='3ch' MarginLeft='1ch'
                BackgroundColor='#141414'
                BorderColor='#808080' FocusBorderColor='#ffffff'
                BorderStyle='Single' RoundedCorners='true'>
                <Input Name='searchInput' Width='100%' Height='1ch'
                    Placeholder='🔍 Search...'
                    BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                    ForegroundColor='#d0d0d0' />
            </Container>
        </StackPanel>
    </StackPanel>
    <Line Name='toolbarSep' Orientation='Horizontal' Type='Solid' Width='100%'
        BackgroundColor='#141414' ForegroundColor='#333333' />
</StackPanel>";
    }

    public string BuildOverlayXml()
    {
        return "";
    }

    public void Initialize()
    {
        _tabBar = _termui.GetWidget<StackPanel>("tabBar");
        _tabLine = _termui.GetWidget<Line>("tabLine");

        // Nav buttons
        _btnBack = _termui.GetWidget<Button>("btnBack");
        _btnForward = _termui.GetWidget<Button>("btnForward");
        _btnUp = _termui.GetWidget<Button>("btnUp");
        if (_btnBack is not null) _btnBack.Click += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);
        if (_btnForward is not null) _btnForward.Click += (_, _) => ForwardRequested?.Invoke(this, EventArgs.Empty);
        if (_btnUp is not null) _btnUp.Click += (_, _) => UpRequested?.Invoke(this, EventArgs.Empty);

        // Breadcrumb + Command Input
        _breadcrumb = _termui.GetWidget<Button>("breadcrumb");
        _cmdContainer = _termui.GetWidget<Container>("cmdContainer");
        _cmdInput = _termui.GetWidget<Input>("cmdInput");

        if (_breadcrumb is not null)
            _breadcrumb.Click += (_, _) => ActivateCommandInput();

        if (_cmdInput is not null)
        {
            _cmdInput.EnterPressed += (_, text) =>
            {
                DeactivateCommandInput();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    CommandSubmitted?.Invoke(this, text.Trim());
                }
            };
            _cmdInput.EscapePressed += (_, _) => DeactivateCommandInput();
        }

        // Search
        _searchInput = _termui.GetWidget<Input>("searchInput");
        if (_searchInput is not null)
            _searchInput.TextChanged += (_, text) => SearchChanged?.Invoke(this, text);

        // Add the "+" button (always last in tabBar)
        _tabBar?.Add(@"
            <Button Name='tabAdd' Width='3ch'
                MarginLeft='2ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                TextColor='#808080' FocusTextColor='#ffffff'
                BorderColor='#808080' FocusBorderColor='#ffffff'
                BorderStyle='Single' RoundedCorners='true'>+</Button>
            <Text Name='tabSpacer' BackgroundColor='#141414' ForegroundColor='#141414'>_____</Text>");
        _addTabButton = _termui.GetWidget<Button>("tabAdd");
        _tabSpacer = _termui.GetWidget<Text>("tabSpacer");
        if (_addTabButton is not null)
            _addTabButton.Click += (_, _) => NewTabRequested?.Invoke(this, EventArgs.Empty);
    }

    // --- Tabs ---

    private static string TruncateTabName(string name)
    {
        if (name.Length <= MaxTabTextLength) return name;
        return name[..(MaxTabTextLength - 1)] + "\u2026"; // ellipsis
    }

    public void AddTab(string directory)
    {
        if (_tabBar is null) return;

        // Move "+" button and spacer out of the way
        if (_tabSpacer is not null)
            _tabBar.Remove(_tabSpacer);
        if (_addTabButton is not null)
            _tabBar.Remove(_addTabButton);

        var tabIndex = _tabs.Count;
        var id = _tabCounter++;
        var tabName = $"tab_{id}";
        var closeName = $"tabClose_{id}";
        var dirName = Path.GetFileName(directory);
        if (string.IsNullOrEmpty(dirName)) dirName = "/";

        var displayName = TruncateTabName(dirName);
        var escaped = SecurityElement.Escape(displayName);

        // First tab has no extra MarginLeft, subsequent tabs need 2ch to compensate
        // for the previous close button's negative margin
        var marginLeft = tabIndex > 0 ? "MarginLeft='2ch'" : "";

        var xml = $@"
            <Button Name='{tabName}' Width='25ch'
                {marginLeft}
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                TextColor='#d0d0d0' FocusTextColor='#ffffff'
                BorderColor='#808080' FocusBorderColor='#ffffff'
                BorderStyle='Single' RoundedCorners='true'
                TextAlign='Left' PaddingLeft='1ch' PaddingRight='2ch'>{escaped}</Button>
            <Button Name='{closeName}' Width='1ch' Height='1ch' BorderStyle='None'
                MarginLeft='-3ch'
                PaddingLeft='0ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                TextColor='#808080' FocusTextColor='#cc5555'>✕</Button>";

        _tabBar.Add(xml);

        var tabBtn = _termui.GetWidget<Button>(tabName);
        var closeBtn = _termui.GetWidget<Button>(closeName);

        var info = new TabInfo
        {
            Index = tabIndex,
            Directory = directory,
            TabButton = tabBtn,
            CloseButton = closeBtn,
            Id = id
        };
        _tabs.Add(info);

        if (tabBtn is not null)
        {
            tabBtn.Click += (_, _) => OnTabClicked(info.Index);
        }
        if (closeBtn is not null)
        {
            closeBtn.Click += (_, _) => TabCloseRequested?.Invoke(this, info.Index);
        }

        SetActiveTab(tabIndex);

        // Re-add "+" button and spacer at the end
        if (_addTabButton is not null)
            _tabBar.Add(_addTabButton);
        if (_tabSpacer is not null)
            _tabBar.Add(_tabSpacer);
    }

    public void RemoveTab(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        if (_tabBar is null) return;

        var tab = _tabs[index];

        // Remove both buttons from the tabBar
        if (tab.TabButton is not null)
            _tabBar.Remove(tab.TabButton);
        if (tab.CloseButton is not null)
            _tabBar.Remove(tab.CloseButton);

        _tabs.RemoveAt(index);

        // Re-index remaining tabs
        for (int i = 0; i < _tabs.Count; i++)
            _tabs[i].Index = i;

        // Fix MarginLeft on the new first tab (remove the 2ch compensation)
        if (_tabs.Count > 0 && _tabs[0].TabButton is { } firstTab)
            firstTab.MarginLeft = "0ch";

        if (_activeTabIndex >= _tabs.Count)
            _activeTabIndex = _tabs.Count - 1;

        SetActiveTab(_activeTabIndex);
    }

    public void SetActiveTab(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        _activeTabIndex = index;
        UpdateTabColors();
        TabSelected?.Invoke(this, index);
    }

    public void UpdateTabDirectory(int index, string directory)
    {
        if (index < 0 || index >= _tabs.Count) return;

        var tab = _tabs[index];
        tab.Directory = directory;
        var dirName = Path.GetFileName(directory);
        if (string.IsNullOrEmpty(dirName)) dirName = "/";

        if (tab.TabButton is not null)
            tab.TabButton.Text = TruncateTabName(dirName);
    }

    public string? GetTabDirectory(int index)
    {
        if (index < 0 || index >= _tabs.Count) return null;
        return _tabs[index].Directory;
    }

    private void OnTabClicked(int index)
    {
        if (index == _activeTabIndex) return;
        SetActiveTab(index);
    }

    private void UpdateTabColors()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            var tab = _tabs[i];
            bool active = i == _activeTabIndex;
            var color = global::TermuiX.Color.Parse(active ? "#ffffff" : "#808080");

            if (tab.TabButton is not null)
            {
                tab.TabButton.TextColor = color;
                tab.TabButton.BorderColor = color;
                tab.TabButton.FocusBorderColor = global::TermuiX.Color.Parse("#ffffff");
            }
            if (tab.CloseButton is not null)
            {
                tab.CloseButton.TextColor = global::TermuiX.Color.Parse(active ? "#ffffff" : "#808080");
            }
        }
    }

    public void UpdateTabLine()
    {
        if (_tabLine is null || _tabBar is null) return;
        _tabLine.Visible = !((IWidget)_tabBar).HasHorizontalScrollbar;
    }

    // --- Command Input ---

    public bool IsCommandInputActive => _cmdContainer?.Visible is true;

    private void ActivateCommandInput()
    {
        if (_breadcrumb is null || _cmdContainer is null || _cmdInput is null) return;

        _breadcrumb.Visible = false;
        _cmdContainer.Visible = true;
        _cmdInput.Value = _currentPath;
        _termui.SetFocus(_cmdInput);
    }

    private void DeactivateCommandInput()
    {
        if (_breadcrumb is null || _cmdContainer is null) return;

        _cmdContainer.Visible = false;
        _breadcrumb.Visible = true;
    }

    public bool IsCommandInputWidget(IWidget widget) => widget == _cmdInput;

    // --- Existing ---

    public void UpdatePath(string path)
    {
        if (_breadcrumb is null) return;

        _currentPath = path;

        // Available width: ComputedWidth minus border(2) minus padding(2)
        int availWidth = ((IWidget)_breadcrumb).ComputedWidth - 4;
        string prefix = "📂 ";
        int prefixLen = 3; // 📂(2) + space(1)
        int maxPathLen = availWidth - prefixLen;

        if (maxPathLen > 0 && path.Length > maxPathLen)
            path = "…" + path[^(maxPathLen - 1)..];

        _breadcrumb.Text = prefix + path;
    }

    public void UpdateHistoryButtons(bool canGoBack, bool canGoForward, bool canGoUp)
    {
        if (_btnBack is not null)
        {
            var color = global::TermuiX.Color.Parse(canGoBack ? "#d0d0d0" : "#404040");
            _btnBack.TextColor = color;
            _btnBack.BorderColor = color;
        }
        if (_btnForward is not null)
        {
            var color = global::TermuiX.Color.Parse(canGoForward ? "#d0d0d0" : "#404040");
            _btnForward.TextColor = color;
            _btnForward.BorderColor = color;
        }
        if (_btnUp is not null)
        {
            var color = global::TermuiX.Color.Parse(canGoUp ? "#d0d0d0" : "#404040");
            _btnUp.TextColor = color;
            _btnUp.BorderColor = color;
        }
    }

    public void FocusSearch()
    {
        if (_searchInput is not null)
            _termui.SetFocus(_searchInput);
    }

    public void ClearSearch()
    {
        if (_searchInput is not null)
            _searchInput.Value = "";
    }

    private class TabInfo
    {
        public int Index;
        public int Id;
        public string Directory = "";
        public Button? TabButton;
        public Button? CloseButton;
    }
}

public enum SortMode
{
    NameAsc, NameDesc, DateAsc, DateDesc, SizeAsc, SizeDesc
}

using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager.Components;

public class FileExplorer
{
    private const int TopBarHeight = 3;

    private readonly TermuiXLib _termui;
    private Container? _leftColumn;
    private Container? _rightColumn;
    private Text? _navigationHints;
    private Text? _lblNameValue;
    private Text? _lblTypeValue;
    private Text? _lblSizeValue;
    private Text? _lblCreatedValue;
    private Text? _lblModifiedValue;
    private Button? _btnCopy;
    private Button? _btnMove;
    private Button? _btnDelete;
    private Button? _btnRename;
    private Input? _inputRename;
    private string? _itemBeingRenamed;
    private Container? _deleteConfirmPopup;
    private Container? _deleteOverlay;
    private Button? _btnDeleteYes;
    private Button? _btnDeleteNo;
    private Text? _deleteConfirmText;

    // Context menu
    private Container? _contextMenu;
    private Button? _ctxOpen;
    private Button? _ctxCopy;
    private Button? _ctxMove;
    private Button? _ctxRename;
    private Button? _ctxDelete;
    private string? _contextMenuTargetPath;

    // Copy/Move operation state
    private enum ClipboardOperation { None, Copy, Move }
    private ClipboardOperation _clipboardOperation = ClipboardOperation.None;
    private string? _clipboardItemPath;

    private string _currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly HashSet<string> _selectedItems = [];
    private readonly Dictionary<Button, string> _buttonPathMap = [];
    private string? _filterText;
    private FilterType _currentSort = FilterType.NameAsc;
    private string? _lastFocusedFilePath;
    private string? _previousFocusedFilePath;

    // History tracking for navigation
    private readonly List<(string directory, string? lastClickedItem)> _historyBack = [];
    private readonly List<(string directory, string? lastClickedItem)> _historyForward = [];
    private string? _lastClickedItemInCurrentDir;

    public FileExplorer(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        int contentHeight = Console.WindowHeight - TopBarHeight - 1;
        int leftColumnWidth = (int)(Console.WindowWidth * 0.7);
        int rightColumnWidth = Console.WindowWidth - leftColumnWidth;

        return $@"
<Text
    Name='navigationHints'
    PositionX='0ch'
    PositionY='{TopBarHeight}ch'
    Height='1ch'
    BackgroundColor='Black'
    ForegroundColor='DarkGray'>
    ↓ Ctrl+R ← Ctrl+U Ctrl+Y →
</Text>

<Container
    Name='leftColumn'
    Width='{leftColumnWidth}ch'
    Height='{contentHeight}ch'
    PositionX='0ch'
    PositionY='{TopBarHeight + 1}ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    Scrollable='true'>
</Container>

<Container
    Name='rightColumn'
    Width='{rightColumnWidth}ch'
    Height='{contentHeight}ch'
    PositionX='{leftColumnWidth}ch'
    PositionY='{TopBarHeight + 1}ch'
    BackgroundColor='Black'
    ForegroundColor='White'>

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

<Input
    Name='inputRename'
    Width='40ch'
    Height='1ch'
    PositionX='10ch'
    PositionY='10ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    FocusBackgroundColor='DarkGray'
    FocusForegroundColor='White'
    BorderStyle='Single'
    MultiLine='false'
    Visible='false'>
</Input>

<Container
    Name='deleteOverlay'
    Width='100%'
    Height='100%'
    PositionX='0ch'
    PositionY='0ch'
    BackgroundColor='Black'
    Visible='false'>
</Container>

<Container
    Name='deleteConfirmPopup'
    Width='50ch'
    Height='10ch'
    PositionX='0ch'
    PositionY='0ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    BorderStyle='Single'
    RoundedCorners='true'
    Visible='false'>

    <Text
        Name='deleteConfirmTitle'
        PositionX='1ch'
        PositionY='1ch'
        BackgroundColor='Black'
        ForegroundColor='Red'
        Style='Bold'>
        Delete Confirmation
    </Text>

    <Text
        Name='deleteConfirmText'
        PositionX='1ch'
        PositionY='3ch'
        Width='46ch'
        BackgroundColor='Black'
        ForegroundColor='White'>
        Are you sure you want to delete this item?
    </Text>

    <Button
        Name='btnDeleteNo'
        PositionX='9ch'
        PositionY='5ch'
        Width='12ch'
        Height='3ch'
        BackgroundColor='DarkGray'
        ForegroundColor='White'
        FocusBackgroundColor='Gray'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'>
        No
    </Button>

    <Button
        Name='btnDeleteYes'
        PositionX='27ch'
        PositionY='5ch'
        Width='12ch'
        Height='3ch'
        BackgroundColor='Red'
        ForegroundColor='White'
        FocusBackgroundColor='DarkRed'
        FocusForegroundColor='White'
        BorderStyle='Single'
        RoundedCorners='true'
        PaddingLeft='1ch'
        PaddingRight='1ch'
        PaddingTop='1ch'
        PaddingBottom='1ch'>
        Yes
    </Button>
</Container>

<Container
    Name='contextMenu'
    Width='18ch'
    Height='7ch'
    PositionX='0ch'
    PositionY='0ch'
    BackgroundColor='Black'
    ForegroundColor='White'
    BorderStyle='Single'
    RoundedCorners='true'
    Visible='false'>

    <Button
        Name='ctxOpen'
        PositionX='0ch'
        PositionY='0ch'
        Width='100%'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'
        TextAlign='Left'
        BorderStyle='None'
        PaddingLeft='0ch'
        PaddingRight='0ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'>
        Open
    </Button>

    <Button
        Name='ctxCopy'
        PositionX='0ch'
        PositionY='1ch'
        Width='100%'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'
        TextAlign='Left'
        BorderStyle='None'
        PaddingLeft='0ch'
        PaddingRight='0ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'>
        Copy
    </Button>

    <Button
        Name='ctxMove'
        PositionX='0ch'
        PositionY='2ch'
        Width='100%'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'
        TextAlign='Left'
        BorderStyle='None'
        PaddingLeft='0ch'
        PaddingRight='0ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'>
        Move
    </Button>

    <Button
        Name='ctxRename'
        PositionX='0ch'
        PositionY='3ch'
        Width='100%'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='White'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='White'
        TextAlign='Left'
        BorderStyle='None'
        PaddingLeft='0ch'
        PaddingRight='0ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'>
        Rename
    </Button>

    <Button
        Name='ctxDelete'
        PositionX='0ch'
        PositionY='4ch'
        Width='100%'
        Height='1ch'
        BackgroundColor='Black'
        ForegroundColor='Red'
        FocusBackgroundColor='DarkGray'
        FocusForegroundColor='Red'
        TextAlign='Left'
        BorderStyle='None'
        PaddingLeft='0ch'
        PaddingRight='0ch'
        PaddingTop='0ch'
        PaddingBottom='0ch'>
        Delete
    </Button>
</Container>
";
    }

    public void Initialize()
    {
        _leftColumn = _termui.GetWidget<Container>("leftColumn");
        _rightColumn = _termui.GetWidget<Container>("rightColumn");
        _navigationHints = _termui.GetWidget<Text>("navigationHints");
        _lblNameValue = _termui.GetWidget<Text>("lblNameValue");
        _lblTypeValue = _termui.GetWidget<Text>("lblTypeValue");
        _lblSizeValue = _termui.GetWidget<Text>("lblSizeValue");
        _lblCreatedValue = _termui.GetWidget<Text>("lblCreatedValue");
        _lblModifiedValue = _termui.GetWidget<Text>("lblModifiedValue");
        _btnCopy = _termui.GetWidget<Button>("btnCopy");
        _btnMove = _termui.GetWidget<Button>("btnMove");
        _btnDelete = _termui.GetWidget<Button>("btnDelete");
        _btnRename = _termui.GetWidget<Button>("btnRename");
        _inputRename = _termui.GetWidget<Input>("inputRename");
        _deleteConfirmPopup = _termui.GetWidget<Container>("deleteConfirmPopup");
        _deleteOverlay = _termui.GetWidget<Container>("deleteOverlay");
        _btnDeleteYes = _termui.GetWidget<Button>("btnDeleteYes");
        _btnDeleteNo = _termui.GetWidget<Button>("btnDeleteNo");
        _deleteConfirmText = _termui.GetWidget<Text>("deleteConfirmText");

        // Attach copy button click handler
        if (_btnCopy is not null)
        {
            _btnCopy.Click += OnCopyClick;
        }

        // Attach move button click handler
        if (_btnMove is not null)
        {
            _btnMove.Click += OnMoveClick;
        }

        // Attach rename button click handler
        if (_btnRename is not null)
        {
            _btnRename.Click += OnRenameClick;
        }

        // Attach input enter handler
        if (_inputRename is not null)
        {
            _inputRename.EnterPressed += OnRenameInputEnter;
        }

        // Attach delete button click handler
        if (_btnDelete is not null)
        {
            _btnDelete.Click += OnDeleteClick;
        }

        // Attach delete confirmation button handlers
        if (_btnDeleteYes is not null)
        {
            _btnDeleteYes.Click += OnDeleteConfirmYes;
        }

        if (_btnDeleteNo is not null)
        {
            _btnDeleteNo.Click += OnDeleteConfirmNo;
        }

        // Context menu
        _contextMenu = _termui.GetWidget<Container>("contextMenu");
        _ctxOpen = _termui.GetWidget<Button>("ctxOpen");
        _ctxCopy = _termui.GetWidget<Button>("ctxCopy");
        _ctxMove = _termui.GetWidget<Button>("ctxMove");
        _ctxRename = _termui.GetWidget<Button>("ctxRename");
        _ctxDelete = _termui.GetWidget<Button>("ctxDelete");

        if (_ctxOpen is not null)
            _ctxOpen.Click += (_, _) => { OnContextOpen(); CloseContextMenu(); };
        if (_ctxCopy is not null)
            _ctxCopy.Click += (_, _) => { OnContextCopy(); CloseContextMenu(); };
        if (_ctxMove is not null)
            _ctxMove.Click += (_, _) => { OnContextMove(); CloseContextMenu(); };
        if (_ctxRename is not null)
            _ctxRename.Click += (_, _) => { OnContextRename(); CloseContextMenu(); };
        if (_ctxDelete is not null)
            _ctxDelete.Click += (_, _) => { OnContextDelete(); CloseContextMenu(); };

        // Subscribe to focus changes to update navigation hints
        _termui.FocusChanged += OnFocusChanged;

        // Close context menu on any mouse click (handles clicking into empty areas)
        _termui.MouseClick += (_, e) =>
        {
            if (IsContextMenuOpen &&
                (e.EventType == MouseEventType.LeftButtonPressed || e.EventType == MouseEventType.RightButtonPressed))
            {
                // Only close if click is outside the context menu area
                if (_contextMenu is not null)
                {
                    int menuX = int.Parse(_contextMenu.PositionX.Replace("ch", ""));
                    int menuY = int.Parse(_contextMenu.PositionY.Replace("ch", ""));
                    int menuW = 18;
                    int menuH = 7;

                    if (e.X < menuX || e.X >= menuX + menuW || e.Y < menuY || e.Y >= menuY + menuH)
                    {
                        CloseContextMenu();
                    }
                }
            }
        };

        RefreshFileList();
    }

    private void OnFocusChanged(object? sender, FocusChangedEventArgs e)
    {
        // Close context menu when focus moves outside it
        if (IsContextMenuOpen && !ContainsContextMenuWidget(e.Widget))
        {
            CloseContextMenu();
        }

        // If delete popup is visible, restrict focus to Yes/No buttons only
        if (_deleteConfirmPopup is not null && _deleteConfirmPopup.Visible)
        {
            if (e.Widget != _btnDeleteYes && e.Widget != _btnDeleteNo)
            {
                // Force focus back to No button (safer option)
                if (_btnDeleteNo is not null)
                {
                    _termui.SetFocus(_btnDeleteNo);
                }
            }
            return;
        }

        UpdateNavigationHints(e.Widget);

        // Update last focused file path when focus changes to a file/folder button
        if (e.Widget is Button button && _buttonPathMap.TryGetValue(button, out string? path))
        {
            _previousFocusedFilePath = _lastFocusedFilePath;
            _lastFocusedFilePath = path;
            UpdatePropertiesPanel(path);
        }

        // Close rename overlay if focus moves away from the input
        if (_inputRename is not null && _inputRename.Visible && e.Widget != _inputRename)
        {
            CloseRenameOverlay();
        }
    }

    private void UpdateNavigationHints(IWidget? focusedWidget)
    {
        if (_navigationHints is null)
        {
            return;
        }

        // Check if focused widget is inside the explorer
        bool isFocusInExplorer = IsFocusInExplorer(focusedWidget);

        if (isFocusInExplorer)
        {
            _navigationHints.Content = "↻ Ctrl+R ← Ctrl+U Ctrl+Y →";
        }
        else
        {
            _navigationHints.Content = "↓ Ctrl+R ← Ctrl+U Ctrl+Y →";
        }
    }

    private bool IsFocusInExplorer(IWidget? widget)
    {
        if (widget is null || _leftColumn is null)
        {
            return false;
        }

        // Check if widget is the left column itself or a child of it
        IWidget? current = widget;
        while (current is not null)
        {
            if (current == _leftColumn)
            {
                return true;
            }
            current = current.Parent;
        }

        return false;
    }

    public Button? GetCopyButton() => _btnCopy;

    /// <summary>
    /// Selects the currently focused item without navigating into it.
    /// This allows selecting folders for copy/move operations.
    /// </summary>
    public void SelectCurrentItem()
    {
        if (_lastFocusedFilePath is null)
        {
            return;
        }

        // Find the button for the currently focused item
        var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == _lastFocusedFilePath).Key;
        if (button is null)
        {
            return;
        }

        // Single selection mode: clear previous selections and add current item
        _selectedItems.Clear();
        _selectedItems.Add(_lastFocusedFilePath);

        // Update button colors
        UpdateSelectionColors();

        // Enable/disable action buttons
        UpdateActionButtons();

        // Update properties panel
        UpdatePropertiesPanel(_lastFocusedFilePath);
    }

    /// <summary>
    /// Refreshes the current directory, reloading all files and folders.
    /// </summary>
    public void Refresh()
    {
        RefreshFileList();

        // Try to restore focus to the same item
        FocusFirstOrSpecificItem(_lastFocusedFilePath);
    }

    public void FocusLastFileButton()
    {
        // If we have a last focused path, find the button for that path
        if (_lastFocusedFilePath is not null)
        {
            var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == _lastFocusedFilePath).Key;
            if (button is not null)
            {
                _termui.SetFocus(button);
                return;
            }
        }

        // Otherwise, focus the first file button
        var firstButton = _buttonPathMap.Keys.FirstOrDefault();
        if (firstButton is not null)
        {
            _termui.SetFocus(firstButton);
            _lastFocusedFilePath = _buttonPathMap[firstButton];
        }
    }

    /// <summary>
    /// Checks if the current focus is in the explorer.
    /// </summary>
    public bool IsFocusedInExplorer()
    {
        var focusedWidget = _termui.GetType()
            .GetField("_focusedWidget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(_termui) as IWidget;

        return IsFocusInExplorer(focusedWidget);
    }

    /// <summary>
    /// Focuses on a specific item by path, or the first item if no path is provided.
    /// </summary>
    private void FocusFirstOrSpecificItem(string? itemPath)
    {
        if (itemPath is not null)
        {
            // Try to find the button for the specific item
            var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == itemPath).Key;
            if (button is not null)
            {
                _termui.SetFocus(button);
                _lastFocusedFilePath = itemPath;
                return;
            }
        }

        // Focus the first item
        var firstButton = _buttonPathMap.Keys.FirstOrDefault();
        if (firstButton is not null)
        {
            _termui.SetFocus(firstButton);
            _lastFocusedFilePath = _buttonPathMap[firstButton];
        }
    }

    public void ApplySort(FilterType sortType)
    {
        _currentSort = sortType;
        RefreshFileList();
    }

    /// <summary>
    /// Sets the search filter text to filter displayed files and directories.
    /// </summary>
    public void SetSearchFilter(string? filterText)
    {
        _filterText = filterText;
        RefreshFileList();
        FocusFirstOrSpecificItem(null);
    }

    /// <summary>
    /// Clears the search filter.
    /// </summary>
    public void ClearSearchFilter(bool focusFileList = true)
    {
        _filterText = null;
        RefreshFileList();
        if (focusFileList)
        {
            FocusFirstOrSpecificItem(null);
        }
    }

    /// <summary>
    /// Navigates to a new directory and updates the history.
    /// </summary>
    public void NavigateToDirectory(string newDirectory, string? clickedItem = null)
    {
        if (!Directory.Exists(newDirectory))
        {
            return;
        }

        // Add current directory to back history with the item we're about to click
        // This way when we go back, we can focus on the item that led us to the next directory
        if (_historyBack.Count == 0 || _historyBack[^1].directory != _currentDirectory)
        {
            _historyBack.Add((_currentDirectory, clickedItem));
        }

        // Clear forward history when navigating to a new directory
        _historyForward.Clear();

        // Navigate to new directory
        _currentDirectory = newDirectory;
        _lastClickedItemInCurrentDir = clickedItem;

        RefreshFileList();

        // Focus the first item after navigation
        FocusFirstOrSpecificItem(null);
    }

    /// <summary>
    /// Goes back in the navigation history (Ctrl+U).
    /// </summary>
    public void GoBack()
    {
        if (_historyBack.Count == 0)
        {
            return;
        }

        // Add current directory to forward history
        _historyForward.Add((_currentDirectory, _lastClickedItemInCurrentDir));

        // Get the last directory from back history
        var (previousDirectory, lastClickedItem) = _historyBack[^1];
        _historyBack.RemoveAt(_historyBack.Count - 1);

        // Navigate to it
        _currentDirectory = previousDirectory;
        _lastClickedItemInCurrentDir = lastClickedItem;

        RefreshFileList();

        // Focus on the item that was clicked to enter the directory we're coming from
        FocusFirstOrSpecificItem(lastClickedItem);
    }

    /// <summary>
    /// Goes forward in the navigation history (Ctrl+Y).
    /// </summary>
    public void GoForward()
    {
        if (_historyForward.Count == 0)
        {
            return;
        }

        // Add current directory to back history
        _historyBack.Add((_currentDirectory, _lastClickedItemInCurrentDir));

        // Get the last directory from forward history
        var (nextDirectory, lastClickedItem) = _historyForward[^1];
        _historyForward.RemoveAt(_historyForward.Count - 1);

        // Navigate to it
        _currentDirectory = nextDirectory;
        _lastClickedItemInCurrentDir = lastClickedItem;

        RefreshFileList();

        // Focus on the first item or specific item if available
        FocusFirstOrSpecificItem(lastClickedItem);
    }

    private void RefreshFileList()
    {
        if (_leftColumn is null)
        {
            return;
        }

        // Clear existing file/folder buttons
        List<IWidget> existingButtons = [.. ((IWidget)_leftColumn).Children
            .Where(w => w is Button btn && btn.Name?.StartsWith("fileItem") == true)];

        foreach (var button in existingButtons)
        {
            _leftColumn.Remove(button);
        }

        // Clear button-path mapping
        _buttonPathMap.Clear();

        try
        {
            // Get directories and files
            var directories = Directory.GetDirectories(_currentDirectory)
                .Select(d => new DirectoryInfo(d))
                .ToList();

            var files = Directory.GetFiles(_currentDirectory)
                .Select(f => new FileInfo(f))
                .ToList();

            // Apply filter if set
            if (!string.IsNullOrWhiteSpace(_filterText))
            {
                directories = directories
                    .Where(d => d.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                files = files
                    .Where(f => f.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply sorting
            directories = _currentSort switch
            {
                FilterType.NameAsc => directories.OrderBy(d => d.Name).ToList(),
                FilterType.NameDesc => directories.OrderByDescending(d => d.Name).ToList(),
                FilterType.DateAsc => directories.OrderBy(d => d.LastWriteTime).ToList(),
                FilterType.DateDesc => directories.OrderByDescending(d => d.LastWriteTime).ToList(),
                FilterType.SizeAsc => directories.OrderBy(d => d.Name).ToList(), // Directories don't have size
                FilterType.SizeDesc => directories.OrderBy(d => d.Name).ToList(), // Directories don't have size
                _ => directories.OrderBy(d => d.Name).ToList()
            };

            files = _currentSort switch
            {
                FilterType.NameAsc => files.OrderBy(f => f.Name).ToList(),
                FilterType.NameDesc => files.OrderByDescending(f => f.Name).ToList(),
                FilterType.DateAsc => files.OrderBy(f => f.LastWriteTime).ToList(),
                FilterType.DateDesc => files.OrderByDescending(f => f.LastWriteTime).ToList(),
                FilterType.SizeAsc => files.OrderBy(f => f.Length).ToList(),
                FilterType.SizeDesc => files.OrderByDescending(f => f.Length).ToList(),
                _ => files.OrderBy(f => f.Name).ToList()
            };

            int yPosition = 0;
            int buttonIndex = 0;

            // Add directory buttons
            foreach (var directory in directories)
            {
                var button = new Button($"📁 {directory.Name}")
                {
                    Name = $"fileItem{buttonIndex}",
                    PositionX = "0ch",
                    PositionY = $"{yPosition}ch",
                    Width = "100%",
                    Height = "1ch",
                    TextAlign = TextAlign.Left,
                    BorderStyle = BorderStyle.None,
                    PaddingLeft = "0ch",
                    PaddingRight = "0ch",
                    PaddingTop = "0ch",
                    PaddingBottom = "0ch",
                    AllowWrapping = false,
                    BackgroundColor = ConsoleColor.Black,
                    ForegroundColor = ConsoleColor.White,
                    FocusBackgroundColor = ConsoleColor.DarkGray,
                    FocusForegroundColor = ConsoleColor.White
                };

                string itemPath = directory.FullName;
                _buttonPathMap[button] = itemPath;
                button.Click += (sender, e) => OnFileItemClick(itemPath, button);
                button.RightClick += (sender, args) => ShowContextMenu(args.X, args.Y, itemPath);

                _leftColumn.Add(button);

                yPosition += 1;
                buttonIndex++;
            }

            // Add file buttons
            foreach (var file in files)
            {
                var button = new Button($"📄 {file.Name}")
                {
                    Name = $"fileItem{buttonIndex}",
                    PositionX = "0ch",
                    PositionY = $"{yPosition}ch",
                    Width = "100%",
                    Height = "1ch",
                    TextAlign = TextAlign.Left,
                    BorderStyle = BorderStyle.None,
                    PaddingLeft = "0ch",
                    PaddingRight = "0ch",
                    PaddingTop = "0ch",
                    PaddingBottom = "0ch",
                    AllowWrapping = false,
                    BackgroundColor = ConsoleColor.Black,
                    ForegroundColor = ConsoleColor.White,
                    FocusBackgroundColor = ConsoleColor.DarkGray,
                    FocusForegroundColor = ConsoleColor.White
                };

                string itemPath = file.FullName;
                _buttonPathMap[button] = itemPath;
                button.Click += (sender, e) => OnFileItemClick(itemPath, button);
                button.RightClick += (sender, args) => ShowContextMenu(args.X, args.Y, itemPath);

                _leftColumn.Add(button);

                yPosition += 1;
                buttonIndex++;
            }
        }
        catch (Exception ex)
        {
            // Handle permission errors or other issues
            var errorButton = new Button($"Error: {ex.Message}")
            {
                Name = "fileItemError",
                PositionX = "0ch",
                PositionY = "0ch",
                Width = "100%",
                Height = "1ch",
                TextAlign = TextAlign.Left,
                BorderStyle = BorderStyle.None,
                PaddingLeft = "0ch",
                PaddingRight = "0ch",
                PaddingTop = "0ch",
                PaddingBottom = "0ch",
                BackgroundColor = ConsoleColor.Black,
                ForegroundColor = ConsoleColor.Red,
                FocusBackgroundColor = ConsoleColor.DarkGray,
                FocusForegroundColor = ConsoleColor.Red,
                Disabled = true
            };

            _leftColumn.Add(errorButton);
        }

        // Restore selection colors for previously selected items
        UpdateSelectionColors();
    }

    private void OnFileItemClick(string itemPath, Button button)
    {
        // Focus-then-activate: first click focuses, second click performs action.
        // When clicking, OnFocusChanged fires BEFORE OnFileItemClick and sets
        // _lastFocusedFilePath = itemPath. So we check _previousFocusedFilePath instead:
        // if the item was NOT previously focused, this is the first click (just focus).
        if (_previousFocusedFilePath != itemPath)
        {
            // First click: select the item (blue highlight) but don't activate
            SelectCurrentItem();
            return;
        }

        // If it's a directory, navigate to it
        if (Directory.Exists(itemPath))
        {
            NavigateToDirectory(itemPath, clickedItem: itemPath);
            return;
        }

        // For files: handle selection
        _selectedItems.Clear();
        _selectedItems.Add(itemPath);
        _lastFocusedFilePath = itemPath;

        UpdateSelectionColors();
        UpdateActionButtons();
    }

    private void UpdateSelectionColors()
    {
        if (_leftColumn is null)
        {
            return;
        }

        // Update all file item buttons
        foreach (var kvp in _buttonPathMap)
        {
            var button = kvp.Key;
            var path = kvp.Value;

            if (_selectedItems.Contains(path))
            {
                // Selected - make it light blue
                button.BackgroundColor = ConsoleColor.Blue;
                button.FocusBackgroundColor = ConsoleColor.Blue;
            }
            else
            {
                // Not selected - revert to default Black
                button.BackgroundColor = ConsoleColor.Black;
                button.FocusBackgroundColor = ConsoleColor.DarkGray;
            }
        }
    }

    private void UpdateActionButtons()
    {
        bool hasSelection = _selectedItems.Count > 0;

        if (_btnCopy is not null)
        {
            _btnCopy.Disabled = !hasSelection;
        }

        if (_btnMove is not null)
        {
            _btnMove.Disabled = !hasSelection;
        }

        if (_btnDelete is not null)
        {
            _btnDelete.Disabled = !hasSelection;
        }

        if (_btnRename is not null)
        {
            _btnRename.Disabled = !hasSelection;
        }
    }

    private void UpdatePropertiesPanel(string itemPath)
    {
        try
        {
            if (File.Exists(itemPath))
            {
                var fileInfo = new FileInfo(itemPath);

                if (_lblNameValue is not null)
                {
                    _lblNameValue.Content = fileInfo.Name;
                }

                if (_lblTypeValue is not null)
                {
                    _lblTypeValue.Content = "File";
                }

                if (_lblSizeValue is not null)
                {
                    _lblSizeValue.Content = FormatFileSize(fileInfo.Length);
                }

                if (_lblCreatedValue is not null)
                {
                    _lblCreatedValue.Content = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
                }

                if (_lblModifiedValue is not null)
                {
                    _lblModifiedValue.Content = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                }
            }
            else if (Directory.Exists(itemPath))
            {
                var dirInfo = new DirectoryInfo(itemPath);

                if (_lblNameValue is not null)
                {
                    _lblNameValue.Content = dirInfo.Name;
                }

                if (_lblTypeValue is not null)
                {
                    _lblTypeValue.Content = "Directory";
                }

                if (_lblSizeValue is not null)
                {
                    _lblSizeValue.Content = "-";
                }

                if (_lblCreatedValue is not null)
                {
                    _lblCreatedValue.Content = dirInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
                }

                if (_lblModifiedValue is not null)
                {
                    _lblModifiedValue.Content = dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                }
            }
        }
        catch
        {
            // Ignore errors when reading file properties
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public void UpdateLayout()
    {
        int contentHeight = Console.WindowHeight - TopBarHeight - 1;
        int leftColumnWidth = (int)(Console.WindowWidth * 0.7);
        int rightColumnWidth = Console.WindowWidth - leftColumnWidth;

        if (_leftColumn is not null)
        {
            _leftColumn.Width = $"{leftColumnWidth}ch";
            _leftColumn.Height = $"{contentHeight}ch";
            _leftColumn.PositionY = $"{TopBarHeight + 1}ch";
        }

        if (_rightColumn is not null)
        {
            _rightColumn.Width = $"{rightColumnWidth}ch";
            _rightColumn.Height = $"{contentHeight}ch";
            _rightColumn.PositionX = $"{leftColumnWidth}ch";
            _rightColumn.PositionY = $"{TopBarHeight + 1}ch";
        }
    }

    private void OnRenameClick(object? sender, EventArgs e)
    {
        if (_lastFocusedFilePath is null || _inputRename is null)
        {
            return;
        }

        // Store the item being renamed
        _itemBeingRenamed = _lastFocusedFilePath;

        // Get the current name (without path)
        string currentName = Path.GetFileName(_lastFocusedFilePath);

        // Set input value to current name
        _inputRename.Value = currentName;

        // Get the button for the focused item to position the overlay
        var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == _lastFocusedFilePath).Key;
        if (button is not null && _leftColumn is not null)
        {
            // Parse the button's PositionY to get its Y offset within the container
            string buttonPosY = button.PositionY;
            if (buttonPosY.EndsWith("ch"))
            {
                buttonPosY = buttonPosY[..^2]; // Remove "ch"
            }

            if (int.TryParse(buttonPosY, out int buttonYInContainer))
            {
                // Calculate absolute position
                // Y = TopBarHeight + 1 (container offset) + button's Y in container - scroll offset
                long scrollOffset = ((IWidget)_leftColumn).ScrollOffsetY;
                int inputY = TopBarHeight + 1 + buttonYInContainer - (int)scrollOffset;

                // X position is just a small offset from the left edge of the left column
                int inputX = 2;

                _inputRename.PositionX = $"{inputX}ch";
                _inputRename.PositionY = $"{inputY}ch";
            }
        }

        // Show the input and focus it
        _inputRename.Visible = true;
        _termui.SetFocus(_inputRename);
    }

    private void OnRenameInputEnter(object? sender, string newName)
    {
        if (_itemBeingRenamed is null || string.IsNullOrWhiteSpace(newName))
        {
            CloseRenameOverlay();
            return;
        }

        try
        {
            // Get the current name (without path)
            string currentName = Path.GetFileName(_itemBeingRenamed);

            // If name hasn't changed, just close the overlay without doing anything
            if (newName == currentName)
            {
                CloseRenameOverlay();
                return;
            }

            // Build the new path
            string directory = Path.GetDirectoryName(_itemBeingRenamed) ?? _currentDirectory;
            string newPath = Path.Combine(directory, newName);

            // Check if target already exists
            if (File.Exists(newPath) || Directory.Exists(newPath))
            {
                // TODO: Show error message
                CloseRenameOverlay();
                return;
            }

            // Perform the rename
            if (Directory.Exists(_itemBeingRenamed))
            {
                Directory.Move(_itemBeingRenamed, newPath);
            }
            else if (File.Exists(_itemBeingRenamed))
            {
                File.Move(_itemBeingRenamed, newPath);
            }

            // Update selection if the item was selected
            if (_selectedItems.Contains(_itemBeingRenamed))
            {
                _selectedItems.Remove(_itemBeingRenamed);
                _selectedItems.Add(newPath);
            }

            // Update last focused path
            if (_lastFocusedFilePath == _itemBeingRenamed)
            {
                _lastFocusedFilePath = newPath;
            }

            // Refresh the file list
            RefreshFileList();

            // Focus the renamed item
            FocusFirstOrSpecificItem(newPath);

            CloseRenameOverlay();
        }
        catch (Exception)
        {
            // TODO: Show error message
            CloseRenameOverlay();
        }
    }

    public void CloseRenameOverlay()
    {
        if (_inputRename is null)
        {
            return;
        }

        _inputRename.Visible = false;
        _itemBeingRenamed = null;

        // Return focus to the file explorer
        if (_lastFocusedFilePath is not null)
        {
            var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == _lastFocusedFilePath).Key;
            if (button is not null)
            {
                _termui.SetFocus(button);
            }
        }
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        if (_lastFocusedFilePath is null)
        {
            return;
        }

        // Update the confirmation text with the item name
        if (_deleteConfirmText is not null)
        {
            string itemName = Path.GetFileName(_lastFocusedFilePath);
            _deleteConfirmText.Content = $"Are you sure you want to delete '{itemName}'?";
        }

        // Show overlay and popup (centered)
        ShowDeleteConfirmation();
    }

    private void ShowDeleteConfirmation()
    {
        if (_deleteOverlay is null || _deleteConfirmPopup is null || _btnDeleteNo is null)
        {
            return;
        }

        // Hide all other UI elements
        if (_leftColumn is not null) _leftColumn.Visible = false;
        if (_rightColumn is not null) _rightColumn.Visible = false;
        if (_navigationHints is not null) _navigationHints.Visible = false;

        // Center the popup
        const int popupWidth = 50;
        const int popupHeight = 10;
        int centerX = (Console.WindowWidth - popupWidth) / 2;
        int centerY = (Console.WindowHeight - popupHeight) / 2;

        _deleteConfirmPopup.PositionX = $"{centerX}ch";
        _deleteConfirmPopup.PositionY = $"{centerY}ch";

        // Show overlay and popup
        _deleteOverlay.Visible = true;
        _deleteConfirmPopup.Visible = true;

        // Focus the "No" button by default (safer option)
        _termui.SetFocus(_btnDeleteNo);
    }

    private void OnDeleteConfirmYes(object? sender, EventArgs e)
    {
        if (_lastFocusedFilePath is null)
        {
            CloseDeleteConfirmation();
            return;
        }

        try
        {
            // Delete the file or directory
            if (Directory.Exists(_lastFocusedFilePath))
            {
                Directory.Delete(_lastFocusedFilePath, true); // Recursive delete
            }
            else if (File.Exists(_lastFocusedFilePath))
            {
                File.Delete(_lastFocusedFilePath);
            }

            // Remove from selection
            _selectedItems.Remove(_lastFocusedFilePath);

            // Clear last focused path
            _lastFocusedFilePath = null;

            // Refresh the file list
            RefreshFileList();

            // Focus the first item
            FocusFirstOrSpecificItem(null);

            CloseDeleteConfirmation();
        }
        catch (Exception)
        {
            // TODO: Show error message
            CloseDeleteConfirmation();
        }
    }

    private void OnDeleteConfirmNo(object? sender, EventArgs e)
    {
        CloseDeleteConfirmation();
    }

    private void CloseDeleteConfirmation()
    {
        if (_deleteOverlay is null || _deleteConfirmPopup is null)
        {
            return;
        }

        _deleteOverlay.Visible = false;
        _deleteConfirmPopup.Visible = false;

        // Show all other UI elements again
        if (_leftColumn is not null) _leftColumn.Visible = true;
        if (_rightColumn is not null) _rightColumn.Visible = true;
        if (_navigationHints is not null) _navigationHints.Visible = true;

        // Return focus to the file explorer
        if (_lastFocusedFilePath is not null)
        {
            var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == _lastFocusedFilePath).Key;
            if (button is not null)
            {
                _termui.SetFocus(button);
            }
        }
    }

    private void OnCopyClick(object? sender, EventArgs e)
    {
        if (_clipboardOperation != ClipboardOperation.None)
        {
            // Paste operation
            PerformPaste();
        }
        else
        {
            // Copy operation
            if (_lastFocusedFilePath is null)
            {
                return;
            }

            _clipboardOperation = ClipboardOperation.Copy;
            _clipboardItemPath = _lastFocusedFilePath;
            UpdateButtonLabels();
        }
    }

    private void OnMoveClick(object? sender, EventArgs e)
    {
        if (_clipboardOperation != ClipboardOperation.None)
        {
            // Cancel operation
            _clipboardOperation = ClipboardOperation.None;
            _clipboardItemPath = null;
            UpdateButtonLabels();
        }
        else
        {
            // Move operation
            if (_lastFocusedFilePath is null)
            {
                return;
            }

            _clipboardOperation = ClipboardOperation.Move;
            _clipboardItemPath = _lastFocusedFilePath;
            UpdateButtonLabels();
        }
    }

    private void PerformPaste()
    {
        if (_clipboardItemPath is null || _clipboardOperation == ClipboardOperation.None)
        {
            return;
        }

        try
        {
            string itemName = Path.GetFileName(_clipboardItemPath);
            string targetPath = Path.Combine(_currentDirectory, itemName);

            // Check if target already exists
            if (File.Exists(targetPath) || Directory.Exists(targetPath))
            {
                // TODO: Show error message or ask for rename
                _clipboardOperation = ClipboardOperation.None;
                _clipboardItemPath = null;
                UpdateButtonLabels();
                return;
            }

            if (_clipboardOperation == ClipboardOperation.Copy)
            {
                // Copy file or directory
                if (Directory.Exists(_clipboardItemPath))
                {
                    CopyDirectory(_clipboardItemPath, targetPath);
                }
                else if (File.Exists(_clipboardItemPath))
                {
                    File.Copy(_clipboardItemPath, targetPath);
                }

                // Keep clipboard for multiple pastes
            }
            else if (_clipboardOperation == ClipboardOperation.Move)
            {
                // Move file or directory
                if (Directory.Exists(_clipboardItemPath))
                {
                    Directory.Move(_clipboardItemPath, targetPath);
                }
                else if (File.Exists(_clipboardItemPath))
                {
                    File.Move(_clipboardItemPath, targetPath);
                }

                // Clear clipboard after move
                _clipboardOperation = ClipboardOperation.None;
                _clipboardItemPath = null;
                UpdateButtonLabels();
            }

            // Refresh the file list
            RefreshFileList();

            // Focus the pasted/moved item
            FocusFirstOrSpecificItem(targetPath);
        }
        catch (Exception)
        {
            // TODO: Show error message
            _clipboardOperation = ClipboardOperation.None;
            _clipboardItemPath = null;
            UpdateButtonLabels();
        }
    }

    private void CopyDirectory(string sourceDir, string targetDir)
    {
        // Create target directory
        Directory.CreateDirectory(targetDir);

        // Copy all files
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile);
        }

        // Copy all subdirectories recursively
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(subDir, targetSubDir);
        }
    }

    private void ShowContextMenu(int x, int y, string targetPath)
    {
        if (_contextMenu is null)
            return;

        // Select the right-clicked item (blue highlight)
        _selectedItems.Clear();
        _selectedItems.Add(targetPath);
        _lastFocusedFilePath = targetPath;
        UpdateSelectionColors();
        UpdateActionButtons();

        _contextMenuTargetPath = targetPath;

        // Clamp to screen bounds (18ch wide, 7ch tall including border)
        int menuWidth = 18;
        int menuHeight = 7;
        if (x + menuWidth > Console.WindowWidth)
            x = Console.WindowWidth - menuWidth;
        if (y + menuHeight > Console.WindowHeight)
            y = Console.WindowHeight - menuHeight;

        _contextMenu.PositionX = $"{x}ch";
        _contextMenu.PositionY = $"{y}ch";
        _contextMenu.Visible = true;

        if (_ctxOpen is not null)
            _termui.SetFocus(_ctxOpen);
    }

    public void CloseContextMenu()
    {
        if (_contextMenu is null)
            return;

        _contextMenu.Visible = false;
        _contextMenuTargetPath = null;
    }

    public bool IsContextMenuOpen => _contextMenu?.Visible == true;

    public bool IsRenameOpen => _inputRename?.Visible == true;

    public bool ContainsContextMenuWidget(IWidget widget)
    {
        return widget == _ctxOpen || widget == _ctxCopy || widget == _ctxMove
            || widget == _ctxRename || widget == _ctxDelete;
    }

    private void OnContextOpen()
    {
        if (_contextMenuTargetPath is null) return;

        if (Directory.Exists(_contextMenuTargetPath))
        {
            NavigateToDirectory(_contextMenuTargetPath, clickedItem: _contextMenuTargetPath);
        }
    }

    private void OnContextCopy()
    {
        if (_contextMenuTargetPath is null) return;

        _clipboardOperation = ClipboardOperation.Copy;
        _clipboardItemPath = _contextMenuTargetPath;
        UpdateButtonLabels();
    }

    private void OnContextMove()
    {
        if (_contextMenuTargetPath is null) return;

        _clipboardOperation = ClipboardOperation.Move;
        _clipboardItemPath = _contextMenuTargetPath;
        UpdateButtonLabels();
    }

    private void OnContextRename()
    {
        if (_contextMenuTargetPath is null) return;

        _lastFocusedFilePath = _contextMenuTargetPath;
        OnRenameClick(null, EventArgs.Empty);
    }

    private void OnContextDelete()
    {
        if (_contextMenuTargetPath is null) return;

        _lastFocusedFilePath = _contextMenuTargetPath;
        OnDeleteClick(null, EventArgs.Empty);
    }

    private void UpdateButtonLabels()
    {
        if (_btnCopy is not null && _btnMove is not null)
        {
            if (_clipboardOperation != ClipboardOperation.None)
            {
                // Show Paste and Cancel
                _btnCopy.Text = "Paste";
                _btnMove.Text = "Cancel";
            }
            else
            {
                // Show Copy and Move
                _btnCopy.Text = "Copy";
                _btnMove.Text = "Move";
            }
        }
    }
}

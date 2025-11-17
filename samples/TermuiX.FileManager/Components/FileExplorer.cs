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
    private Button? _btnMove;
    private Button? _btnDelete;
    private Button? _btnRename;
    private Button? _btnProperties;

    private string _currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly HashSet<string> _selectedItems = [];
    private readonly Dictionary<Button, string> _buttonPathMap = [];
    private string? _filterText;

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
    BackgroundColor='Black'
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
        _btnMove = _termui.GetWidget<Button>("btnMove");
        _btnDelete = _termui.GetWidget<Button>("btnDelete");
        _btnRename = _termui.GetWidget<Button>("btnRename");
        _btnProperties = _termui.GetWidget<Button>("btnProperties");

        RefreshFileList();
    }

    public Button? GetCopyButton() => _btnCopy;

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
                .OrderBy(d => d.Name)
                .ToList();

            var files = Directory.GetFiles(_currentDirectory)
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.Name)
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
    }

    private void OnFileItemClick(string itemPath, Button button)
    {
        // Single selection mode: clear previous selections
        // TODO: Implement Ctrl+Click multi-selection when framework supports modifier detection in Click events
        _selectedItems.Clear();

        // Add clicked item to selection
        _selectedItems.Add(itemPath);

        // Update button colors for all file item buttons
        UpdateSelectionColors();

        // Enable/disable action buttons based on selection
        UpdateActionButtons();

        // Update properties panel with selected item info
        UpdatePropertiesPanel(itemPath);
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

        if (_btnProperties is not null)
        {
            _btnProperties.Disabled = !hasSelection;
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

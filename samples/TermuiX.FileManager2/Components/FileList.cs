using System.Formats.Tar;
using System.IO.Compression;
using System.Security;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class FileList
{
    private readonly TermuiXLib _termui;
    private StackPanel? _fileListPanel;

    private string _currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly Dictionary<Button, string> _buttonPathMap = [];
    private readonly Dictionary<string, Checkbox> _checkboxPathMap = [];
    private readonly HashSet<string> _selectedItems = [];
    private string? _lastFocusedPath;
    private string? _previousFocusedPath;
    private string? _filterText;
    private SortMode _sortMode = SortMode.NameAsc;
    private int _itemCounter;

    // Multi-select mode
    private bool _multiSelectMode;
    private readonly HashSet<string> _checkedItems = [];

    // Clipboard
    private ClipboardOp _clipboardOp = ClipboardOp.None;
    private string? _clipboardPath;
    private readonly List<string> _clipboardPaths = [];

    // History
    private readonly List<(string dir, string? focused)> _historyBack = [];
    private readonly List<(string dir, string? focused)> _historyForward = [];

    // Inline rename
    private Container? _renameOverlay;
    private Input? _renameInput;
    private string? _renamingPath;

    // Events
    public event EventHandler<string>? DirectoryChanged;
    public event EventHandler<FileListStats>? StatsChanged;
    public event EventHandler<(int x, int y, string path)>? ContextMenuRequested;
    public event EventHandler<(int x, int y)>? EmptyAreaRightClicked;
    public event EventHandler<string>? OpenFileRequested;

    public string CurrentDirectory => _currentDirectory;
    public bool CanGoBack => _historyBack.Count > 0;
    public bool CanGoForward => _historyForward.Count > 0;
    public string? SelectedPath => _lastFocusedPath;
    public string? ClipboardPath => _clipboardPath;
    public ClipboardOp CurrentClipboardOp => _clipboardOp;
    public bool IsMultiSelectMode => _multiSelectMode;
    public IReadOnlyCollection<string> CheckedItems => _checkedItems;
    public IReadOnlyCollection<string> ClipboardPaths => _clipboardPaths;

    public FileList(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='fileListPanel' Direction='Vertical' Width='fill' Height='100%'
    ScrollY='true' BackgroundColor='#191919' />";
    }

    public string BuildOverlayXml()
    {
        return @"
<Container Name='fileRenameOverlay' Width='40ch' Height='1ch' PositionX='0ch' PositionY='0ch'
    BackgroundColor='#191919' Visible='false'>
    <Input Name='fileRenameInput' Width='100%' Height='1ch'
        BackgroundColor='#191919' ForegroundColor='#d0d0d0'
        FocusBackgroundColor='#191919' FocusForegroundColor='#d0d0d0'
        BorderStyle='None' CursorColor='#d0d0d0'
        PaddingLeft='0ch' PaddingRight='0ch' />
</Container>";
    }

    public void Initialize()
    {
        _fileListPanel = _termui.GetWidget<StackPanel>("fileListPanel");
        _renameOverlay = _termui.GetWidget<Container>("fileRenameOverlay");
        _renameInput = _termui.GetWidget<Input>("fileRenameInput");

        if (_renameInput is not null)
        {
            _renameInput.EnterPressed += (_, newName) =>
            {
                if (_renamingPath is not null && !string.IsNullOrWhiteSpace(newName))
                    PerformRename(_renamingPath, newName.Trim());
                CloseRenameOverlay();
            };
            _renameInput.EscapePressed += (_, _) => CloseRenameOverlay();
        }
    }

    // --- Inline Rename ---

    public bool IsRenameOpen => _renameOverlay?.Visible is true;

    public void StartInlineRename(string path)
    {
        if (_renameOverlay is null || _renameInput is null || _fileListPanel is null) return;

        _renamingPath = path;

        // Find the button for this path and its index
        var button = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == path).Key;
        if (button is null) return;

        int index = ((IWidget)_fileListPanel).Children.IndexOf(button);
        if (index < 0) return;

        // Calculate Y: toolbar height (~5ch) + index * 1ch - scroll offset
        // The fileListPanel starts after: toolbar(~5ch rows) + no additional offset
        // But as an appRoot overlay, we need absolute screen position.
        // StackPanel header: toolbar ~5ch, then contentRow starts.
        // Inside contentRow: treePanel(28ch) + line(1ch) = 29ch X offset
        // Y: toolbar height + index - scroll offset
        long scrollOffset = ((IWidget)_fileListPanel).ScrollOffsetY;
        int toolbarHeight = 5; // toolbar rows + separator
        int y = toolbarHeight + index - (int)scrollOffset;
        int x = 30; // after tree panel (28ch) + separator (1ch) + 1ch padding

        _renameOverlay.PositionX = $"{x}ch";
        _renameOverlay.PositionY = $"{y}ch";

        // Pre-fill with current filename
        string currentName = Path.GetFileName(path);
        _renameInput.Value = currentName;

        _renameOverlay.Visible = true;
        _termui.SetFocus(_renameInput);
    }

    public void CloseRenameOverlay()
    {
        if (_renameOverlay is null) return;
        _renameOverlay.Visible = false;
        _renamingPath = null;

        // Return focus to the file list
        FocusItem(_lastFocusedPath);
    }

    public bool IsRenameWidget(IWidget widget) => widget == _renameInput;

    // --- Multi-Select ---

    public void ToggleMultiSelect()
    {
        _multiSelectMode = !_multiSelectMode;
        if (!_multiSelectMode)
            _checkedItems.Clear();
        else if (_lastFocusedPath is not null)
            _checkedItems.Add(_lastFocusedPath);

        RefreshList();
        FocusItem(_lastFocusedPath);
    }

    public void SelectAll()
    {
        if (!_multiSelectMode)
        {
            _multiSelectMode = true;
            _checkedItems.Clear();
        }

        foreach (var path in _buttonPathMap.Values)
            _checkedItems.Add(path);

        RefreshList();
        FocusItem(_lastFocusedPath);
    }

    public void SetMultiSelectClipboard(ClipboardOp op)
    {
        if (_checkedItems.Count == 0) return;
        _clipboardOp = op;
        _clipboardPaths.Clear();
        _clipboardPaths.AddRange(_checkedItems);
        _clipboardPath = _clipboardPaths.Count > 0 ? _clipboardPaths[0] : null;
        _multiSelectMode = false;
        _checkedItems.Clear();
        RefreshList();
        FocusItem(_lastFocusedPath);
    }

    public void DeleteChecked()
    {
        if (_checkedItems.Count == 0) return;
        var paths = _checkedItems.ToList();
        foreach (var path in paths)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }
        _checkedItems.Clear();
        _multiSelectMode = false;
        _selectedItems.Clear();
        _lastFocusedPath = null;
        RefreshList();
        FocusFirst();
    }

    // --- Navigation ---

    public void NavigateTo(string directory, bool addToHistory = true)
    {
        if (!Directory.Exists(directory)) return;

        if (addToHistory && _currentDirectory != directory)
        {
            _historyBack.Add((_currentDirectory, _lastFocusedPath));
            _historyForward.Clear();
        }

        _currentDirectory = directory;
        _selectedItems.Clear();
        _lastFocusedPath = null;
        _previousFocusedPath = null;
        if (_multiSelectMode)
        {
            _multiSelectMode = false;
            _checkedItems.Clear();
        }
        RefreshList();
        DirectoryChanged?.Invoke(this, directory);
    }

    public void GoBack()
    {
        if (_historyBack.Count == 0) return;

        _historyForward.Add((_currentDirectory, _lastFocusedPath));
        var (dir, focused) = _historyBack[^1];
        _historyBack.RemoveAt(_historyBack.Count - 1);

        _currentDirectory = dir;
        _selectedItems.Clear();
        RefreshList();
        FocusItem(focused);
        DirectoryChanged?.Invoke(this, dir);
    }

    public void GoForward()
    {
        if (_historyForward.Count == 0) return;

        _historyBack.Add((_currentDirectory, _lastFocusedPath));
        var (dir, focused) = _historyForward[^1];
        _historyForward.RemoveAt(_historyForward.Count - 1);

        _currentDirectory = dir;
        _selectedItems.Clear();
        RefreshList();
        FocusItem(focused);
        DirectoryChanged?.Invoke(this, dir);
    }

    public void SetFilter(string? text)
    {
        _filterText = string.IsNullOrWhiteSpace(text) ? null : text;
        RefreshList();
    }

    public void SetSort(SortMode mode)
    {
        _sortMode = mode;
        RefreshList();
    }

    public void Refresh()
    {
        var focused = _lastFocusedPath;
        RefreshList();
        FocusItem(focused);
    }

    public void SetClipboard(ClipboardOp op, string? path)
    {
        _clipboardOp = op;
        _clipboardPath = path;
        _clipboardPaths.Clear();
        if (path is not null)
            _clipboardPaths.Add(path);
    }

    public void Paste()
    {
        if (_clipboardPaths.Count == 0 && _clipboardPath is not null)
            _clipboardPaths.Add(_clipboardPath);

        if (_clipboardPaths.Count == 0 || _clipboardOp == ClipboardOp.None) return;

        try
        {
            string? lastTarget = null;
            foreach (var src in _clipboardPaths)
            {
                string itemName = Path.GetFileName(src);
                string targetPath = Path.Combine(_currentDirectory, itemName);

                if (File.Exists(targetPath) || Directory.Exists(targetPath)) continue;

                if (_clipboardOp == ClipboardOp.Copy)
                {
                    if (Directory.Exists(src))
                        CopyDirectoryRecursive(src, targetPath);
                    else if (File.Exists(src))
                        File.Copy(src, targetPath);
                }
                else if (_clipboardOp == ClipboardOp.Move)
                {
                    if (Directory.Exists(src))
                        Directory.Move(src, targetPath);
                    else if (File.Exists(src))
                        File.Move(src, targetPath);
                }
                lastTarget = targetPath;
            }

            if (_clipboardOp == ClipboardOp.Move)
            {
                _clipboardOp = ClipboardOp.None;
                _clipboardPath = null;
                _clipboardPaths.Clear();
            }

            RefreshList();
            FocusItem(lastTarget);
        }
        catch
        {
            _clipboardOp = ClipboardOp.None;
            _clipboardPath = null;
            _clipboardPaths.Clear();
        }
    }

    public void PerformRename(string oldPath, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;

        try
        {
            string dir = Path.GetDirectoryName(oldPath) ?? _currentDirectory;
            string newPath = Path.Combine(dir, newName);

            if (File.Exists(newPath) || Directory.Exists(newPath)) return;

            if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
            else if (File.Exists(oldPath))
                File.Move(oldPath, newPath);

            if (_selectedItems.Remove(oldPath))
                _selectedItems.Add(newPath);
            if (_lastFocusedPath == oldPath)
                _lastFocusedPath = newPath;

            RefreshList();
            FocusItem(newPath);
        }
        catch { }
    }

    public void PerformDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else if (File.Exists(path))
                File.Delete(path);

            _selectedItems.Remove(path);
            if (_lastFocusedPath == path) _lastFocusedPath = null;

            RefreshList();
            FocusFirst();
        }
        catch { }
    }

    public void CreateNewFile(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
            string path = Path.Combine(_currentDirectory, name);
            if (File.Exists(path) || Directory.Exists(path)) return;

            File.Create(path).Dispose();
            RefreshList();
            FocusItem(path);
        }
        catch { }
    }

    public void CreateNewFolder(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
            string path = Path.Combine(_currentDirectory, name);
            if (File.Exists(path) || Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
            RefreshList();
            FocusItem(path);
        }
        catch { }
    }

    // --- Compress ---

    public void CompressSingle(string path, string format)
    {
        try
        {
            string baseName = Path.GetFileName(path);
            string ext = format == "zip" ? ".zip" : ".tar.gz";
            string archivePath = Path.Combine(_currentDirectory, baseName + ext);
            archivePath = GetUniqueFilePath(archivePath);

            if (format == "zip")
            {
                CompressToZip([path], archivePath);
            }
            else
            {
                CompressToTarGz([path], archivePath);
            }

            RefreshList();
            FocusItem(archivePath);
        }
        catch { }
    }

    public void CompressChecked(string format)
    {
        if (_checkedItems.Count == 0) return;

        try
        {
            string ext = format == "zip" ? ".zip" : ".tar.gz";
            string archivePath = Path.Combine(_currentDirectory, "archive" + ext);
            archivePath = GetUniqueFilePath(archivePath);

            var paths = _checkedItems.ToList();

            if (format == "zip")
            {
                CompressToZip(paths, archivePath);
            }
            else
            {
                CompressToTarGz(paths, archivePath);
            }

            _checkedItems.Clear();
            _multiSelectMode = false;
            RefreshList();
            FocusItem(archivePath);
        }
        catch { }
    }

    private static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path)) return path;

        string dir = Path.GetDirectoryName(path) ?? ".";
        string nameNoExt = Path.GetFileNameWithoutExtension(path);
        string ext = Path.GetExtension(path);

        // Handle double extensions like .tar.gz
        if (path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
        {
            nameNoExt = Path.GetFileNameWithoutExtension(nameNoExt);
            ext = ".tar.gz";
        }

        for (int i = 2; i < 100; i++)
        {
            string candidate = Path.Combine(dir, $"{nameNoExt}_{i}{ext}");
            if (!File.Exists(candidate)) return candidate;
        }

        return path;
    }

    private static void CompressToZip(List<string> paths, string archivePath)
    {
        using var zip = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                zip.CreateEntryFromFile(path, Path.GetFileName(path));
            }
            else if (Directory.Exists(path))
            {
                string dirName = Path.GetFileName(path);
                AddDirectoryToZip(zip, path, dirName);
            }
        }
    }

    private static void AddDirectoryToZip(ZipArchive zip, string sourceDir, string entryPrefix)
    {
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            zip.CreateEntryFromFile(file, Path.Combine(entryPrefix, Path.GetFileName(file)));
        }
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            AddDirectoryToZip(zip, dir, Path.Combine(entryPrefix, Path.GetFileName(dir)));
        }
    }

    private static void CompressToTarGz(List<string> paths, string archivePath)
    {
        using var fileStream = File.Create(archivePath);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        using var tarWriter = new TarWriter(gzipStream);

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                tarWriter.WriteEntry(path, Path.GetFileName(path));
            }
            else if (Directory.Exists(path))
            {
                string dirName = Path.GetFileName(path);
                AddDirectoryToTar(tarWriter, path, dirName);
            }
        }
    }

    private static void AddDirectoryToTar(TarWriter tar, string sourceDir, string entryPrefix)
    {
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            tar.WriteEntry(file, Path.Combine(entryPrefix, Path.GetFileName(file)));
        }
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            AddDirectoryToTar(tar, dir, Path.Combine(entryPrefix, Path.GetFileName(dir)));
        }
    }

    // --- Extract ---

    public void ExtractArchive(string archivePath)
    {
        if (!File.Exists(archivePath)) return;

        try
        {
            // Create a subfolder named after the archive (without extension)
            string archiveName = Path.GetFileNameWithoutExtension(archivePath);
            if (archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                || archivePath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
            {
                archiveName = Path.GetFileNameWithoutExtension(archiveName);
            }

            string extractDir = Path.Combine(_currentDirectory, archiveName);
            extractDir = GetUniqueDirectoryPath(extractDir);
            Directory.CreateDirectory(extractDir);

            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(archivePath, extractDir);
            }
            else if (archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                     || archivePath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
            {
                ExtractTarGz(archivePath, extractDir);
            }
            else if (archivePath.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
            {
                ExtractTar(archivePath, extractDir);
            }
            else if (archivePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            {
                ExtractGz(archivePath, extractDir);
            }

            RefreshList();
            FocusItem(extractDir);
        }
        catch { }
    }

    public void ExtractChecked()
    {
        if (_checkedItems.Count == 0) return;

        try
        {
            var paths = _checkedItems.Where(p => File.Exists(p) && IsExtractable(p)).ToList();
            string? lastExtracted = null;

            foreach (var path in paths)
            {
                string archiveName = Path.GetFileNameWithoutExtension(path);
                if (path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
                {
                    archiveName = Path.GetFileNameWithoutExtension(archiveName);
                }

                string extractDir = Path.Combine(_currentDirectory, archiveName);
                extractDir = GetUniqueDirectoryPath(extractDir);
                Directory.CreateDirectory(extractDir);

                if (path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    ZipFile.ExtractToDirectory(path, extractDir);
                }
                else if (path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                         || path.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractTarGz(path, extractDir);
                }
                else if (path.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractTar(path, extractDir);
                }
                else if (path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractGz(path, extractDir);
                }

                lastExtracted = extractDir;
            }

            _checkedItems.Clear();
            _multiSelectMode = false;
            RefreshList();
            FocusItem(lastExtracted);
        }
        catch { }
    }

    private static bool IsExtractable(string path)
    {
        return path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".tar", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetUniqueDirectoryPath(string path)
    {
        if (!Directory.Exists(path)) return path;

        for (int i = 2; i < 100; i++)
        {
            string candidate = $"{path}_{i}";
            if (!Directory.Exists(candidate)) return candidate;
        }

        return path;
    }

    private static void ExtractTarGz(string archivePath, string extractDir)
    {
        using var fileStream = File.OpenRead(archivePath);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gzipStream, extractDir, overwriteFiles: false);
    }

    private static void ExtractTar(string archivePath, string extractDir)
    {
        using var fileStream = File.OpenRead(archivePath);
        TarFile.ExtractToDirectory(fileStream, extractDir, overwriteFiles: false);
    }

    private static void ExtractGz(string archivePath, string extractDir)
    {
        // Single .gz file — decompress to a file inside extractDir
        string outputName = Path.GetFileNameWithoutExtension(archivePath);
        string outputPath = Path.Combine(extractDir, outputName);

        using var fileStream = File.OpenRead(archivePath);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var outputStream = File.Create(outputPath);
        gzipStream.CopyTo(outputStream);
    }

    private void RefreshList()
    {
        if (_fileListPanel is null) return;

        // Clear
        var toRemove = ((IWidget)_fileListPanel).Children.ToList();
        foreach (var child in toRemove) _fileListPanel.Remove(child);
        _buttonPathMap.Clear();
        _checkboxPathMap.Clear();
        _itemCounter = 0;

        try
        {
            var dirs = Directory.GetDirectories(_currentDirectory)
                .Select(d => new DirectoryInfo(d))
                .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                .ToList();

            var files = Directory.GetFiles(_currentDirectory)
                .Select(f => new FileInfo(f))
                .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                .ToList();

            // Filter
            if (_filterText is not null)
            {
                dirs = dirs.Where(d => d.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase)).ToList();
                files = files.Where(f => f.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sort
            dirs = SortDirectories(dirs);
            files = SortFiles(files);

            long totalSize = 0;

            foreach (var dir in dirs)
                AddEntry(dir.FullName, $"📁 {dir.Name}", null, dir.LastWriteTime);

            foreach (var file in files)
            {
                AddEntry(file.FullName, $"📄 {file.Name}", file.Length, file.LastWriteTime);
                totalSize += file.Length;
            }

            UpdateSelectionColors();
            EmitStats(dirs.Count + files.Count, totalSize);
        }
        catch
        {
            EmitStats(0, 0);
        }

        // Add empty-area fill button for right-click in empty space
        AddEmptyAreaButton();
    }

    private void AddEmptyAreaButton()
    {
        if (_fileListPanel is null) return;

        var name = $"fl_empty_{_itemCounter++}";
        bool isEmpty = _buttonPathMap.Count == 0;
        var text = isEmpty ? "Right-click for options" : " ";
        var escaped = SecurityElement.Escape(text);
        var fgColor = isEmpty ? "#808080" : "#191919";
        // Use 'fill' only when directory is empty (no scroll needed).
        // Otherwise use a small fixed height so content can overflow and trigger scrollbar.
        var height = isEmpty ? "fill" : "3ch";
        var xml = $@"<Button Name='{name}' Width='100%' Height='{height}' BorderStyle='None'
            PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='0ch' PaddingBottom='0ch'
            BackgroundColor='#191919' FocusBackgroundColor='#191919'
            TextColor='{fgColor}' FocusTextColor='{fgColor}' TextAlign='Left'
            AllowWrapping='false'>{escaped}</Button>";
        _fileListPanel.Add(xml);

        var btn = _termui.GetWidget<Button>(name);
        if (btn is not null)
        {
            btn.RightClick += (_, args) => EmptyAreaRightClicked?.Invoke(this, (args.X, args.Y));
            btn.Click += (_, _) =>
            {
                // Click on empty area deselects
                _selectedItems.Clear();
                UpdateSelectionColors();
                EmitCurrentStats();
            };
        }
    }

    private void AddEntry(string fullPath, string label, long? size, DateTime modified)
    {
        if (_fileListPanel is null) return;

        var escaped = SecurityElement.Escape(label);
        var name = $"fl_{_itemCounter++}";

        if (_multiSelectMode)
        {
            var cbName = $"cb_{_itemCounter}";
            var xml = $@"<StackPanel Direction='Horizontal' Width='100%' Height='1ch'
                BackgroundColor='#191919'>
                <Checkbox Name='{cbName}' Checked='{_checkedItems.Contains(fullPath).ToString().ToLower()}'
                    BackgroundColor='#191919' FocusBackgroundColor='#2a2a2a'
                    ForegroundColor='#d0d0d0' FocusForegroundColor='#ffffff' />
                <Button Name='{name}' Width='fill' Height='1ch' BorderStyle='None'
                    PaddingLeft='0ch' PaddingRight='1ch' PaddingTop='0ch' PaddingBottom='0ch'
                    BackgroundColor='#191919' FocusBackgroundColor='#2a2a2a'
                    TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'
                    AllowWrapping='false'>{escaped}</Button>
            </StackPanel>";
            _fileListPanel.Add(xml);

            var cb = _termui.GetWidget<Checkbox>(cbName);
            if (cb is not null)
            {
                _checkboxPathMap[fullPath] = cb;
                cb.CheckedChanged += (_, isChecked) =>
                {
                    if (isChecked)
                    {
                        _checkedItems.Add(fullPath);
                    }
                    else
                    {
                        _checkedItems.Remove(fullPath);
                    }
                    EmitCurrentStats();
                };
            }
        }
        else
        {
            var xml = $@"<Button Name='{name}' Width='100%' Height='1ch' BorderStyle='None'
                PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='0ch' PaddingBottom='0ch'
                BackgroundColor='#191919' FocusBackgroundColor='#2a2a2a'
                TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'
                AllowWrapping='false'>{escaped}</Button>";
            _fileListPanel.Add(xml);
        }

        var btn = _termui.GetWidget<Button>(name);
        if (btn is not null)
        {
            _buttonPathMap[btn] = fullPath;
            btn.Click += (_, _) => OnItemClick(fullPath, btn);
            btn.RightClick += (_, args) => ContextMenuRequested?.Invoke(this, (args.X, args.Y, fullPath));
        }
    }

    private void OnItemClick(string path, Button button)
    {
        if (_multiSelectMode)
        {
            // Toggle checkbox
            if (_checkboxPathMap.TryGetValue(path, out var cb))
            {
                cb.Checked = !cb.Checked;
            }
            return;
        }

        // Focus-then-activate pattern
        if (_previousFocusedPath != path)
        {
            // First click: select
            _selectedItems.Clear();
            _selectedItems.Add(path);
            _previousFocusedPath = _lastFocusedPath;
            _lastFocusedPath = path;
            UpdateSelectionColors();
            EmitCurrentStats();
            return;
        }

        // Second click: activate
        if (Directory.Exists(path))
        {
            NavigateTo(path);
        }
        else if (File.Exists(path))
        {
            OpenFileRequested?.Invoke(this, path);
        }
    }

    public void OnFocusChanged(IWidget? widget)
    {
        if (widget is Button btn && _buttonPathMap.TryGetValue(btn, out string? path))
        {
            _previousFocusedPath = _lastFocusedPath;
            _lastFocusedPath = path;

            if (!_multiSelectMode)
            {
                _selectedItems.Clear();
                _selectedItems.Add(path);
                UpdateSelectionColors();
                EmitCurrentStats();
            }
        }
    }

    private void UpdateSelectionColors()
    {
        foreach (var (btn, path) in _buttonPathMap)
        {
            if (_selectedItems.Contains(path))
            {
                btn.BackgroundColor = global::TermuiX.Color.Parse("#1a3050");
                btn.TextColor = global::TermuiX.Color.Parse("#c0c0c0");
            }
            else
            {
                btn.BackgroundColor = global::TermuiX.Color.Parse("#191919");
                btn.TextColor = global::TermuiX.Color.Parse("#d0d0d0");
            }
        }
    }

    private int SelectedCount => _multiSelectMode ? _checkedItems.Count : _selectedItems.Count;

    private void EmitStats(int count, long totalSize)
    {
        StatsChanged?.Invoke(this, new FileListStats(count, SelectedCount, totalSize));
    }

    private void EmitCurrentStats()
    {
        long totalSize = 0;
        int count = _buttonPathMap.Count;
        foreach (var path in _buttonPathMap.Values)
        {
            try { if (File.Exists(path)) totalSize += new FileInfo(path).Length; } catch { }
        }
        StatsChanged?.Invoke(this, new FileListStats(count, SelectedCount, totalSize));
    }

    private void FocusItem(string? path)
    {
        if (path is null) { FocusFirst(); return; }
        var btn = _buttonPathMap.FirstOrDefault(kvp => kvp.Value == path).Key;
        if (btn is not null) { _termui.SetFocus(btn); _lastFocusedPath = path; }
        else FocusFirst();
    }

    private void FocusFirst()
    {
        var first = _buttonPathMap.Keys.FirstOrDefault();
        if (first is not null)
        {
            _termui.SetFocus(first);
            _lastFocusedPath = _buttonPathMap[first];
        }
    }

    public void FocusFileList()
    {
        FocusItem(_lastFocusedPath);
    }

    public void TriggerOpenFile(string path)
    {
        OpenFileRequested?.Invoke(this, path);
    }

    private List<DirectoryInfo> SortDirectories(List<DirectoryInfo> dirs) => _sortMode switch
    {
        SortMode.NameDesc => dirs.OrderByDescending(d => d.Name).ToList(),
        SortMode.DateAsc => dirs.OrderBy(d => d.LastWriteTime).ToList(),
        SortMode.DateDesc => dirs.OrderByDescending(d => d.LastWriteTime).ToList(),
        _ => dirs.OrderBy(d => d.Name).ToList()
    };

    private List<FileInfo> SortFiles(List<FileInfo> files) => _sortMode switch
    {
        SortMode.NameDesc => files.OrderByDescending(f => f.Name).ToList(),
        SortMode.DateAsc => files.OrderBy(f => f.LastWriteTime).ToList(),
        SortMode.DateDesc => files.OrderByDescending(f => f.LastWriteTime).ToList(),
        SortMode.SizeAsc => files.OrderBy(f => f.Length).ToList(),
        SortMode.SizeDesc => files.OrderByDescending(f => f.Length).ToList(),
        _ => files.OrderBy(f => f.Name).ToList()
    };

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return $"{size:0.#} {units[i]}";
    }

    private static void CopyDirectoryRecursive(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        foreach (string dir in Directory.GetDirectories(source))
            CopyDirectoryRecursive(dir, Path.Combine(target, Path.GetFileName(dir)));
    }
}

public record FileListStats(int TotalItems, int SelectedItems, long TotalSize);

public enum ClipboardOp { None, Copy, Move }

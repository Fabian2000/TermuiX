using System.Diagnostics;
using System.Runtime.InteropServices;
using TermuiX;
using TermuiX.FileManager2.Components;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

try
{
    var termui = TermuiXLib.Init();
    var startDir = Environment.CurrentDirectory;

    // --- Per-tab state ---
    var tabStates = new List<TabState>();
    int activeTab = 0;

    // --- Components (recreated on rebuild) ---
    Toolbar toolbar = null!;
    DirectoryTree dirTree = null!;
    FileList fileList = null!;
    PreviewPanel preview = null!;
    StatusBar statusBar = null!;
    ContextMenu contextMenu = null!;
    DialogManager dialogs = null!;

    void BuildAndWireUI()
    {
        toolbar = new Toolbar(termui);
        dirTree = new DirectoryTree(termui);
        fileList = new FileList(termui);
        preview = new PreviewPanel(termui);
        statusBar = new StatusBar(termui);
        contextMenu = new ContextMenu(termui);
        dialogs = new DialogManager(termui);

        // --- Layout ---
        var layout = @"
<Container Name='appRoot' Width='100%' Height='100%' BackgroundColor='#191919'>
    <StackPanel Name='mainLayout' Direction='Vertical' Width='100%' Height='100%'>
        " + toolbar.BuildXml() + @"
        <StackPanel Name='contentRow' Direction='Horizontal' Width='100%' Height='fill'>
            " + dirTree.BuildXml() + @"
            <Line Name='treeSep' Orientation='Vertical' Type='Solid' Height='fill' BackgroundColor='#191919' ForegroundColor='#333333' />
            " + fileList.BuildXml() + @"
            " + preview.BuildXml() + @"
        </StackPanel>
        " + statusBar.BuildXml() + @"
    </StackPanel>
</Container>";

        termui.LoadXml(layout);

        // Add overlays to appRoot (floating above everything)
        var appRoot = termui.GetWidget<Container>("appRoot");
        if (appRoot is not null)
        {
            appRoot.Add(contextMenu.BuildXml());
            appRoot.Add(fileList.BuildOverlayXml());
            appRoot.Add(statusBar.BuildOverlayXml());
            appRoot.Add(dialogs.BuildXml());
        }

        // --- Initialize ---
        toolbar.Initialize();
        dirTree.Initialize();
        fileList.Initialize();
        preview.Initialize();
        statusBar.Initialize();
        contextMenu.Initialize();
        dialogs.Initialize();

        // --- Wire events BEFORE navigation ---
        WireEvents();

        // --- Restore tab state ---
        if (tabStates.Count == 0)
        {
            tabStates.Add(new TabState { Directory = startDir });
        }

        // Add all tabs
        for (int i = 0; i < tabStates.Count; i++)
            toolbar.AddTab(tabStates[i].Directory);

        // Navigate to active tab's directory
        var currentState = tabStates[activeTab];
        fileList.NavigateTo(currentState.Directory, addToHistory: false);

        // Set active tab without triggering TabSelected (which would re-navigate)
        if (activeTab > 0 && activeTab < tabStates.Count)
            toolbar.SetActiveTab(activeTab);

        // Expand tree to current working directory
        dirTree.ExpandToPath(currentState.Directory);
    }

    void WireEvents()
    {
        // TreeView → FileList
        dirTree.DirectorySelected += (_, path) =>
        {
            fileList.NavigateTo(path);
        };

        // FileList → Toolbar + StatusBar
        fileList.DirectoryChanged += (_, path) =>
        {
            toolbar.UpdatePath(path);
            toolbar.UpdateHistoryButtons(fileList.CanGoBack, fileList.CanGoForward,
                Path.GetDirectoryName(path) is not null);
            toolbar.ClearSearch();
            toolbar.UpdateTabDirectory(activeTab, path);
            dirTree.ExpandToPath(path);

            // Save to tab state
            if (activeTab < tabStates.Count)
                tabStates[activeTab].Directory = path;
        };

        fileList.StatsChanged += (_, stats) =>
        {
            statusBar.Update(stats);
        };

        // Preview toggle
        statusBar.PreviewToggled += (_, enabled) =>
        {
            if (enabled)
            {
                preview.Show();
                preview.UpdatePreview(fileList.SelectedPath);
            }
            else
            {
                preview.Hide();
            }
        };

        // FileList → ContextMenu
        fileList.ContextMenuRequested += (_, args) =>
        {
            contextMenu.Show(args.x, args.y, args.path,
                fileList.CurrentClipboardOp != ClipboardOp.None || fileList.ClipboardPaths.Count > 0);
        };

        // FileList → Empty area right-click (for new file/folder in empty dirs)
        fileList.EmptyAreaRightClicked += (_, args) =>
        {
            contextMenu.Show(args.x, args.y, fileList.CurrentDirectory,
                fileList.CurrentClipboardOp != ClipboardOp.None || fileList.ClipboardPaths.Count > 0,
                isEmptyArea: true);
        };

        // FileList → Open file
        fileList.OpenFileRequested += (_, path) =>
        {
            try
            {
                var editor = Environment.GetEnvironmentVariable("EDITOR")
                    ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "notepad" : "nano");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: GUI editor (notepad etc.) opens in its own window — no DeInit needed
                    var psi = new ProcessStartInfo(editor, path)
                    {
                        UseShellExecute = false
                    };
                    var process = Process.Start(psi);
                    process?.WaitForExit();
                }
                else
                {
                    // Unix: terminal editor (nano, vim) takes over the terminal — DeInit/ReInit required
                    if (activeTab < tabStates.Count)
                        tabStates[activeTab].Directory = fileList.CurrentDirectory;

                    TermuiXLib.DeInit();

                    var psi = new ProcessStartInfo(editor, path)
                    {
                        UseShellExecute = false
                    };
                    var process = Process.Start(psi);
                    process?.WaitForExit();

                    termui = TermuiXLib.Init();
                    BuildAndWireUI();
                }
            }
            catch { }
        };

        toolbar.CommandSubmitted += (_, text) =>
        {
            // Directory → navigate
            if (Directory.Exists(text))
            {
                fileList.NavigateTo(text);
                return;
            }

            // Exact file path without arguments → open with nano
            if (File.Exists(text) && !IsExecutable(text))
            {
                fileList.TriggerOpenFile(text);
                return;
            }

            // Everything else → run via bash (executables, commands, pipes, etc.)
            RunExternalCommand(text);
        };

        void RunExternalCommand(string commandLine)
        {
            try
            {
                if (activeTab < tabStates.Count)
                    tabStates[activeTab].Directory = fileList.CurrentDirectory;

                TermuiXLib.DeInit();

                // Run via shell so pipes, redirects, aliases etc. work
                ProcessStartInfo psi;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi = new ProcessStartInfo("cmd.exe", $"/c {commandLine}")
                    {
                        UseShellExecute = false,
                        WorkingDirectory = fileList.CurrentDirectory,
                    };
                }
                else
                {
                    var shell = Environment.GetEnvironmentVariable("SHELL") ?? "/bin/sh";
                    psi = new ProcessStartInfo(shell, $"-c \"{commandLine.Replace("\"", "\\\"")}\"")
                    {
                        UseShellExecute = false,
                        WorkingDirectory = fileList.CurrentDirectory,
                    };
                }

                var process = Process.Start(psi);
                process?.WaitForExit();
            }
            catch { }
            finally
            {
                termui = TermuiXLib.Init();
                BuildAndWireUI();
            }
        }

        static bool IsExecutable(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists) return false;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: check by extension
                    var ext = info.Extension.ToLowerInvariant();
                    return ext is ".exe" or ".cmd" or ".bat" or ".com" or ".ps1";
                }
                else
                {
                    // Unix: check execute permission via file mode
                    return (info.UnixFileMode & (UnixFileMode.UserExecute
                        | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
                }
            }
            catch
            {
                return false;
            }
        }

        toolbar.SearchChanged += (_, text) => fileList.SetFilter(string.IsNullOrWhiteSpace(text) ? null : text);
        statusBar.SortSelected += (_, mode) => fileList.SetSort(mode);

        // Tab events
        toolbar.BackRequested += (_, _) => fileList.GoBack();
        toolbar.ForwardRequested += (_, _) => fileList.GoForward();
        toolbar.UpRequested += (_, _) =>
        {
            var parent = Path.GetDirectoryName(fileList.CurrentDirectory);
            if (parent is not null)
                fileList.NavigateTo(parent);
        };

        toolbar.NewTabRequested += (_, _) =>
        {
            var dir = fileList.CurrentDirectory;
            tabStates.Add(new TabState { Directory = dir });
            toolbar.AddTab(dir);
        };

        toolbar.TabSelected += (_, index) =>
        {
            if (index == activeTab) return;

            // Save current tab state
            if (activeTab < tabStates.Count)
                tabStates[activeTab].Directory = fileList.CurrentDirectory;

            activeTab = index;

            // Load new tab state
            if (activeTab < tabStates.Count)
            {
                var state = tabStates[activeTab];
                fileList.NavigateTo(state.Directory, addToHistory: false);
            }
        };

        toolbar.TabCloseRequested += (_, index) =>
        {
            if (tabStates.Count <= 1)
            {
                // Last tab closed — exit app
                TermuiXLib.DeInit();
                Environment.Exit(0);
                return;
            }

            tabStates.RemoveAt(index);
            toolbar.RemoveTab(index);

            if (activeTab >= tabStates.Count)
                activeTab = tabStates.Count - 1;

            var state = tabStates[activeTab];
            fileList.NavigateTo(state.Directory, addToHistory: false);
        };

        // ContextMenu events
        contextMenu.OpenRequested += (_, path) =>
        {
            if (Directory.Exists(path))
                fileList.NavigateTo(path);
            else if (File.Exists(path))
                fileList.TriggerOpenFile(path);
        };

        contextMenu.CopyRequested += (_, path) =>
        {
            if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
                fileList.SetMultiSelectClipboard(ClipboardOp.Copy);
            else
                fileList.SetClipboard(ClipboardOp.Copy, path);
        };

        contextMenu.MoveRequested += (_, path) =>
        {
            if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
                fileList.SetMultiSelectClipboard(ClipboardOp.Move);
            else
                fileList.SetClipboard(ClipboardOp.Move, path);
        };

        contextMenu.PasteRequested += (_, _) =>
        {
            fileList.Paste();
        };

        contextMenu.RenameRequested += (_, path) =>
        {
            fileList.StartInlineRename(path);
        };

        contextMenu.DeleteRequested += (_, path) =>
        {
            if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
            {
                var count = fileList.CheckedItems.Count;
                dialogs.ShowConfirm($"Delete {count} selected items?", () => fileList.DeleteChecked());
            }
            else
            {
                string name = Path.GetFileName(path);
                dialogs.ShowConfirm($"Delete '{name}'?", () => fileList.PerformDelete(path));
            }
        };

        contextMenu.NewFileRequested += (_, _) =>
        {
            dialogs.ShowInputDialog("New file name:", "", name => fileList.CreateNewFile(name));
        };

        contextMenu.NewFolderRequested += (_, _) =>
        {
            dialogs.ShowInputDialog("New folder name:", "", name => fileList.CreateNewFolder(name));
        };

        contextMenu.CompressRequested += (_, args) =>
        {
            if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
            {
                fileList.CompressChecked(args.format);
            }
            else
            {
                fileList.CompressSingle(args.path, args.format);
            }
        };

        contextMenu.ExtractRequested += (_, path) =>
        {
            if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
            {
                fileList.ExtractChecked();
            }
            else
            {
                fileList.ExtractArchive(path);
            }
        };

        contextMenu.MultiSelectRequested += (_, _) =>
        {
            fileList.ToggleMultiSelect();
        };

        contextMenu.PropertiesRequested += (_, path) =>
        {
            dialogs.ShowProperties(path);
        };

        // --- Close popups on click/focus ---
        void ClosePopups()
        {
            contextMenu.Close();
            statusBar.CloseSortDropdown();
        }

        termui.MouseClick += (_, e) =>
        {
            if (e.EventType == MouseEventType.LeftButtonPressed ||
                e.EventType == MouseEventType.RightButtonPressed)
            {
                ClosePopups();
            }
        };

        termui.FocusChanged += (_, e) =>
        {
            // Track focus in file list
            fileList.OnFocusChanged(e.Widget);

            // Update preview when file selection changes
            if (statusBar.IsPreviewEnabled && fileList.SelectedPath is not null)
                preview.UpdatePreview(fileList.SelectedPath);

            // Close popups when focus moves away
            if (contextMenu.IsOpen && !contextMenu.ContainsWidget(e.Widget))
                contextMenu.Close();

            // Close inline rename when focus moves away
            if (fileList.IsRenameOpen && !fileList.IsRenameWidget(e.Widget))
                fileList.CloseRenameOverlay();

            // Dialogs close only via Cancel/Escape, not by clicking outside

            if (e.Reason == FocusChangeReason.Keyboard)
                ClosePopups();
        };

        // --- Keyboard shortcuts ---
        termui.Shortcut += (_, key) =>
        {
            if (dialogs.IsAnyDialogOpen)
            {
                if (key.Key == ConsoleKey.Escape)
                    dialogs.CloseAll();
                return;
            }

            // Don't intercept shortcuts while rename or command input is active
            if (fileList.IsRenameOpen) return;
            if (toolbar.IsCommandInputActive) return;

            if (key.Key == ConsoleKey.F)
            {
                toolbar.FocusSearch();
            }
            else if (key.Key == ConsoleKey.R)
            {
                fileList.Refresh();
            }
            else if (key.Key == ConsoleKey.N)
            {
                if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    dialogs.ShowInputDialog("New folder name:", "", name => fileList.CreateNewFolder(name));
                else
                    dialogs.ShowInputDialog("New file name:", "", name => fileList.CreateNewFile(name));
            }
            else if (key.Key == ConsoleKey.D)
            {
                if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
                {
                    var count = fileList.CheckedItems.Count;
                    dialogs.ShowConfirm($"Delete {count} selected items?", () => fileList.DeleteChecked());
                }
                else if (fileList.SelectedPath is not null)
                {
                    string name = Path.GetFileName(fileList.SelectedPath);
                    string path = fileList.SelectedPath;
                    dialogs.ShowConfirm($"Delete '{name}'?", () => fileList.PerformDelete(path));
                }
            }
            else if (key.Key == ConsoleKey.V)
            {
                fileList.Paste();
            }
            else if (key.Key == ConsoleKey.C)
            {
                if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
                    fileList.SetMultiSelectClipboard(ClipboardOp.Copy);
                else if (fileList.SelectedPath is not null)
                    fileList.SetClipboard(ClipboardOp.Copy, fileList.SelectedPath);
            }
            else if (key.Key == ConsoleKey.X)
            {
                if (fileList.IsMultiSelectMode && fileList.CheckedItems.Count > 0)
                    fileList.SetMultiSelectClipboard(ClipboardOp.Move);
                else if (fileList.SelectedPath is not null)
                    fileList.SetClipboard(ClipboardOp.Move, fileList.SelectedPath);
            }
            else if (key.Key == ConsoleKey.A)
            {
                // Select all
                fileList.SelectAll();
            }
            else if (key.Key == ConsoleKey.M)
            {
                // Toggle multi-select mode
                fileList.ToggleMultiSelect();
            }
            else if (key.Key == ConsoleKey.T)
            {
                // New tab
                var dir = fileList.CurrentDirectory;
                tabStates.Add(new TabState { Directory = dir });
                toolbar.AddTab(dir);
            }
            else if (key.Key == ConsoleKey.W)
            {
                // Close tab
                if (tabStates.Count <= 1)
                {
                    TermuiXLib.DeInit();
                    Environment.Exit(0);
                    return;
                }

                tabStates.RemoveAt(activeTab);
                toolbar.RemoveTab(activeTab);
                if (activeTab >= tabStates.Count)
                    activeTab = tabStates.Count - 1;
                var state = tabStates[activeTab];
                fileList.NavigateTo(state.Directory, addToHistory: false);
            }
            else if (key.Key == ConsoleKey.I)
            {
                // Properties
                if (fileList.SelectedPath is not null)
                    dialogs.ShowProperties(fileList.SelectedPath);
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                if (contextMenu.IsOpen) contextMenu.Close();
                else if (statusBar.IsSortDropdownOpen) statusBar.CloseSortDropdown();
                else if (fileList.IsMultiSelectMode) fileList.ToggleMultiSelect();
                else fileList.FocusFileList();
            }
        };
    }

    // --- Initial build ---
    BuildAndWireUI();

    // --- Render loop ---
    while (true)
    {
        termui.Render();
        toolbar.UpdateTabLine();
        dirTree.GuardScrollPosition(); // Detect & revert spurious scroll resets by ScrollToWidget
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}

class TabState
{
    public string Directory { get; set; } = "";
}

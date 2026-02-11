using TermuiX.FileManager.Components;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

try
{
    var termui = TermuiXLib.Init();

    var xml = @"
<Container
    Name='root'
    Width='100%'
    Height='100%'
    BackgroundColor='Black'
    ForegroundColor='White'>
</Container>";

    termui.LoadXml(xml);

    var root = termui.GetWidget<Container>("root");

    var topBar = new TopBar(termui);

    if (root is not null)
    {
        root.Add(topBar.BuildXml());
    }

    topBar.Initialize();

    var fileExplorer = new FileExplorer(termui);

    if (root is not null)
    {
        root.Add(fileExplorer.BuildXml());
    }

    fileExplorer.Initialize();

    var burgerButton = topBar.GetBurgerButton();
    var sidebar = new Sidebar(termui, burgerButton);

    if (root is not null)
    {
        root.Add(sidebar.BuildXml());
    }

    sidebar.Initialize();
    sidebar.SetFileExplorer(fileExplorer);

    // Set initial directory to first drive from sidebar
    var firstDrive = sidebar.GetFirstDrive();
    if (firstDrive is not null)
    {
        fileExplorer.NavigateToDirectory(firstDrive);
    }

    var filterButton = topBar.GetFilterButton();
    var filterPopup = new FilterPopup(termui, filterButton);

    if (root is not null)
    {
        root.Add(filterPopup.BuildXml());
    }

    filterPopup.Initialize();
    filterPopup.SetFileExplorer(fileExplorer);

    if (burgerButton is not null)
    {
        burgerButton.Click += async (sender, e) =>
        {
            await sidebar.OpenAsync();
        };
    }

    if (filterButton is not null)
    {
        filterButton.Click += (sender, e) =>
        {
            filterPopup.Open();
        };
    }

    var searchInput = topBar.GetSearchInput();
    if (searchInput is not null)
    {
        searchInput.EnterPressed += (sender, searchText) =>
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Clear filter if search is empty
                fileExplorer.ClearSearchFilter();
                return;
            }

            // Check if the input is a valid directory path
            if (Directory.Exists(searchText))
            {
                // Navigate to the directory
                fileExplorer.NavigateToDirectory(searchText);
                // Clear the search input after navigation
                searchInput.Value = "";
            }
            else
            {
                // Apply filter to show only matching files
                fileExplorer.SetSearchFilter(searchText);
            }
        };

        searchInput.TextChanged += (sender, newText) =>
        {
            if (string.IsNullOrWhiteSpace(newText))
            {
                // Clear filter when text is cleared, but don't steal focus from search input
                fileExplorer.ClearSearchFilter(focusFileList: false);
            }
        };
    }

    termui.FocusChanged += async (sender, e) =>
    {
        if (sidebar.IsOpen && !sidebar.ContainsWidget(e.Widget))
        {
            await sidebar.CloseAsync();
        }

        if (filterPopup.IsOpen && !filterPopup.ContainsWidget(e.Widget))
        {
            filterPopup.Close();
        }
    };

    termui.Shortcut += async (sender, key) =>
    {
        if (key.Key == ConsoleKey.B)
        {
            if (sidebar.IsOpen)
            {
                var closeButton = sidebar.GetCloseButton();
                if (closeButton is not null)
                {
                    termui.SetFocus(closeButton);
                }
            }
            else
            {
                if (burgerButton is not null)
                {
                    termui.SetFocus(burgerButton);
                }
            }
        }
        else if (key.Key == ConsoleKey.F)
        {
            if (sidebar.IsOpen)
            {
                var searchDriveInput = sidebar.GetSearchDriveInput();
                if (searchDriveInput is not null)
                {
                    termui.SetFocus(searchDriveInput);
                }
            }
            else
            {
                var searchInput = topBar.GetSearchInput();
                if (searchInput is not null)
                {
                    termui.SetFocus(searchInput);
                }
            }
        }
        else if (key.Key == ConsoleKey.E)
        {
            var filterButton = topBar.GetFilterButton();
            if (filterButton is not null)
            {
                termui.SetFocus(filterButton);
            }
        }
        else if (key.Key == ConsoleKey.P)
        {
            // Select the current item first (if in explorer)
            if (fileExplorer.IsFocusedInExplorer())
            {
                fileExplorer.SelectCurrentItem();
            }

            // Then jump to copy button
            var copyButton = fileExplorer.GetCopyButton();
            if (copyButton is not null)
            {
                termui.SetFocus(copyButton);
            }
        }
        else if (key.Key == ConsoleKey.R)
        {
            // If focus is in explorer, refresh the directory
            // Otherwise, jump to explorer
            if (fileExplorer.IsFocusedInExplorer())
            {
                fileExplorer.Refresh();
            }
            else
            {
                fileExplorer.FocusLastFileButton();
            }
        }
        else if (key.Key == ConsoleKey.U)
        {
            fileExplorer.GoBack();
        }
        else if (key.Key == ConsoleKey.Y)
        {
            fileExplorer.GoForward();
        }
        else if (key.Key == ConsoleKey.M)
        {
            topBar.ToggleMouse();
        }
        else if (key.Key == ConsoleKey.Escape)
        {
            // Close whatever is open: sidebar, filter popup, context menu, rename overlay
            if (sidebar.IsOpen)
            {
                await sidebar.CloseAsync();
            }
            else if (filterPopup.IsOpen)
            {
                filterPopup.Close();
            }
            else if (fileExplorer.IsContextMenuOpen)
            {
                fileExplorer.CloseContextMenu();
            }
            else if (fileExplorer.IsRenameOpen)
            {
                fileExplorer.CloseRenameOverlay();
            }
            else
            {
                // Nothing open — jump focus back to file explorer
                fileExplorer.FocusLastFileButton();
            }
        }
    };

    int lastWidth = Console.WindowWidth;
    int lastHeight = Console.WindowHeight;

    // FPS tracking
    int frameCount = 0;
    long lastFpsTick = Environment.TickCount64;

    while (true)
    {
        int currentWidth = Console.WindowWidth;
        int currentHeight = Console.WindowHeight;

        if (currentWidth != lastWidth || currentHeight != lastHeight)
        {
            topBar.UpdateLayout();
            sidebar.UpdateLayout();
            filterPopup.UpdateLayout();
            fileExplorer.UpdateLayout();

            lastWidth = currentWidth;
            lastHeight = currentHeight;
        }

        // Update FPS every second
        frameCount++;
        long now = Environment.TickCount64;
        if (now - lastFpsTick >= 1000)
        {
            topBar.UpdateFps(frameCount);
            frameCount = 0;
            lastFpsTick = now;
        }

        termui.Render();
        await Task.Delay(16);
    }
}
catch (Exception ex)
{
    TermuiXLib.DeInit();
    throw;
}
finally
{
    TermuiXLib.DeInit();
}

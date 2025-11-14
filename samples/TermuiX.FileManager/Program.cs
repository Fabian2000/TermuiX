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

    var filterButton = topBar.GetFilterButton();
    var filterPopup = new FilterPopup(termui, filterButton);

    if (root is not null)
    {
        root.Add(filterPopup.BuildXml());
    }

    filterPopup.Initialize();

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

    termui.FocusChanged += async (sender, widget) =>
    {
        if (sidebar.IsOpen && !sidebar.ContainsWidget(widget))
        {
            await sidebar.CloseAsync();
        }

        if (filterPopup.IsOpen && !filterPopup.ContainsWidget(widget))
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
            var copyButton = fileExplorer.GetCopyButton();
            if (copyButton is not null)
            {
                termui.SetFocus(copyButton);
            }
        }
    };

    int lastWidth = Console.WindowWidth;
    int lastHeight = Console.WindowHeight;

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

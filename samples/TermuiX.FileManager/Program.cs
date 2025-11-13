using TermuiX.FileManager.Components;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

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

var sidebar = new Sidebar(termui);
var topBar = new TopBar(termui);

if (root is not null)
{
    root.Add(topBar.BuildXml());
    root.Add(sidebar.BuildXml());
}

topBar.Initialize();
sidebar.Initialize();

var burgerButton = topBar.GetBurgerButton();
if (burgerButton is not null)
{
    burgerButton.Click += async (sender, e) =>
    {
        await sidebar.OpenAsync();
    };
}

termui.Shortcut += (sender, key) =>
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
        var searchInput = topBar.GetSearchInput();
        if (searchInput is not null)
        {
            termui.SetFocus(searchInput);
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
};

try
{
    int lastWidth = Console.WindowWidth;

    while (true)
    {
        int currentWidth = Console.WindowWidth;
        if (currentWidth != lastWidth)
        {
            topBar.UpdateLayout();
            sidebar.UpdateLayout();
            lastWidth = currentWidth;
        }

        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}

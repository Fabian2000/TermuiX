# Examples

Complete working examples for common use cases. Each example is a standalone program that can be run directly.

## Minimal Hello World

The simplest TermuiX application:

```csharp
using TermuiXLib = TermuiX.TermuiX;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <Container Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
        <Text PositionX='2ch' PositionY='1ch' ForegroundColor='#cdd6f4'>
            Hello, TermuiX! Press Ctrl+C to exit.
        </Text>
    </Container>");

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## Form with Input and Button

A simple form with text input, submit button, and status display:

```csharp
using TermuiX;
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%'
        BackgroundColor='#1e1e2e' PaddingLeft='2ch' PaddingTop='1ch'>

        <Text ForegroundColor='#cdd6f4' Style='Bold'>Registration Form</Text>

        <Text ForegroundColor='#a6adc8' MarginTop='1ch'>Name:</Text>
        <Container Width='40ch' Height='3ch' MarginTop='0ch'
            BorderStyle='Single' RoundedCorners='true'
            BackgroundColor='#313244' BorderColor='#45475a' FocusBorderColor='#89b4fa'>
            <Input Name='nameInput' Width='100%' Height='1ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                ForegroundColor='#cdd6f4' CursorColor='#cdd6f4'
                Placeholder='Enter your name...' />
        </Container>

        <Text ForegroundColor='#a6adc8' MarginTop='1ch'>Email:</Text>
        <Container Width='40ch' Height='3ch' MarginTop='0ch'
            BorderStyle='Single' RoundedCorners='true'
            BackgroundColor='#313244' BorderColor='#45475a' FocusBorderColor='#89b4fa'>
            <Input Name='emailInput' Width='100%' Height='1ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                ForegroundColor='#cdd6f4' CursorColor='#cdd6f4'
                Placeholder='Enter your email...' />
        </Container>

        <StackPanel Direction='Horizontal' MarginTop='1ch'>
            <Button Name='submitBtn' Width='12ch'
                BackgroundColor='#89b4fa' FocusBackgroundColor='#74a0e8'
                TextColor='#1e1e2e' FocusTextColor='#1e1e2e'
                BorderStyle='Single' RoundedCorners='true'>Submit</Button>
            <Button Name='clearBtn' Width='12ch' MarginLeft='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#cdd6f4' FocusTextColor='#ffffff'
                BorderColor='#45475a' FocusBorderColor='#89b4fa'
                BorderStyle='Single' RoundedCorners='true'>Clear</Button>
        </StackPanel>

        <Text Name='status' ForegroundColor='#a6adc8' MarginTop='1ch' />
    </StackPanel>");

    var nameInput = termui.GetWidget<Input>("nameInput");
    var emailInput = termui.GetWidget<Input>("emailInput");
    var status = termui.GetWidget<Text>("status");

    var submitBtn = termui.GetWidget<Button>("submitBtn");
    submitBtn!.Click += (_, _) =>
    {
        status!.Content = $"Submitted: {nameInput!.Value} ({emailInput!.Value})";
        status.ForegroundColor = Color.Parse("#a6e3a1");
    };

    var clearBtn = termui.GetWidget<Button>("clearBtn");
    clearBtn!.Click += (_, _) =>
    {
        nameInput!.Value = "";
        emailInput!.Value = "";
        status!.Content = "Form cleared.";
        status.ForegroundColor = Color.Parse("#a6adc8");
    };

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## Header-Sidebar-Content Layout

Classic application layout with a fixed header, sidebar, and fill content area:

```csharp
using TermuiXLib = TermuiX.TermuiX;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%' BackgroundColor='#1e1e2e'>

        <!-- Header -->
        <StackPanel Direction='Horizontal' Width='100%' Height='3ch'
            BackgroundColor='#181825' Align='Center' PaddingLeft='1ch' PaddingRight='1ch'>
            <Text ForegroundColor='#89b4fa' Style='Bold'>My App</Text>
            <Text Width='fill' />
            <Button Name='menuBtn' Width='8ch'
                BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                TextColor='#a6adc8' FocusTextColor='#ffffff'
                BorderColor='#45475a' FocusBorderColor='#89b4fa'
                BorderStyle='Single' RoundedCorners='true'>Menu</Button>
        </StackPanel>

        <Line Orientation='Horizontal' Type='Solid' Width='100%'
            ForegroundColor='#313244' BackgroundColor='#1e1e2e' />

        <!-- Body -->
        <StackPanel Direction='Horizontal' Width='100%' Height='fill'>

            <!-- Sidebar -->
            <StackPanel Direction='Vertical' Width='25ch' Height='100%'
                BackgroundColor='#181825' PaddingLeft='1ch' PaddingTop='1ch'>
                <Text ForegroundColor='#6c7086' Style='Bold'>Navigation</Text>
                <Button Name='nav1' Width='23ch' MarginTop='1ch'
                    BackgroundColor='Inherit' FocusBackgroundColor='#313244'
                    TextColor='#cdd6f4' FocusTextColor='#ffffff'
                    BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'>Dashboard</Button>
                <Button Name='nav2' Width='23ch'
                    BackgroundColor='Inherit' FocusBackgroundColor='#313244'
                    TextColor='#cdd6f4' FocusTextColor='#ffffff'
                    BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'>Settings</Button>
                <Button Name='nav3' Width='23ch'
                    BackgroundColor='Inherit' FocusBackgroundColor='#313244'
                    TextColor='#cdd6f4' FocusTextColor='#ffffff'
                    BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'>About</Button>
            </StackPanel>

            <Line Orientation='Vertical' Type='Solid' Height='100%'
                ForegroundColor='#313244' BackgroundColor='#1e1e2e' />

            <!-- Content -->
            <Container Width='fill' Height='100%' ScrollY='true'
                BackgroundColor='#1e1e2e' PaddingLeft='2ch' PaddingTop='1ch'>
                <Text Name='content' Width='100%' ForegroundColor='#cdd6f4'>
                    Welcome to the Dashboard.
                </Text>
            </Container>
        </StackPanel>

        <!-- Footer -->
        <Line Orientation='Horizontal' Type='Solid' Width='100%'
            ForegroundColor='#313244' BackgroundColor='#1e1e2e' />
        <StackPanel Direction='Horizontal' Width='100%' Height='1ch'
            BackgroundColor='#181825' PaddingLeft='1ch'>
            <Text ForegroundColor='#6c7086'>Ready</Text>
        </StackPanel>
    </StackPanel>");

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## Interactive List with Context Menu

A scrollable list of items with click selection and a right-click context menu:

```csharp
using TermuiX;
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;
using System.Security;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <Container Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
        <StackPanel Direction='Vertical' Width='100%' Height='100%'>
            <Text Width='100%' Height='1ch' PaddingLeft='1ch'
                ForegroundColor='#89b4fa' Style='Bold' BackgroundColor='#181825'>
                Items
            </Text>
            <Container Name='listScroll' Width='100%' Height='fill' ScrollY='true'>
                <StackPanel Name='itemList' Direction='Vertical' Width='100%' Height='auto' />
            </Container>
            <Text Name='status' Width='100%' Height='1ch' PaddingLeft='1ch'
                ForegroundColor='#6c7086' BackgroundColor='#181825'>
                0 items
            </Text>
        </StackPanel>

        <!-- Context Menu (overlay, hidden) -->
        <Container Name='ctxMenu' Width='16ch' Height='5ch'
            PositionX='0ch' PositionY='0ch'
            BackgroundColor='#313244' BorderStyle='Single' RoundedCorners='true'
            Visible='false'>
            <Button Name='ctxOpen' Width='14ch' Height='1ch' PositionX='0ch' PositionY='0ch'
                BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#cdd6f4' FocusTextColor='#ffffff'
                PaddingTop='0ch' PaddingBottom='0ch'>Open</Button>
            <Button Name='ctxEdit' Width='14ch' Height='1ch' PositionX='0ch' PositionY='1ch'
                BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#cdd6f4' FocusTextColor='#ffffff'
                PaddingTop='0ch' PaddingBottom='0ch'>Edit</Button>
            <Button Name='ctxDelete' Width='14ch' Height='1ch' PositionX='0ch' PositionY='2ch'
                BorderStyle='None' TextAlign='Left' PaddingLeft='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#f38ba8' FocusTextColor='#f38ba8'
                PaddingTop='0ch' PaddingBottom='0ch'>Delete</Button>
        </Container>
    </Container>");

    var itemList = termui.GetWidget<StackPanel>("itemList");
    var status = termui.GetWidget<Text>("status");
    var ctxMenu = termui.GetWidget<Container>("ctxMenu");
    string? selectedItem = null;

    // Populate list
    var items = new[] { "Document.pdf", "Image.png", "Notes.txt", "Project.zip",
                        "Report.docx", "Data.csv", "Script.sh", "Config.yaml" };

    for (int i = 0; i < items.Length; i++)
    {
        var name = items[i];
        var escaped = SecurityElement.Escape(name);
        itemList!.Add($@"
            <Button Name='item_{i}' Width='100%' Height='1ch'
                BorderStyle='None' TextAlign='Left' PaddingLeft='2ch'
                BackgroundColor='Inherit' FocusBackgroundColor='#313244'
                TextColor='#cdd6f4' FocusTextColor='#ffffff'
                PaddingTop='0ch' PaddingBottom='0ch'>{escaped}</Button>");

        var btn = termui.GetWidget<Button>($"item_{i}");
        var itemName = name;
        btn!.Click += (_, _) =>
        {
            selectedItem = itemName;
            status!.Content = $"Selected: {itemName}";
        };
        btn.RightClick += (_, args) =>
        {
            selectedItem = itemName;
            ctxMenu!.PositionX = $"{args.X}ch";
            ctxMenu.PositionY = $"{args.Y}ch";
            ctxMenu.Visible = true;
            termui.SetFocus(termui.GetWidget<Button>("ctxOpen")!);
        };
    }
    status!.Content = $"{items.Length} items";

    // Close context menu on click outside or Escape
    termui.MouseClick += (_, args) =>
    {
        if (args.EventType == MouseEventType.LeftButtonPressed)
        {
            ctxMenu!.Visible = false;
        }
    };
    termui.Shortcut += (_, key) =>
    {
        if (key.Key == ConsoleKey.Escape)
        {
            ctxMenu!.Visible = false;
        }
    };

    // Context menu actions
    termui.GetWidget<Button>("ctxDelete")!.Click += (_, _) =>
    {
        status!.Content = $"Deleted: {selectedItem}";
        status.ForegroundColor = Color.Parse("#f38ba8");
        ctxMenu!.Visible = false;
    };

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## TreeView with Lazy Loading

A directory browser using TreeView with on-demand loading:

```csharp
using TermuiX;
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Horizontal' Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
        <Container Name='treeScroll' Width='35ch' Height='100%' ScrollY='true'
            BackgroundColor='#181825'>
            <TreeView Name='dirTree' Width='35ch' Height='auto'
                BackgroundColor='#181825' ForegroundColor='#cdd6f4'
                HighlightBackgroundColor='#313244' HighlightForegroundColor='#cdd6f4' />
        </Container>
        <Line Orientation='Vertical' Type='Solid' Height='100%'
            ForegroundColor='#313244' BackgroundColor='#1e1e2e' />
        <Text Name='info' Width='fill' PaddingLeft='2ch' PaddingTop='1ch'
            ForegroundColor='#a6adc8'>Select a directory</Text>
    </StackPanel>");

    var tree = termui.GetWidget<TreeView>("dirTree");
    var info = termui.GetWidget<Text>("info");

    tree!.FocusBackgroundColor = Color.Parse("#181825");
    tree.FocusForegroundColor = Color.Parse("#cdd6f4");

    // Populate root
    var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var homeNode = tree.Root.AddChild($"Home ({Path.GetFileName(homePath)})");
    homeNode.Tag = homePath;
    LoadChildren(homeNode);
    homeNode.IsExpanded = true;

    // Lazy load on expand
    tree.NodeToggled += (_, node) =>
    {
        if (node.IsExpanded && node.Children.Count == 1
            && node.Children[0].Tag as string == "__placeholder__")
        {
            LoadChildren(node);
        }
    };

    // Show info on select
    tree.NodeSelected += (_, node) =>
    {
        if (node.Tag is string path && path != "__placeholder__")
        {
            try
            {
                var dirs = Directory.GetDirectories(path).Length;
                var files = Directory.GetFiles(path).Length;
                info!.Content = $"Path: {path}\nDirectories: {dirs}\nFiles: {files}";
            }
            catch
            {
                info!.Content = $"Path: {path}\nAccess denied";
            }
        }
    };

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}

void LoadChildren(TreeNode parent)
{
    var path = parent.Tag as string;
    if (path is null) return;

    while (parent.Children.Count > 0)
        parent.RemoveChild(parent.Children[0]);

    try
    {
        foreach (var dir in Directory.GetDirectories(path)
            .Select(d => new DirectoryInfo(d))
            .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
            .OrderBy(d => d.Name))
        {
            var node = parent.AddChild(dir.Name);
            node.Tag = dir.FullName;

            try
            {
                if (Directory.GetDirectories(dir.FullName).Length > 0)
                {
                    var placeholder = node.AddChild("...");
                    placeholder.Tag = "__placeholder__";
                }
            }
            catch { /* permission denied */ }
        }
    }
    catch
    {
        var err = parent.AddChild("Access denied");
        err.Tag = null;
    }
}
```

## Progress Bar with Async Work

Simulating a background task with progress updates:

```csharp
using TermuiX;
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%'
        BackgroundColor='#1e1e2e' PaddingLeft='2ch' PaddingTop='2ch'>
        <Text ForegroundColor='#cdd6f4' Style='Bold'>File Processing</Text>
        <Text Name='taskLabel' ForegroundColor='#a6adc8' MarginTop='1ch'>
            Ready to start.
        </Text>
        <ProgressBar Name='progress' Width='50ch' MarginTop='1ch'
            ForegroundColor='#89b4fa' BackgroundColor='#313244' />
        <Button Name='startBtn' Width='12ch' MarginTop='1ch'
            BackgroundColor='#89b4fa' FocusBackgroundColor='#74a0e8'
            TextColor='#1e1e2e' FocusTextColor='#1e1e2e'
            BorderStyle='Single' RoundedCorners='true'>Start</Button>
    </StackPanel>");

    var progress = termui.GetWidget<ProgressBar>("progress");
    var taskLabel = termui.GetWidget<Text>("taskLabel");
    var startBtn = termui.GetWidget<Button>("startBtn");
    bool running = false;

    startBtn!.Click += async (_, _) =>
    {
        if (running) return;
        running = true;
        startBtn.Disabled = true;

        for (int i = 0; i <= 100; i += 5)
        {
            progress!.Value = i / 100.0;
            taskLabel!.Content = $"Processing... {i}%";
            await Task.Delay(100);
        }

        taskLabel!.Content = "Done!";
        taskLabel.ForegroundColor = Color.Parse("#a6e3a1");
        startBtn.Disabled = false;
        running = false;
    };

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## Checkbox and RadioButton Form

Settings panel with checkboxes and radio buttons:

```csharp
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%'
        BackgroundColor='#1e1e2e' PaddingLeft='2ch' PaddingTop='1ch'>

        <Text ForegroundColor='#89b4fa' Style='Bold'>Settings</Text>

        <Text ForegroundColor='#a6adc8' MarginTop='1ch'>Options:</Text>

        <StackPanel Direction='Horizontal' Align='Center' MarginTop='0ch'>
            <Checkbox Name='cbNotify' Checked='true'
                ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
            <Text ForegroundColor='#cdd6f4' MarginLeft='1ch'>Enable notifications</Text>
        </StackPanel>

        <StackPanel Direction='Horizontal' Align='Center'>
            <Checkbox Name='cbAutoSave'
                ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
            <Text ForegroundColor='#cdd6f4' MarginLeft='1ch'>Auto-save on exit</Text>
        </StackPanel>

        <Text ForegroundColor='#a6adc8' MarginTop='1ch'>Theme:</Text>

        <!-- RadioButtons must be direct siblings for mutual exclusion -->
        <StackPanel Direction='Horizontal' MarginTop='0ch'>
            <StackPanel Direction='Vertical'>
                <RadioButton Name='rbDark' Checked='true'
                    ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
                <RadioButton Name='rbLight'
                    ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
                <RadioButton Name='rbSystem'
                    ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
            </StackPanel>
            <StackPanel Direction='Vertical' MarginLeft='1ch'>
                <Text Height='1ch' ForegroundColor='#cdd6f4'>Dark</Text>
                <Text Height='1ch' ForegroundColor='#cdd6f4'>Light</Text>
                <Text Height='1ch' ForegroundColor='#cdd6f4'>System</Text>
            </StackPanel>
        </StackPanel>

        <Text Name='result' ForegroundColor='#6c7086' MarginTop='2ch' />
    </StackPanel>");

    var cbNotify = termui.GetWidget<Checkbox>("cbNotify");
    var cbAutoSave = termui.GetWidget<Checkbox>("cbAutoSave");
    var rbDark = termui.GetWidget<RadioButton>("rbDark");
    var rbLight = termui.GetWidget<RadioButton>("rbLight");
    var rbSystem = termui.GetWidget<RadioButton>("rbSystem");
    var result = termui.GetWidget<Text>("result");

    void UpdateResult()
    {
        var theme = rbDark!.Selected ? "Dark" : rbLight!.Selected ? "Light" : "System";
        result!.Content = $"Notifications: {cbNotify!.Checked}, AutoSave: {cbAutoSave!.Checked}, Theme: {theme}";
    }

    cbNotify!.CheckedChanged += (_, _) => UpdateResult();
    cbAutoSave!.CheckedChanged += (_, _) => UpdateResult();
    rbDark!.SelectionChanged += (_, _) => UpdateResult();
    rbLight!.SelectionChanged += (_, _) => UpdateResult();
    rbSystem!.SelectionChanged += (_, _) => UpdateResult();

    UpdateResult();

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

## Keyboard Shortcuts

Handling shortcuts and focus management:

```csharp
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
TermuiXLib.AllowCancelKeyExit = false;
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
        <StackPanel Direction='Horizontal' Width='100%' Height='3ch'
            BackgroundColor='#181825' Align='Center' PaddingLeft='1ch' PaddingRight='1ch'>
            <Text ForegroundColor='#89b4fa' Style='Bold'>Editor</Text>
            <Text Width='fill' />
            <Container Name='searchBox' Width='30ch' Height='3ch'
                BorderStyle='Single' RoundedCorners='true'
                BackgroundColor='#313244' BorderColor='#45475a' FocusBorderColor='#89b4fa'>
                <Input Name='searchInput' Width='100%' Height='1ch'
                    BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
                    ForegroundColor='#cdd6f4' CursorColor='#cdd6f4'
                    Placeholder='Search (Ctrl+F)...' />
            </Container>
        </StackPanel>
        <Text Name='content' Width='100%' Height='fill' PaddingLeft='2ch' PaddingTop='1ch'
            ForegroundColor='#cdd6f4'>
            Press Ctrl+F to focus search.\nPress Ctrl+Q to quit.\nPress Escape to clear.
        </Text>
        <Text Name='statusLine' Width='100%' Height='1ch' PaddingLeft='1ch'
            ForegroundColor='#6c7086' BackgroundColor='#181825'>
            Ctrl+F: Search | Ctrl+Q: Quit | Escape: Clear
        </Text>
    </StackPanel>");

    var searchInput = termui.GetWidget<Input>("searchInput");
    var content = termui.GetWidget<Text>("content");
    bool shouldExit = false;

    termui.Shortcut += (_, key) =>
    {
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            switch (key.Key)
            {
                case ConsoleKey.F:
                    termui.SetFocus(searchInput!);
                    break;
                case ConsoleKey.Q:
                    shouldExit = true;
                    break;
            }
        }
        if (key.Key == ConsoleKey.Escape)
        {
            searchInput!.Value = "";
        }
    };

    searchInput!.TextChanged += (_, text) =>
    {
        content!.Content = string.IsNullOrEmpty(text)
            ? "Press Ctrl+F to focus search.\nPress Ctrl+Q to quit.\nPress Escape to clear."
            : $"Searching for: {text}";
    };

    while (!shouldExit)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}
```

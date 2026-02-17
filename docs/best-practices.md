# Best Practices and Known Behaviors

Layout behaviors that may be surprising, common patterns, and recommended approaches.

## Known Behaviors

### Input Has No Visible Border

Input does not render borders. The `BorderColor` and `FocusBorderColor` properties are deprecated and might be removed in a future version. To get a visible border, wrap Input in a bordered Container.

```xml
<Container Width='32ch' Height='3ch'
    BorderStyle='Single' RoundedCorners='true'
    BackgroundColor='#1e1e2e'
    BorderColor='#808080' FocusBorderColor='#ffffff'>
    <Input Width='100%' Height='1ch'
        BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
        ForegroundColor='#cdd6f4' CursorColor='#cdd6f4'
        Placeholder='Type here...' />
</Container>
```

Set Input Height to `"1ch"`, Container Height to `"3ch"` (1 border + 1 text + 1 border). Use `BackgroundColor='Inherit'` on Input.

### StackPanel Defaults to Auto Size

StackPanel defaults Width and Height to `"auto"`, not `"100%"`. Without explicit size, it shrinks to fit its children. If you expect it to fill its parent, set Width and/or Height explicitly.

```xml
<!-- Only as wide as its children -->
<StackPanel Direction='Horizontal'>
    <Button>A</Button>
    <Button>B</Button>
</StackPanel>

<!-- Fills the parent width -->
<StackPanel Direction='Horizontal' Width='100%'>
    <Button>A</Button>
    <Button>B</Button>
</StackPanel>
```

### Border Adds 1ch Per Side

A Container with `BorderStyle='Single'` consumes 1 character on each side. A `Width='20ch'` Container with border has 18ch of content width, and `Height - 2` content height.

Padding is applied inside the border: `PaddingLeft='1ch'` + border = 2ch from the left edge to content.

### Table Size Is Read-Only

The Table widget auto-calculates Width and Height from its content. Setting Width or Height has no effect. Column widths are determined by the widest cell in each column.

### Chart Properties Are Code-Only

Chart has no widget-specific XML attributes. Series, MinY, MaxY, XLabels, ShowLegend, ShowAxes, and YAxisWidth must be set in code.

### TreeView Highlight Colors

TreeView supports HighlightBackgroundColor and HighlightForegroundColor in XML to style the selected node:

```xml
<TreeView HighlightBackgroundColor='#313244' HighlightForegroundColor='#ffffff'
    FocusBackgroundColor='#1e1e2e' FocusForegroundColor='#cdd6f4'>
```

Without this, the TreeView uses default ConsoleColor.Gray highlight.

## Common Patterns

### Overlay/Popup

Use a Container with absolute positioning, initially hidden:

```xml
<Container Name='root' Width='100%' Height='100%'>
    <!-- Main content -->
    <StackPanel Width='100%' Height='100%'>
        <!-- ... -->
    </StackPanel>

    <!-- Overlay -->
    <Container Name='popup' Width='30ch' Height='10ch'
        PositionX='10ch' PositionY='5ch'
        BackgroundColor='#313244' BorderStyle='Single' RoundedCorners='true'
        Visible='false'>
        <Text PositionX='1ch' PositionY='1ch' ForegroundColor='#cdd6f4'>
            Popup content
        </Text>
    </Container>
</Container>
```

Show/hide by toggling `popup.Visible`. Position dynamically with `popup.PositionX` and `popup.PositionY`.

### ClosePopups

Close popups when the user clicks elsewhere or presses Escape:

```csharp
void ClosePopups()
{
    if (popup is not null) { popup.Visible = false; }
    if (dropdown is not null) { dropdown.Visible = false; }
}

termui.MouseClick += (_, args) =>
{
    if (args.EventType == MouseEventType.LeftButtonPressed)
    {
        ClosePopups();
    }
};

termui.Shortcut += (_, key) =>
{
    if (key.Key == ConsoleKey.Escape)
    {
        ClosePopups();
    }
};
```

### Reusable Component

For reusable multi-widget components, use the BuildXml + Initialize pattern:

```csharp
public class Sidebar
{
    private readonly TermuiX _termui;
    private TreeView? _tree;

    public Sidebar(TermuiX termui) => _termui = termui;

    public string BuildXml()
    {
        return @"
        <StackPanel Name='sidebar' Direction='Vertical' Width='25ch' Height='100%'>
            <Text Style='Bold' PaddingLeft='1ch'>Explorer</Text>
            <Container Name='sidebarScroll' Width='25ch' Height='fill' ScrollY='true'>
                <TreeView Name='sidebarTree' Width='25ch' Height='auto' />
            </Container>
        </StackPanel>";
    }

    public void Initialize()
    {
        _tree = _termui.GetWidget<TreeView>("sidebarTree");
        // Set colors, bind events, populate data...
    }
}
```

Register and use:

```csharp
var sidebar = new Sidebar(termui);
termui.RegisterComponent("Sidebar", attrs => sidebar.BuildXml());
termui.LoadXml(@"
<StackPanel Direction='Horizontal' Width='100%' Height='100%'>
    <Sidebar />
    <Container Width='fill' Height='100%'>...</Container>
</StackPanel>");
sidebar.Initialize();
```

### Launching External Programs

To launch an external program that needs the terminal, you must temporarily leave TUI mode:

```csharp
// 1. Leave TUI mode so the external program can use the terminal
TermuiXLib.DeInit();

// 2. Run the external program
var process = Process.Start(new ProcessStartInfo("programName", arguments)
{
    UseShellExecute = false
});
process?.WaitForExit();

// 3. Re-enter TUI mode
termui = TermuiXLib.Init();
// Re-load XML, re-initialize components, restore state
```

`DeInit()` restores the terminal to normal mode so the external program can use it. After it exits, `Init()` re-enters TUI mode. You need to rebuild the entire UI state.

### Tab System

Use buttons as tabs and swap content on selection:

```csharp
public void AddTab(string title)
{
    tabBar.Add($@"
        <Button Name='tab_{id}' Width='20ch'
            BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
            TextColor='#808080' FocusTextColor='#ffffff'
            BorderStyle='Single' RoundedCorners='true'>{escaped}</Button>");

    var btn = termui.GetWidget<Button>($"tab_{id}");
    btn.Click += (_, _) => SwitchToTab(index);
}

public void SwitchToTab(int index)
{
    // Update tab button colors (active vs inactive)
    // Swap content in the main panel
}
```

### Dynamic List

For scrollable lists of items, use a StackPanel with dynamically added buttons:

```csharp
var listPanel = termui.GetWidget<StackPanel>("fileList");
listPanel.Clear();

foreach (var file in files)
{
    var escaped = SecurityElement.Escape(file.Name);
    listPanel.Add($@"
        <Button Name='file_{file.Id}' Width='100%' Height='1ch'
            BorderStyle='None' TextAlign='Left'
            BackgroundColor='Inherit' FocusBackgroundColor='#313244'
            TextColor='#cdd6f4' FocusTextColor='#ffffff'
            PaddingLeft='1ch'>{escaped}</Button>");

    var btn = termui.GetWidget<Button>($"file_{file.Id}");
    btn.Click += (_, _) => OnFileClicked(file);
    btn.RightClick += (_, args) => ShowContextMenu(file, args.X, args.Y);
}
```

## Tips

### Render Loop

Use 16ms delay for responsive interaction. Use higher values (50-100ms) if CPU usage is a concern.

```csharp
while (true)
{
    termui.Render();
    await Task.Delay(16);
}
```

### skipUnchanged for Slow Terminals

For SSH or slow connections:

```csharp
termui.Render(skipUnchanged: true);
```

Skips the console write if the output is identical to the previous frame. Input is still processed.

### Minimize Dynamic Widget Creation

`Add(string xml)` parses XML on every call. For frequently updated lists, consider reusing existing widgets and changing their properties instead of removing and re-creating.

### Scroll Container Sizing

Place scrollable content in a Container with explicit size, not `"auto"`. Auto-sized scrollable containers expand to fit content, defeating the purpose of scrolling.

```xml
<!-- BAD: Auto height = no scrolling -->
<Container Height='auto' ScrollY='true'>
    <StackPanel Direction='Vertical' Height='auto'>...</StackPanel>
</Container>

<!-- GOOD: Fill or fixed height = actual scrolling -->
<Container Height='fill' ScrollY='true'>
    <StackPanel Direction='Vertical' Height='auto'>...</StackPanel>
</Container>
```

### Debugging Layout

After a `Render()` call, check `ComputedWidth` and `ComputedHeight` to verify layout:

```csharp
var widget = termui.GetWidget<Container>("myContainer");
Console.Title = $"W={((IWidget)widget).ComputedWidth} H={((IWidget)widget).ComputedHeight}";
```

### Widget Not Rendering?

Check:
1. `Visible` is true on the widget and all ancestors. An invisible parent hides all children.
2. Width and Height are not `"0ch"`.
3. The widget is in the widget tree (added to a parent that is part of the root tree).

### Tab Not Reaching a Widget?

Check:
1. `CanFocus` is true (default for Button, Input, Checkbox, RadioButton, Slider, TreeView).
2. `Disabled` is false.
3. `Visible` is true on the widget and all ancestors.
4. The widget is in the widget tree.

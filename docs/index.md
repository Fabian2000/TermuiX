# TermuiX Documentation

TermuiX is a declarative terminal UI library for .NET. Define your interface in XML, style with TrueColor, and handle mouse and keyboard input.

## Table of Contents

- [index.md](index.md) - This file. Getting started, API reference, common properties.
- [layout.md](layout.md) - Layout system: Container, StackPanel, sizing, margins, padding.
- [widgets.md](widgets.md) - Complete widget reference with all properties, events, and examples.
- [styling.md](styling.md) - Colors, borders, focus states, text styles, Style element.
- [events.md](events.md) - Mouse input, keyboard input, focus, shortcuts.
- [xml-reference.md](xml-reference.md) - XML syntax, attributes, dynamic Add/Remove, custom widgets.
- [best-practices.md](best-practices.md) - Known behaviors, common patterns, tips.
- [examples.md](examples.md) - Complete working examples.

## Getting Started

A TermuiX application follows this lifecycle:

1. `Init()` - Initialize the terminal for TUI rendering.
2. `LoadXml()` - Load a widget tree from an XML string.
3. `Render()` in a loop - Process input and draw the UI every frame.
4. `DeInit()` - Restore the terminal to its normal state.

```csharp
using TermuiXLib = TermuiX.TermuiX;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <Container Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
        <Text PositionX='2ch' PositionY='1ch' ForegroundColor='#cdd6f4'>Hello, TermuiX!</Text>
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

The recommended frame delay is 16ms (~60fps). Higher delays reduce CPU usage but make input feel sluggish.

## Widgets

All built-in widget types:

| Widget | Description | Focusable |
|--------|-------------|-----------|
| Container | Base container, absolute positioning of children | No |
| StackPanel | Directional layout with justify, align, wrap (extends Container) | No |
| Button | Clickable button with border, text, focus states | Yes |
| Text | Static text display with markdown support | No |
| Input | Text input with cursor, placeholder, events | Yes |
| Checkbox | Toggle checkbox (checked/unchecked) | Yes |
| RadioButton | Single-selection radio button | Yes |
| Slider | Numeric range slider with keyboard and mouse | Yes |
| TreeView | Hierarchical tree with expand/collapse | Yes |
| Line | Horizontal or vertical separator | No |
| ProgressBar | Determinate progress or animated marquee | No |
| Chart | Data visualization with multiple series | No |
| Table | Data table with borders and styled cells | No |

## Size Units

All size properties accept string values in one of these formats:

- **`Nch`** - Character units. `"30ch"` means 30 characters wide/tall.
- **`N%`** - Percentage of parent's available space. `"100%"` fills the parent. `"50%"` is half.
- **`fill`** - In a StackPanel on the main axis: takes all remaining space after siblings. On the cross axis: stretches to the full cross-axis size. Outside a StackPanel: behaves like `"100%"`.
- **`auto`** - Size is computed from content or children.

## API Reference

### TermuiX (Main Class)

```csharp
// Lifecycle
static TermuiX Init();
static void DeInit();
void Render(bool skipUnchanged = false);

// Widget tree
void LoadXml(string xml);
void AddToWindow(IWidget widget);
T? GetWidget<T>(string name) where T : class, IWidget;
List<T> GetWidgetsByGroup<T>(string group) where T : class, IWidget;

// Focus
void SetFocus(IWidget widget);
IWidget? FocusedWidget { get; }

// Custom types
void RegisterWidget(string tagName, Func<Dictionary<string, string>, IWidget> factory);
void UnregisterWidget(string tagName);
void RegisterComponent(string tagName, Func<Dictionary<string, string>, string> xmlFactory);
void UnregisterComponent(string tagName);

// Properties
static bool AllowCancelKeyExit { get; set; }  // default: true
bool MouseEnabled { get; set; }               // default: true

// Events
event EventHandler<ConsoleKeyInfo>? Shortcut;
event EventHandler<FocusChangedEventArgs>? FocusChanged;
event EventHandler<MouseEventArgs>? MouseClick;
```

### Render

`Render()` processes all pending input events and draws the current frame. The optional `skipUnchanged` parameter skips the console write if the output is identical to the previous frame. Useful for SSH or slow terminals.

### GetWidget / GetWidgetsByGroup

`GetWidget<T>(name)` finds a widget by its `Name` property. Names must be unique; duplicates throw `InvalidOperationException` when the tree is loaded.

`GetWidgetsByGroup<T>(group)` finds all widgets whose `Group` property contains the given group name. A widget's Group can hold multiple space-separated names (like CSS classes).

```csharp
termui.LoadXml(@"
<StackPanel Direction='Horizontal'>
    <Button Name='btn1' Group='toolbar primary'>OK</Button>
    <Button Name='btn2' Group='toolbar'>Cancel</Button>
</StackPanel>");

var btn1 = termui.GetWidget<Button>("btn1");
var toolbarButtons = termui.GetWidgetsByGroup<Button>("toolbar"); // returns both
var primaryButtons = termui.GetWidgetsByGroup<Button>("primary"); // returns only btn1
```

### RegisterWidget / RegisterComponent

**RegisterWidget** registers a custom widget type for use in XML. The factory receives XML attributes as a dictionary and returns an `IWidget`. Common attributes (Name, Width, colors, etc.) are applied automatically. If the returned widget is a Container, XML child elements are parsed and added.

```csharp
termui.RegisterWidget("MyWidget", attrs =>
{
    var label = attrs.GetValueOrDefault("Label", "default");
    return new MyCustomWidget(label);
});
// Use in XML: <MyWidget Label="hello" Width="20ch" />
```

**RegisterComponent** registers a composite widget that generates XML. The factory returns an XML string that gets parsed into a widget tree.

```csharp
var sidebar = new Sidebar(termui);
termui.RegisterComponent("Sidebar", attrs => sidebar.BuildXml());

termui.LoadXml(@"
<StackPanel Direction='Horizontal' Width='100%' Height='100%'>
    <Sidebar />
    <Container Width='fill' Height='100%'>...</Container>
</StackPanel>");
sidebar.Initialize(); // Bind events after LoadXml
```

### Ctrl+C Handling

By default, Ctrl+C calls `DeInit()` and exits the process. To prevent this:

```csharp
TermuiXLib.AllowCancelKeyExit = false;
// Now Ctrl+C does NOT exit. Use Console.CancelKeyPress to handle it.
```

## Common Properties (All Widgets)

Every widget supports these properties, settable via XML attributes or C# code:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Name | string? | null | Unique identifier for GetWidget lookup |
| Group | string? | null | Space-separated group names for GetWidgetsByGroup |
| Width | string | varies | Size: "30ch", "100%", "fill", "auto" |
| Height | string | varies | Size: "30ch", "100%", "fill", "auto" |
| MinWidth | string? | null | Minimum width constraint |
| MaxWidth | string? | null | Maximum width constraint |
| MinHeight | string? | null | Minimum height constraint |
| MaxHeight | string? | null | Maximum height constraint |
| PositionX | string | "0ch" | X offset within parent (for absolute positioning in Container) |
| PositionY | string | "0ch" | Y offset within parent (for absolute positioning in Container) |
| Visible | bool | true | Whether the widget is rendered and can receive focus |
| AllowWrapping | bool | true | Whether text wraps at the widget boundary |
| Disabled | bool | false | Disables focus and interaction |
| BackgroundColor | Color | Inherit | Background color |
| ForegroundColor | Color | Inherit | Text/foreground color |
| FocusBackgroundColor | Color | Inherit | Background when focused |
| FocusForegroundColor | Color | Inherit | Foreground when focused |
| DisabledBackgroundColor | Color | default | Background when disabled |
| DisabledForegroundColor | Color | default | Foreground when disabled |
| PaddingLeft | string | "0ch" | Inner padding left |
| PaddingTop | string | "0ch" | Inner padding top |
| PaddingRight | string | "0ch" | Inner padding right |
| PaddingBottom | string | "0ch" | Inner padding bottom |
| MarginLeft | string | "0ch" | Outer margin left |
| MarginTop | string | "0ch" | Outer margin top |
| MarginRight | string | "0ch" | Outer margin right |
| MarginBottom | string | "0ch" | Outer margin bottom |

Read-only properties available after `Render()`:

| Property | Type | Description |
|----------|------|-------------|
| ComputedWidth | int | Actual width in characters after layout |
| ComputedHeight | int | Actual height in characters after layout |
| Focussed | bool | Whether this widget currently has focus |
| Hovered | bool | Whether the mouse is over this widget |
| Parent | IWidget? | Parent widget in the tree |
| Children | IReadOnlyList | Child widgets |
| HasHorizontalScrollbar | bool | Whether horizontal scrollbar is visible |
| HasVerticalScrollbar | bool | Whether vertical scrollbar is visible |

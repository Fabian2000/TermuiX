# XML Reference

This document covers XML syntax, all available tags and attributes, text content, dynamic widget manipulation, and custom widget registration.

## Loading XML

```csharp
termui.LoadXml(@"
<Container Width='100%' Height='100%' BackgroundColor='#1e1e2e'>
    <Text ForegroundColor='#cdd6f4'>Hello</Text>
</Container>");
```

The XML string is parsed into a widget tree. Widget names are validated for uniqueness.

## Built-in Tags

| Tag | Widget |
|-----|--------|
| `Container` | Container |
| `StackPanel` | StackPanel |
| `Button` | Button |
| `Text` | Text |
| `Input` | Input |
| `Checkbox` | Checkbox |
| `RadioButton` | RadioButton |
| `ProgressBar` | ProgressBar |
| `Chart` | Chart |
| `Slider` | Slider |
| `TreeView` | TreeView |
| `Line` | Line |
| `Table` | Table |
| `Style` | Style element (see [styling.md](styling.md)) |
| `TableRow` | Child of Table |
| `TableCell` | Child of TableRow |
| `TreeNode` | Child of TreeView |

Tag names are case-insensitive. Unknown tags throw `NotSupportedException`.

## Common Attributes (All Widgets)

| Attribute | Example | Type |
|-----------|---------|------|
| `Name` | `Name='myButton'` | string |
| `Group` | `Group='toolbar primary'` | string (space-separated) |
| `Width` | `Width='100%'` | size (ch, %, fill, auto) |
| `Height` | `Height='3ch'` | size |
| `MinWidth` | `MinWidth='10ch'` | size |
| `MaxWidth` | `MaxWidth='50ch'` | size |
| `MinHeight` | `MinHeight='5ch'` | size |
| `MaxHeight` | `MaxHeight='20ch'` | size |
| `PositionX` | `PositionX='5ch'` | size |
| `PositionY` | `PositionY='3ch'` | size |
| `Visible` | `Visible='false'` | bool |
| `AllowWrapping` | `AllowWrapping='false'` | bool |
| `BackgroundColor` | `BackgroundColor='#1e1e2e'` | Color |
| `ForegroundColor` | `ForegroundColor='#cdd6f4'` | Color |
| `FocusBackgroundColor` | `FocusBackgroundColor='#313244'` | Color |
| `FocusForegroundColor` | `FocusForegroundColor='#ffffff'` | Color |
| `PaddingLeft` | `PaddingLeft='1ch'` | size |
| `PaddingTop` | `PaddingTop='0ch'` | size |
| `PaddingRight` | `PaddingRight='1ch'` | size |
| `PaddingBottom` | `PaddingBottom='0ch'` | size |
| `MarginLeft` | `MarginLeft='2ch'` | size |
| `MarginTop` | `MarginTop='1ch'` | size |
| `MarginRight` | `MarginRight='0ch'` | size |
| `MarginBottom` | `MarginBottom='0ch'` | size |

## Container / StackPanel Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Scrollable` | `Scrollable='true'` | bool |
| `ScrollX` | `ScrollX='true'` | bool |
| `ScrollY` | `ScrollY='true'` | bool |
| `BorderStyle` | `BorderStyle='Single'` | `None`, `Single`, `Double` |
| `RoundedCorners` | `RoundedCorners='true'` | bool |

## StackPanel-Only Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Direction` | `Direction='Horizontal'` | `Vertical`, `Horizontal` |
| `Justify` | `Justify='SpaceBetween'` | `Start`, `End`, `Center`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` |
| `Align` | `Align='Center'` | `Start`, `Center`, `End` |
| `Wrap` | `Wrap='true'` | bool |

## Button Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Text` | `Text='Click Me'` | string (alternative to inner text) |
| `BorderStyle` | `BorderStyle='None'` | `None`, `Single`, `Double` |
| `RoundedCorners` | `RoundedCorners='true'` | bool |
| `BorderColor` | `BorderColor='#808080'` | Color |
| `TextColor` | `TextColor='#ffffff'` | Color |
| `FocusBorderColor` | `FocusBorderColor='#89b4fa'` | Color |
| `FocusTextColor` | `FocusTextColor='#ffffff'` | Color |
| `TextStyle` | `TextStyle='Bold'` | `Normal`, `Bold`, `Italic`, `BoldItalic`, `Underline`, `Strikethrough` |
| `TextAlign` | `TextAlign='Left'` | `Left`, `Center`, `Right` |
| `Disabled` | `Disabled='true'` | bool |
| `DisabledBackgroundColor` | `DisabledBackgroundColor='#333'` | Color |
| `DisabledForegroundColor` | `DisabledForegroundColor='#666'` | Color |

## Text Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Content` | `Content='Hello'` | string (alternative to inner text) |
| `TextAlign` | `TextAlign='Center'` | `Left`, `Center`, `Right` |
| `Style` | `Style='Bold'` | `Normal`, `Bold`, `Italic`, `BoldItalic`, `Underline`, `Strikethrough` |
| `Markdown` | `Markdown='true'` | bool |
| `CodeBackgroundColor` | `CodeBackgroundColor='#313244'` | Color |
| `CodeForegroundColor` | `CodeForegroundColor='#cdd6f4'` | Color |

## Input Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Value` | `Value='initial text'` | string |
| `Multiline` | `Multiline='true'` | bool |
| `IsPassword` | `IsPassword='true'` | bool |
| `SubmitKey` | `SubmitKey='CtrlEnter'` | `Enter`, `CtrlEnter` |
| `Placeholder` | `Placeholder='Type here...'` | string |
| `BorderColor` | `BorderColor='#808080'` | Color (not rendered, see [styling.md](styling.md)) |
| `FocusBorderColor` | `FocusBorderColor='#ffffff'` | Color (not rendered) |
| `PlaceholderColor` | `PlaceholderColor='#666666'` | Color |
| `CursorColor` | `CursorColor='#ffffff'` | Color |
| `Disabled` | `Disabled='true'` | bool |
| `DisabledBackgroundColor` | `DisabledBackgroundColor='#333'` | Color |
| `DisabledForegroundColor` | `DisabledForegroundColor='#666'` | Color |

## Checkbox Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Checked` | `Checked='true'` | bool |
| `Disabled` | `Disabled='true'` | bool |
| `DisabledBackgroundColor` | `DisabledBackgroundColor='#333'` | Color |
| `DisabledForegroundColor` | `DisabledForegroundColor='#666'` | Color |

## RadioButton Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Checked` | `Checked='true'` | bool (maps to `Selected` property in C#) |
| `Disabled` | `Disabled='true'` | bool |
| `DisabledBackgroundColor` | `DisabledBackgroundColor='#333'` | Color |
| `DisabledForegroundColor` | `DisabledForegroundColor='#666'` | Color |

## ProgressBar Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Value` | `Value='0.5'` | double (0.0 to 1.0) |
| `Mode` | `Mode='Marquee'` | `Progress`, `Marquee` |

## Slider Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Value` | `Value='50'` | double |
| `Min` | `Min='0'` | double |
| `Max` | `Max='100'` | double |
| `Step` | `Step='5'` | double |
| `Disabled` | `Disabled='true'` | bool |
| `DisabledBackgroundColor` | `DisabledBackgroundColor='#333'` | Color |
| `DisabledForegroundColor` | `DisabledForegroundColor='#666'` | Color |

## Line Attributes

| Attribute | Example | Values |
|-----------|---------|--------|
| `Orientation` | `Orientation='Vertical'` | `Horizontal`, `Vertical` |
| `Type` | `Type='Dotted'` | `Solid`, `Double`, `Thick`, `Dotted` |

## Table Attributes

Table has no widget-specific XML attributes beyond the common ones. Table-specific properties like `BorderColor` must be set in code.

## TreeView Attributes

| Attribute | Type | Description |
| --- | --- | --- |
| HighlightBackgroundColor | Color | Background color of the selected node |
| HighlightForegroundColor | Color | Foreground color of the selected node |

FocusBackgroundColor and FocusForegroundColor are common attributes available on all widgets.

## Text Content

For Text, Button, and Input widgets, the inner text sets the widget's content:

```xml
<Text>This is the content</Text>
<Button>Click Me</Button>
<Input>Default value</Input>
```

The `\n` escape sequence is converted to actual newlines:

```xml
<Text>Line 1\nLine 2\nLine 3</Text>
```

Whitespace is trimmed from inner text.

## Auto-Sizing from Content

### Button

If no Width is set, the parser computes: `Width = textDisplayWidth + 4` (2 border + 2 padding).
If no Height is set, it defaults to `"3ch"` (1 border + 1 text + 1 border).

### Text

If no Width is set, it uses the display width of the widest line.
If no Height is set: without explicit Width, the line count; with explicit Width, `"auto"` (dynamic from wrapping).
If no content and no explicit size, both default to `"0ch"`.

## Table Row/Cell Syntax

```xml
<Table>
    <TableRow>
        <TableCell Text='Name' Style='Bold' ForegroundColor='#89b4fa' />
        <TableCell Text='Age' Style='Bold' ForegroundColor='#89b4fa' />
    </TableRow>
    <TableRow>
        <TableCell Text='Alice' />
        <TableCell Text='30' />
    </TableRow>
</Table>
```

### TableCell Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `Text` | string | Cell text content |
| `BackgroundColor` | Color | Cell background |
| `ForegroundColor` | Color | Cell text color |
| `Style` | TextStyle | Text style |

A TableCell can contain a child widget instead of text:

```xml
<TableCell>
    <Button Width='8ch' Height='1ch' BorderStyle='None'>Edit</Button>
</TableCell>
```

If both text and a child widget are present, the widget takes priority.

## TreeNode Syntax

```xml
<TreeView Name='tree'>
    <TreeNode Text='Folder' Expanded='true'>
        <TreeNode Text='SubFolder'>
            <TreeNode Text='File.txt' />
        </TreeNode>
    </TreeNode>
</TreeView>
```

### TreeNode Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `Text` | string | Node display text (or use inner text) |
| `Expanded` | bool | Whether the node starts expanded |

## Dynamic Widget Manipulation

After loading XML, you can add, remove, and modify widgets at runtime.

### Adding via XML String

```csharp
var panel = termui.GetWidget<StackPanel>("myPanel");
panel.Add(@"<Button Name='newBtn' Width='10ch'>New</Button>");

var newBtn = termui.GetWidget<Button>("newBtn");
newBtn.Click += (_, _) => { /* handle */ };
```

### Adding Programmatically

```csharp
var btn = new Button { Text = "Dynamic", Width = "10ch" };
btn.Click += (_, _) => { /* handle */ };
panel.Add(btn);
```

### Inserting at a Position

```csharp
panel.Insert(0, @"<Text ForegroundColor='#cdd6f4'>Inserted at top</Text>");
panel.Insert(2, someWidget);
```

### Removing Widgets

```csharp
panel.Remove(someWidget);  // Remove specific widget
panel.Clear();              // Remove all children
```

### Modifying Properties at Runtime

All properties can be changed at runtime. Changes take effect on the next `Render()` call.

```csharp
var text = termui.GetWidget<Text>("statusText");
text.Content = "Updated status";
text.ForegroundColor = Color.Parse("#f38ba8");

var container = termui.GetWidget<Container>("popup");
container.Visible = !container.Visible;  // Toggle visibility

var btn = termui.GetWidget<Button>("saveBtn");
btn.Disabled = true;
```

## Custom Widget Registration

### RegisterWidget

For custom widget types:

```csharp
termui.RegisterWidget("MyGauge", attrs =>
{
    var gauge = new MyGaugeWidget();
    if (attrs.TryGetValue("Value", out var val))
    {
        gauge.Value = double.Parse(val);
    }
    return gauge;
});
```

Use in XML:

```xml
<MyGauge Name='gauge1' Value='0.75' Width='30ch' Height='3ch' />
```

Common attributes (Name, Width, Height, colors, etc.) are applied automatically. The factory only handles widget-specific attributes. If the returned widget is a Container, child XML elements are parsed and added.

### RegisterComponent

For composite widgets that generate XML:

```csharp
var toolbar = new Toolbar(termui);
termui.RegisterComponent("Toolbar", attrs => toolbar.BuildXml());

termui.LoadXml(@"
<StackPanel Direction='Vertical' Width='100%' Height='100%'>
    <Toolbar />
    <Container Width='100%' Height='fill' />
</StackPanel>");

toolbar.Initialize();  // Bind events after LoadXml
```

### Unregister

```csharp
termui.UnregisterWidget("MyGauge");
termui.UnregisterComponent("Toolbar");
```

## Name Uniqueness

Widget names must be unique within the tree. `LoadXml()` validates this and throws `InvalidOperationException` on duplicates. Names added later via `Add()` are not validated, so avoid duplicates when adding dynamically.

## XML Escaping

Standard XML escaping rules apply:

| Character | Escape |
|-----------|--------|
| `<` | `&lt;` |
| `>` | `&gt;` |
| `&` | `&amp;` |
| `'` | `&apos;` |
| `"` | `&quot;` |

When building XML strings with user-provided text, use `SecurityElement.Escape()`:

```csharp
var escaped = SecurityElement.Escape(userText);
panel.Add($"<Text ForegroundColor='#cdd6f4'>{escaped}</Text>");
```

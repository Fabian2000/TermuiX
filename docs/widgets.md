# Widget Reference

Every built-in widget with all properties, events, and usage examples. Properties listed under "Common Properties" in [index.md](index.md) apply to all widgets and are not repeated here unless their default differs.

## Container

Base container widget. Positions children using absolute coordinates (PositionX, PositionY).

### Defaults

| Property | Default |
|----------|---------|
| Width | `"100%"` |
| Height | `"100%"` |
| BackgroundColor | `Color.Inherit` |
| CanFocus | `false` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| BorderStyle | BorderStyle? | null | `None`, `Single`, `Double`. null = no border. |
| RoundedCorners | bool | false | Round corners (only with Single border). |
| ScrollX | bool | false | Enable horizontal scrolling. |
| ScrollY | bool | false | Enable vertical scrolling. |
| Scrollable | bool | false | Convenience: sets both ScrollX and ScrollY. |

### Methods

```csharp
void Add(IWidget widget);           // Append child widget
void Add(string xml);               // Parse XML and append resulting widgets
void Insert(int index, IWidget w);  // Insert child at specific index
void Insert(int index, string xml); // Parse XML and insert at index
void Remove(IWidget widget);        // Remove a child widget
void Clear();                       // Remove all children
```

### Usage Notes

- A border consumes 1 character on each side. A `Width='20ch'` Container with border has 18ch of content width.
- Scrollbars appear automatically when children exceed the visible area. Vertical scrollbar: 1 column on the right. Horizontal scrollbar: 1 row at the bottom.
- Children are clipped to the container boundary.

### Example

```xml
<Container Width='50ch' Height='20ch' BackgroundColor='#1e1e2e'
    BorderStyle='Single' RoundedCorners='true' ScrollY='true'>
    <Text PositionX='1ch' PositionY='0ch' ForegroundColor='#cdd6f4'>Line 1</Text>
    <Text PositionX='1ch' PositionY='1ch' ForegroundColor='#cdd6f4'>Line 2</Text>
</Container>
```

---

## StackPanel

Arranges children sequentially along a direction axis. Extends Container. See [layout.md](layout.md) for full layout details.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"auto"` |
| Height | `"auto"` |
| Direction | `Vertical` |
| Justify | `Start` |
| Align | `Start` |
| Wrap | `false` |

**Important**: StackPanel defaults Width and Height to `"auto"`, not `"100%"`. Without explicit size, it shrinks to fit its children.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Direction | StackDirection | Vertical | `Vertical` or `Horizontal` |
| Justify | StackJustify | Start | Main axis: `Start`, `End`, `Center`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` |
| Align | StackAlign | Start | Cross axis: `Start`, `Center`, `End` |
| Wrap | bool | false | Wrap children to next line when they overflow |

Inherits all Container properties and methods.

### Example

```xml
<StackPanel Direction='Horizontal' Width='100%' Height='3ch' Align='Center'>
    <Text PaddingLeft='1ch' ForegroundColor='#cdd6f4'>Title</Text>
    <Text Width='fill' />
    <Button Width='10ch'>Menu</Button>
</StackPanel>
```

---

## Button

Clickable button with border, text content, and focus states.

### Defaults

| Property | Default |
|----------|---------|
| Width | auto-calculated: `textDisplayWidth + 4` (2 border + 2 padding) |
| Height | `"3ch"` (1 border + 1 text + 1 border) |
| BorderStyle | `Single` |
| CanFocus | `true` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | `""` | The button label (or use inner text in XML) |
| BorderStyle | BorderStyle | Single | `None`, `Single`, `Double` |
| RoundedCorners | bool | false | Round border corners |
| BorderColor | Color | (default) | Border color |
| TextColor | Color | (default) | Text foreground color |
| FocusBorderColor | Color | (default) | Border color when focused |
| FocusTextColor | Color | (default) | Text color when focused |
| TextStyle | TextStyle | Normal | `Normal`, `Bold`, `Italic`, `BoldItalic`, `Underline`, `Strikethrough` |
| TextAlign | TextAlign | Center | `Left`, `Center`, `Right` |
| Disabled | bool | false | Disables focus and click |
| DisabledBackgroundColor | Color | (default) | Background when disabled |
| DisabledForegroundColor | Color | (default) | Text color when disabled |

Use `BackgroundColor='Inherit'` and `FocusBackgroundColor='Inherit'` to make the button transparent against its parent.

### Events

```csharp
event EventHandler? Click;                      // Left click or Enter/Space when focused
event EventHandler<MouseEventArgs>? RightClick;  // Right mouse button click
```

### Usage Notes

- Enter and Space trigger Click when focused.
- With `BorderStyle='None'`, no border is drawn and the button acts like a clickable text label.
- PaddingLeft, PaddingRight, PaddingTop, PaddingBottom control spacing inside the border around the text.

### Example

```xml
<Button Name='btnOk' Width='15ch'
    BackgroundColor='#313244' FocusBackgroundColor='#45475a'
    TextColor='#cdd6f4' FocusTextColor='#ffffff'
    BorderColor='#45475a' FocusBorderColor='#89b4fa'
    BorderStyle='Single' RoundedCorners='true'>OK</Button>
```

```csharp
var btn = termui.GetWidget<Button>("btnOk");
btn.Click += (sender, e) => { /* handle click */ };
btn.RightClick += (sender, args) =>
{
    int mouseX = args.X; // absolute screen X
    int mouseY = args.Y; // absolute screen Y
};
```

---

## Text

Static text display widget. Supports plain text, alignment, styles, and Markdown rendering.

### Defaults

| Property | Default |
|----------|---------|
| Width | auto-calculated from content (widest line) |
| Height | auto-calculated from content (line count, or `"auto"` with explicit Width) |
| AllowWrapping | `true` |
| CanFocus | `false` |

When no content and no explicit size: both default to `"0ch"`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Content | string | `""` | The text to display (or use inner text in XML) |
| TextAlign | TextAlign | Left | `Left`, `Center`, `Right` |
| Style | TextStyle | Normal | `Normal`, `Bold`, `Italic`, `BoldItalic`, `Underline`, `Strikethrough` |
| Markdown | bool | false | Enable Markdown rendering |
| CodeBackgroundColor | Color | (default) | Background for inline code and code blocks |
| CodeForegroundColor | Color | (default) | Foreground for inline code and code blocks |

### Markdown Support

When `Markdown='true'`:
- `**bold**` renders as bold
- `*italic*` renders as italic
- `` `code` `` renders as inline code with CodeBackgroundColor/CodeForegroundColor
- `~~strikethrough~~` renders as strikethrough
- Fenced code blocks (triple backticks) render with syntax highlighting

### Text Content in XML

```xml
<Text ForegroundColor='#cdd6f4'>Hello World</Text>
<Text ForegroundColor='#cdd6f4'>Line 1\nLine 2</Text>
```

The `\n` sequence is converted to actual newlines.

### Example

```xml
<Text Width='100%' ForegroundColor='#cdd6f4' Style='Bold' TextAlign='Center'>
    Welcome to the App
</Text>
```

---

## Input

Text input with cursor, placeholder, and keyboard editing. Supports single-line and multi-line modes.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"30ch"` |
| Height | `"3ch"` |
| PaddingLeft | `"1ch"` |
| PaddingRight | `"1ch"` |
| CanFocus | `true` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | string | `""` | Current text value |
| Multiline | bool | false | Allow multiple lines |
| SubmitKey | SubmitKeyMode | Enter | `Enter` or `CtrlEnter`. Which key fires EnterPressed. |
| IsPassword | bool | false | Display text as dots |
| Placeholder | string | `""` | Placeholder shown when Value is empty |
| BorderColor | Color | (default) | Not rendered (see below) |
| FocusBorderColor | Color | (default) | Not rendered (see below) |
| PlaceholderColor | Color | (default) | Placeholder text color |
| CursorColor | Color | (default) | Cursor color |
| Disabled | bool | false | Disables input |
| DisabledBackgroundColor | Color | (default) | Background when disabled |
| DisabledForegroundColor | Color | (default) | Foreground when disabled |

### Events

```csharp
event EventHandler<string>? EnterPressed;  // Fired when submit key is pressed. Argument: current Value.
event EventHandler? EscapePressed;          // Fired when Escape is pressed.
event EventHandler<string>? TextChanged;    // Fired on every keystroke. Argument: new Value.
```

### Important: Input Has No Visible Border

The Input widget has BorderColor and FocusBorderColor properties, but **they are not rendered**. To get a visible border, wrap the Input in a bordered Container:

```xml
<Container Width='32ch' Height='3ch'
    BorderStyle='Single' RoundedCorners='true'
    BorderColor='#808080' FocusBorderColor='#ffffff'
    BackgroundColor='#1e1e2e'>
    <Input Width='100%' Height='1ch'
        Placeholder='Border visible'
        BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
        ForegroundColor='#cdd6f4' CursorColor='#cdd6f4' />
</Container>
```

Set Input Height to `"1ch"` (text line only), Container Height to `"3ch"` (1 border + 1 text + 1 border). Use `BackgroundColor='Inherit'` on the Input.

### Usage Notes

- When `SubmitKey='Enter'`, pressing Enter fires EnterPressed. When `SubmitKey='CtrlEnter'`, Ctrl+Enter fires EnterPressed, and plain Enter inserts a newline (only in Multiline mode).
- The cursor blinks at 500ms intervals when focused.

### Example

```xml
<Container Width='40ch' Height='3ch'
    BorderStyle='Single' RoundedCorners='true'
    BackgroundColor='#313244' BorderColor='#45475a' FocusBorderColor='#89b4fa'>
    <Input Name='nameInput' Width='100%' Height='1ch'
        Placeholder='Enter your name...'
        BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
        ForegroundColor='#cdd6f4' PlaceholderColor='#6c7086'
        CursorColor='#cdd6f4' />
</Container>
```

```csharp
var input = termui.GetWidget<Input>("nameInput");
input.EnterPressed += (sender, text) => { /* text is the submitted value */ };
input.TextChanged += (sender, text) => { /* text is the current value after each keystroke */ };
input.EscapePressed += (sender, e) => { /* handle cancel */ };
```

---

## Checkbox

Toggle checkbox (checked/unchecked).

### Defaults

| Property | Default |
|----------|---------|
| Width | `"2ch"` |
| Height | `"1ch"` |
| CanFocus | `true` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Checked | bool | false | Whether the checkbox is checked |
| Disabled | bool | false | Disables interaction |
| DisabledBackgroundColor | Color | (default) | Background when disabled |
| DisabledForegroundColor | Color | (default) | Foreground when disabled |

### Events

```csharp
event EventHandler<bool>? CheckedChanged;  // Fired when checked state changes. Argument: new Checked value.
```

### Usage Notes

- Space, Enter, or mouse click toggles the checked state.
- Pair with a Text widget for a label.

### Example

```xml
<StackPanel Direction='Horizontal' Align='Center'>
    <Checkbox Name='cbDarkMode' Checked='false'
        ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
    <Text ForegroundColor='#cdd6f4' MarginLeft='1ch'>Dark Mode</Text>
</StackPanel>
```

```csharp
var cb = termui.GetWidget<Checkbox>("cbDarkMode");
cb.CheckedChanged += (sender, isChecked) => { /* handle toggle */ };
```

---

## RadioButton

Single-selection radio button. Selecting one automatically deselects sibling RadioButtons in the same parent.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"1ch"` |
| Height | `"1ch"` |
| CanFocus | `true` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Selected | bool | false | Whether selected. XML attribute is `Checked`, C# property is `Selected`. |
| Disabled | bool | false | Disables interaction |
| DisabledBackgroundColor | Color | (default) | Background when disabled |
| DisabledForegroundColor | Color | (default) | Foreground when disabled |

### Events

```csharp
event EventHandler<bool>? SelectionChanged;  // Fired when selection changes. Argument: new Selected value.
```

### Usage Notes

- Space, Enter, or mouse click selects this radio button.
- Selecting a RadioButton automatically deselects all other RadioButton siblings in the same parent.
- A RadioButton does not toggle off when clicked again.

### Example

To add labels next to RadioButtons while keeping them as direct siblings (required for mutual exclusion), use a two-column layout:

```xml
<!-- RadioButtons must be direct siblings in the same parent for mutual exclusion.
     Use two parallel StackPanels: one for radios, one for labels. -->
<StackPanel Direction='Horizontal'>
    <StackPanel Direction='Vertical'>
        <RadioButton Name='rbSmall' Checked='true'
            ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
        <RadioButton Name='rbMedium'
            ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
        <RadioButton Name='rbLarge'
            ForegroundColor='#cdd6f4' FocusForegroundColor='#89b4fa' />
    </StackPanel>
    <StackPanel Direction='Vertical' MarginLeft='1ch'>
        <Text Height='1ch'>Small</Text>
        <Text Height='1ch'>Medium</Text>
        <Text Height='1ch'>Large</Text>
    </StackPanel>
</StackPanel>
```

```csharp
var rbSmall = termui.GetWidget<RadioButton>("rbSmall");
rbSmall.SelectionChanged += (sender, selected) =>
{
    if (selected) { /* rbSmall was selected */ }
};
```

---

## Slider

Numeric range slider with keyboard and mouse support.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"20ch"` |
| Height | `"1ch"` |
| Value | 0 |
| Min | 0 |
| Max | 100 |
| Step | 1.0 |
| ShowValue | true |
| CanFocus | `true` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | double | 0 | Current value |
| Min | double | 0 | Minimum value |
| Max | double | 100 | Maximum value |
| Step | double | 1.0 | Increment/decrement step |
| ShowValue | bool | true | Display the numeric value next to the slider (code only) |
| Disabled | bool | false | Disables interaction |
| DisabledBackgroundColor | Color | (default) | Background when disabled |
| DisabledForegroundColor | Color | (default) | Foreground when disabled |

### Events

```csharp
event EventHandler<double>? ValueChanged;  // Fired when value changes. Argument: new Value.
```

### Usage Notes

- Keyboard: Left/Right change by Step. Shift+Left/Right change by Step*10. Home sets Min. End sets Max.
- Mouse: Click on the track sets value proportionally. Drag moves the thumb.

### Example

```xml
<StackPanel Direction='Horizontal' Align='Center'>
    <Text ForegroundColor='#cdd6f4'>Volume:</Text>
    <Slider Name='volumeSlider' Width='30ch' Min='0' Max='100' Step='5' Value='50'
        ForegroundColor='#89b4fa' FocusForegroundColor='#89b4fa'
        BackgroundColor='#313244' MarginLeft='1ch' />
</StackPanel>
```

```csharp
var slider = termui.GetWidget<Slider>("volumeSlider");
slider.ValueChanged += (sender, value) => { /* value is 0-100 */ };
```

---

## Line

Horizontal or vertical separator line.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"1ch"` (vertical) or auto |
| Height | `"1ch"` (horizontal) or auto |
| CanFocus | `false` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Orientation | LineOrientation | Horizontal | `Horizontal` or `Vertical` |
| Type | LineType | Solid | `Solid`, `Double`, `Thick`, `Dotted` |

### Usage Notes

- Horizontal lines force Height to `"1ch"`. Vertical lines force Width to `"1ch"`.
- Non-interactive, no children, no focus.

### Example

```xml
<StackPanel Direction='Vertical' Width='100%'>
    <Text ForegroundColor='#cdd6f4'>Above the line</Text>
    <Line Orientation='Horizontal' Type='Solid' Width='100%'
        ForegroundColor='#45475a' BackgroundColor='#1e1e2e' />
    <Text ForegroundColor='#cdd6f4'>Below the line</Text>
</StackPanel>
```

---

## ProgressBar

Progress indicator with determinate and indeterminate (marquee) modes.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"30ch"` |
| Height | `"1ch"` |
| Value | 0.0 |
| Mode | Progress |
| ShowPercentage | true |
| CanFocus | `false` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | double | 0.0 | Progress from 0.0 to 1.0 |
| Mode | ProgressBarMode | Progress | `Progress` (determinate) or `Marquee` (animated) |
| FilledChar | char | block | Character for filled portion (code only) |
| EmptyChar | char | light shade | Character for empty portion (code only) |
| ShowPercentage | bool | true | Show percentage text centered on bar (code only) |

### Usage Notes

- In Progress mode, the bar fills from left to right based on Value.
- In Marquee mode, an animated block moves back and forth. Value is ignored.

### Example

```xml
<ProgressBar Name='progress' Width='40ch' Value='0.0'
    ForegroundColor='#89b4fa' BackgroundColor='#313244' />
```

```csharp
var bar = termui.GetWidget<ProgressBar>("progress");
bar.Value = 0.75; // 75% complete
```

---

## Chart

Data visualization with one or more data series.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"60ch"` |
| Height | `"15ch"` |
| ShowLegend | true |
| ShowAxes | true |
| YAxisWidth | 6 |
| CanFocus | `false` |

### Properties (Code Only)

All Chart-specific properties must be set in code. They are not available as XML attributes.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Series | List\<ChartDataSeries\> | (empty) | Data series (read-only, use AddSeries) |
| MinY | double? | null | Minimum Y value. null = auto-scale. |
| MaxY | double? | null | Maximum Y value. null = auto-scale. |
| XLabels | List\<string\> | (empty) | Labels for the X axis |
| ShowLegend | bool | true | Show legend below the chart |
| ShowAxes | bool | true | Show Y and X axes |
| YAxisWidth | int | 6 | Character width for Y-axis labels |

### ChartDataSeries

```csharp
public class ChartDataSeries
{
    public string Label { get; set; }       // Legend label
    public List<double> Data { get; set; }  // Data points
    public Color Color { get; set; }        // Series color (default: White)
}
```

### Methods

```csharp
void AddSeries(ChartDataSeries series);  // Add a data series
void ClearSeries();                       // Remove all series
```

### Example

```csharp
var chart = termui.GetWidget<Chart>("myChart");
chart.AddSeries(new ChartDataSeries
{
    Label = "CPU",
    Data = new List<double> { 20, 45, 30, 60, 80, 55 },
    Color = Color.Parse("#89b4fa")
});
chart.AddSeries(new ChartDataSeries
{
    Label = "Memory",
    Data = new List<double> { 40, 42, 45, 50, 48, 52 },
    Color = Color.Parse("#f38ba8")
});
chart.XLabels = new List<string> { "10s", "20s", "30s", "40s", "50s", "60s" };
```

---

## Table

Data table with borders and styled cells. Size is auto-calculated from content.

### Defaults

| Property | Default |
|----------|---------|
| Width | auto-calculated from column widths |
| Height | auto-calculated from row count |
| BorderStyle | `Single` |
| RoundedCorners | false |
| CanFocus | `false` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Rows | List\<TableRow\> | (empty) | Table rows |
| BorderStyle | BorderStyle? | Single | Border style |
| HasBorder | bool | true | Show borders (code only) |
| RoundedCorners | bool | false | Round outer corners |
| BorderColor | Color | (default) | Border color (code only) |

### Width/Height Are Read-Only

The Table auto-calculates its size from content. Setting Width or Height has no effect. Column widths are determined by the widest cell in each column.

### Table Structure (Code)

```csharp
public class TableRow
{
    public List<TableCell> Cells { get; }
    public void AddCell(TableCell cell);
}

public class TableCell
{
    public string? Text { get; set; }
    public Color BackgroundColor { get; set; }
    public Color ForegroundColor { get; set; }
    public TextStyle Style { get; set; }
    public IWidget? Widget { get; set; }  // Embed a widget instead of text
}
```

### Methods

```csharp
void AddRow(TableRow row);  // Add a row
```

### Usage Notes

- Tables are display-only. No interactive events.
- A TableCell can contain either Text or a Widget. If both are set, Widget takes priority.
- Column widths cannot be manually controlled.

### XML Example

```xml
<Table Name='myTable' BorderStyle='Single' RoundedCorners='true'>
    <TableRow>
        <TableCell Text='Name' Style='Bold' ForegroundColor='#89b4fa' />
        <TableCell Text='Value' Style='Bold' ForegroundColor='#89b4fa' />
    </TableRow>
    <TableRow>
        <TableCell Text='CPU' />
        <TableCell Text='45%' />
    </TableRow>
</Table>
```

---

## TreeView

Hierarchical tree with expand/collapse, keyboard navigation, and mouse support.

### Defaults

| Property | Default |
|----------|---------|
| Width | `"30ch"` |
| Height | `"auto"` |
| AllowWrapping | `false` |
| CanFocus | `true` |
| HighlightBackgroundColor | `ConsoleColor.Gray` |
| HighlightForegroundColor | `ConsoleColor.Black` |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Root | TreeNode | (hidden root) | The invisible root node. Add children to this. (code only) |
| SelectedNode | TreeNode? | (read-only) | Currently selected node. (code only) |
| HighlightBackgroundColor | Color | Gray | Background of selected row (XML and code) |
| HighlightForegroundColor | Color | Black | Foreground of selected row (XML and code) |

### Events

```csharp
event EventHandler<TreeNode>? NodeSelected;  // Fired on Enter/Space/Click
event EventHandler<TreeNode>? NodeToggled;    // Fired on expand or collapse
```

### Methods

```csharp
void SelectNode(TreeNode node);  // Programmatically select a visible node
```

### TreeNode

```csharp
public class TreeNode
{
    public string Text { get; set; }
    public bool IsExpanded { get; set; }
    public TreeNode? Parent { get; }
    public List<TreeNode> Children { get; }
    public object? Tag { get; set; }            // User data (e.g., file path)

    public TreeNode AddChild(string text);      // Add child, returns new node
    public TreeNode AddChild(TreeNode node);    // Add existing node as child
    public void RemoveChild(TreeNode node);     // Remove child
}
```

### Keyboard Navigation

| Key | Action |
|-----|--------|
| Up / Down | Move selection |
| Right | Expand node, or move to first child if already expanded |
| Left | Collapse node, or move to parent if already collapsed |
| Enter / Space | Toggle expand/collapse and fire NodeSelected |
| Home / End | Jump to first / last visible node |

### Mouse

- Click on the expand indicator toggles expand/collapse.
- Click elsewhere on a row selects the node.

### Usage Notes

- The root node is invisible. Only its children are displayed.
- Indentation is 2 characters per depth level.
- Height defaults to `"auto"`, meaning the TreeView is as tall as its visible nodes. Place it inside a scrollable Container for scrolling.
- HighlightBackgroundColor, HighlightForegroundColor, FocusBackgroundColor, and FocusForegroundColor are all settable via XML.

### XML Example

```xml
<TreeView Name='tree' Width='30ch' Height='auto'
    BackgroundColor='#1e1e2e' ForegroundColor='#cdd6f4'
    HighlightBackgroundColor='#313244' HighlightForegroundColor='#ffffff'
    FocusBackgroundColor='#1e1e2e' FocusForegroundColor='#cdd6f4'>
    <TreeNode Text='Root Folder' Expanded='true'>
        <TreeNode Text='Sub Folder 1'>
            <TreeNode Text='File A' />
            <TreeNode Text='File B' />
        </TreeNode>
        <TreeNode Text='Sub Folder 2' />
    </TreeNode>
</TreeView>
```

### Lazy Loading Example

Load subdirectories on demand when the user expands a node. The TreeView starts empty (no XML children) and is populated in code:

```xml
<TreeView Name='dirTree' Width='30ch' Height='auto'
    BackgroundColor='#1e1e2e' ForegroundColor='#cdd6f4'
    HighlightBackgroundColor='#1a3050' HighlightForegroundColor='#cdd6f4' />
```

```csharp
var tree = termui.GetWidget<TreeView>("dirTree");

var root = tree.Root.AddChild("Home");
root.Tag = "/home/user";
var placeholder = root.AddChild("...");
placeholder.Tag = "__placeholder__";

tree.NodeToggled += (sender, node) =>
{
    if (node.IsExpanded && node.Children.Count == 1
        && node.Children[0].Tag as string == "__placeholder__")
    {
        node.RemoveChild(node.Children[0]);

        var path = node.Tag as string;
        foreach (var dir in Directory.GetDirectories(path))
        {
            var child = node.AddChild(Path.GetFileName(dir));
            child.Tag = dir;
            var sub = child.AddChild("...");
            sub.Tag = "__placeholder__";
        }
    }
};
```

# Layout System

TermuiX has two container widgets for layout: **Container** and **StackPanel**. Container uses absolute positioning. StackPanel arranges children sequentially along an axis.

## Size Units

All size properties (Width, Height, MinWidth, MaxWidth, MinHeight, MaxHeight, PositionX, PositionY, padding, margins) accept these formats:

### Character Units (`Nch`)

Fixed size in terminal characters. `"30ch"` means 30 characters.

```xml
<Button Width='30ch' Height='3ch'>Click Me</Button>
```

### Percentage (`N%`)

Percentage of the parent's available content area (after the parent's border and padding are subtracted). `"100%"` fills the parent. `"50%"` is half.

```xml
<Container Width='100%' Height='100%'>
    <Text Width='50%' Height='1ch'>Half width</Text>
</Container>
```

When StackPanel children use percentage widths that add up to 100%, rounding is handled automatically so they fill the parent exactly without gaps.

### Fill (`fill`)

`fill` has different behavior depending on context:

- **Main axis** (e.g., `Width='fill'` in a Horizontal StackPanel): Takes all remaining space after non-fill siblings are measured. If multiple siblings use `fill`, the remaining space is divided equally.
- **Cross axis** (e.g., `Height='fill'` in a Horizontal StackPanel): Stretches to the full cross-axis size of the panel.
- **Outside StackPanel** (in a plain Container): Behaves like `"100%"`.

```xml
<StackPanel Direction='Horizontal' Width='100%'>
    <Button Width='10ch'>Left</Button>
    <Text Width='fill'>This takes all remaining space</Text>
    <Button Width='10ch'>Right</Button>
</StackPanel>
```

### Auto (`auto`)

Size is computed from content or children. For StackPanel, `auto` means the panel shrinks to exactly fit its children. For Text, `auto` means height is computed from text wrapping.

```xml
<StackPanel Direction='Vertical' Width='auto' Height='auto'>
    <Text>Line 1</Text>
    <Text>Line 2</Text>
</StackPanel>
<!-- This panel is exactly as wide as its widest child and as tall as the sum of children -->
```

## Container

Base container widget. Positions children using absolute coordinates (PositionX, PositionY).

### Container Defaults

| Property | Default |
|----------|---------|
| Width | `"100%"` |
| Height | `"100%"` |
| BackgroundColor | `Color.Inherit` |
| ForegroundColor | `Color.Inherit` |
| CanFocus | `false` |

### Container Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| BorderStyle | BorderStyle? | null | `None`, `Single`, `Double`. null = no border. |
| RoundedCorners | bool | false | Round corners (only with Single border). |
| ScrollX | bool | false | Enable horizontal scrolling. |
| ScrollY | bool | false | Enable vertical scrolling. |
| Scrollable | bool | false | Convenience: sets both ScrollX and ScrollY. |

### Container Methods

```csharp
void Add(IWidget widget);           // Append child widget
void Add(string xml);               // Parse XML and append resulting widgets
void Insert(int index, IWidget w);  // Insert child at specific index
void Insert(int index, string xml); // Parse XML and insert at index
void Remove(IWidget widget);        // Remove a child widget
void Clear();                       // Remove all children
```

### Container Layout

Children are positioned absolutely within the content area. The content area is the container's size minus border (1ch per side) minus padding.

```xml
<Container Width='40ch' Height='10ch' BorderStyle='Single' PaddingLeft='1ch'>
    <!-- Content area: 40 - 2 (border) - 1 (padLeft) = 37ch wide, 10 - 2 (border) = 8ch tall -->
    <Text PositionX='0ch' PositionY='0ch'>Top-left of content area</Text>
    <Text PositionX='0ch' PositionY='7ch'>Bottom of content area</Text>
</Container>
```

### Scrolling

When ScrollX or ScrollY is enabled, children can extend beyond the visible area. Scrollbars appear automatically when content exceeds the container.

- A vertical scrollbar takes 1 character column on the right.
- A horizontal scrollbar takes 1 character row at the bottom.
- Mouse wheel scrolls vertically by 3 lines per tick. Ctrl+Wheel scrolls horizontally.
- `ScrollOffsetX` and `ScrollOffsetY` properties (type `long`) can be read/written in code.

## StackPanel

StackPanel extends Container. It arranges children sequentially along a direction axis.

### StackPanel Defaults

| Property | Default |
|----------|---------|
| Width | `"auto"` |
| Height | `"auto"` |
| Direction | `Vertical` |
| Justify | `Start` |
| Align | `Start` |
| Wrap | `false` |

**Important**: StackPanel defaults Width and Height to `"auto"`, not `"100%"` like Container. A StackPanel without explicit size shrinks to fit its children. If you want a StackPanel to fill its parent, set Width and/or Height explicitly.

### StackPanel Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Direction | StackDirection | Vertical | `Vertical` or `Horizontal` |
| Justify | StackJustify | Start | Main axis distribution (see below) |
| Align | StackAlign | Start | Cross axis alignment (see below) |
| Wrap | bool | false | Wrap children to next line when they overflow |

StackPanel inherits all Container properties and methods.

### Direction

`Vertical` stacks children top-to-bottom (main axis = Y, cross axis = X).
`Horizontal` stacks children left-to-right (main axis = X, cross axis = Y).

```xml
<StackPanel Direction='Horizontal'>
    <Button>A</Button>
    <Button>B</Button>
    <Button>C</Button>
</StackPanel>
<!-- Renders: [A] [B] [C] in a row -->
```

### Justify (Main Axis)

Controls how children are distributed along the main axis.

| Value | Behavior |
|-------|----------|
| `Start` | Children packed at the start. Default. |
| `End` | Children packed at the end. |
| `Center` | Children centered. |
| `SpaceBetween` | First child at start, last at end, equal space between. |
| `SpaceAround` | Equal space around each child. Edge gaps are half the inner gaps. |
| `SpaceEvenly` | Equal space between all children and edges. |

```xml
<StackPanel Direction='Horizontal' Width='100%' Justify='SpaceBetween'>
    <Button>Left</Button>
    <Button>Center</Button>
    <Button>Right</Button>
</StackPanel>
```

### Align (Cross Axis)

Controls how children are aligned on the cross axis.

| Value | Behavior |
|-------|----------|
| `Start` | Aligned to start of cross axis. Default. |
| `Center` | Centered on cross axis. |
| `End` | Aligned to end of cross axis. |

```xml
<StackPanel Direction='Horizontal' Height='5ch' Align='Center'>
    <Button Height='3ch'>Centered vertically</Button>
</StackPanel>
```

### Wrap

When `Wrap='true'`, children that exceed the main axis wrap to the next line/column.

```xml
<StackPanel Direction='Horizontal' Width='40ch' Wrap='true'>
    <Button Width='15ch'>A</Button>
    <Button Width='15ch'>B</Button>
    <Button Width='15ch'>C</Button>
    <!-- C wraps to next line because 15+15+15 > 40 -->
</StackPanel>
```

### Fill in StackPanel

On the **main axis**, `fill` distributes remaining space:

1. All non-fill children are measured first.
2. Their total size is subtracted from the panel's main axis size.
3. The remaining space is divided equally among fill children.

On the **cross axis**, `fill` stretches the child to the full cross-axis size.

```xml
<StackPanel Direction='Horizontal' Width='100%'>
    <Text Width='10ch'>Label:</Text>
    <Input Width='fill' />
    <!-- Input gets: panel width - 10ch -->
</StackPanel>
```

### Auto Sizing

When Width or Height is `"auto"`, the StackPanel computes its size from children:

- **Vertical, auto Height**: Height = sum of all children's heights + margins.
- **Horizontal, auto Width**: Width = sum of all children's widths + margins.
- **Cross axis auto**: Size = maximum child size on the cross axis.

Auto-sized StackPanels cannot contain `fill` children on the auto axis (there is no remaining space to fill).

### Margins in StackPanel

Margins on children are part of the layout. `MarginLeft`, `MarginRight`, `MarginTop`, `MarginBottom` create space around each child.

```xml
<StackPanel Direction='Vertical'>
    <Text MarginBottom='1ch'>First (1ch gap below)</Text>
    <Text MarginTop='1ch'>Second (1ch gap above, total 2ch between)</Text>
</StackPanel>
```

## Padding vs Margins

**Padding** is inner space between a widget's edge and its content.

**Margins** are outer space around a widget, pushing it away from siblings or parent edges.

```
[margin] [border] [padding] [content] [padding] [border] [margin]
```

## Absolute Positioning

Within a plain Container, children use `PositionX` and `PositionY` for placement. These are offsets from the content area origin (after border and padding).

```xml
<Container Width='50ch' Height='20ch'>
    <Text PositionX='5ch' PositionY='3ch'>At column 5, row 3</Text>
    <Button PositionX='10ch' PositionY='10ch'>Floating Button</Button>
</Container>
```

In a StackPanel, PositionX and PositionY are not used for layout. The StackPanel computes positions automatically from Direction, Justify, Align, and margins.

## Min/Max Constraints

`MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight` clamp the computed size.

```xml
<Text Width='fill' MinWidth='10ch' MaxWidth='50ch'>
    This text fills available space but stays between 10 and 50 characters wide.
</Text>
```

## Nested Layout Example

```xml
<StackPanel Direction='Vertical' Width='100%' Height='100%'>
    <!-- Header: fixed height -->
    <StackPanel Direction='Horizontal' Width='100%' Height='3ch' Align='Center'>
        <Text Width='fill' PaddingLeft='1ch'>App Title</Text>
        <Button Width='10ch'>Menu</Button>
    </StackPanel>

    <!-- Body: fills remaining space -->
    <StackPanel Direction='Horizontal' Width='100%' Height='fill'>
        <!-- Sidebar: fixed width -->
        <StackPanel Direction='Vertical' Width='25ch' Height='100%'>
            <Text>Sidebar</Text>
        </StackPanel>

        <!-- Content: fills remaining width -->
        <Container Width='fill' Height='100%' ScrollY='true'>
            <Text Width='100%'>Scrollable content here...</Text>
        </Container>
    </StackPanel>

    <!-- Footer: fixed height -->
    <StackPanel Direction='Horizontal' Width='100%' Height='1ch'>
        <Text Width='fill'>Status: Ready</Text>
    </StackPanel>
</StackPanel>
```

This creates a header-sidebar-content-footer layout where the body fills all vertical space and the content area fills all horizontal space after the sidebar.

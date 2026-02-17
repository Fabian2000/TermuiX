# Styling

This document covers colors, borders, focus states, text styles, and the Style element.

## Colors

TermuiX supports three color modes.

### ConsoleColor (16 Colors)

The classic 16 terminal colors. Work on every terminal.

```xml
<Text ForegroundColor='White' BackgroundColor='Black'>Classic colors</Text>
```

Supported names (case-insensitive): `Black`, `DarkBlue`, `DarkGreen`, `DarkCyan`, `DarkRed`, `DarkMagenta`, `DarkYellow`, `Gray`, `DarkGray`, `Blue`, `Green`, `Cyan`, `Red`, `Magenta`, `Yellow`, `White`.

### TrueColor RGB

24-bit RGB colors. Requires a terminal with TrueColor support (most modern terminals).

Three formats:

```xml
<!-- #RRGGBB (6 digits) -->
<Text ForegroundColor='#cdd6f4'>Full hex</Text>

<!-- #RGB (3 digits, expanded to #RRGGBB) -->
<Text ForegroundColor='#cdf'>Short hex</Text>

<!-- rgb(R,G,B) -->
<Text ForegroundColor='rgb(205,214,244)'>RGB function</Text>
```

### Color.Inherit

A special value meaning "use the parent's color." Colors cascade down the widget tree until a child overrides them.

```xml
<Container BackgroundColor='#1e1e2e'>
    <!-- This Text inherits #1e1e2e as its background -->
    <Text BackgroundColor='Inherit' ForegroundColor='#cdd6f4'>Inherits parent bg</Text>
</Container>
```

In XML, use the string `"Inherit"` (case-insensitive). In C#, use `Color.Inherit`.

Most widgets default BackgroundColor and ForegroundColor to `Color.Inherit`. Set a color on the root Container and all children inherit it unless they override it.

If no ancestor sets a color, the default fallback is Black (background) and White (foreground).

### Color in Code

```csharp
Color blue = Color.Parse("#89b4fa");
Color red = Color.Parse("Red");
Color inherit = Color.Parse("Inherit");

// From ConsoleColor
Color white = ConsoleColor.White;  // implicit conversion

// Inherit sentinel
Color inherit2 = Color.Inherit;
```

### Color Inheritance Example

```xml
<Container BackgroundColor='#1e1e2e' ForegroundColor='#cdd6f4'>
    <!-- Everything inside inherits these colors unless overridden -->
    <StackPanel Direction='Vertical'>
        <Text>Inherits #cdd6f4 on #1e1e2e</Text>
        <Text ForegroundColor='#f38ba8'>Red text on #1e1e2e</Text>
        <Container BackgroundColor='#313244'>
            <Text>Inherits #cdd6f4 on #313244</Text>
        </Container>
    </StackPanel>
</Container>
```

## Borders

### BorderStyle Values

| Value | Description |
|-------|-------------|
| `None` | No border |
| `Single` | Single-line border |
| `Double` | Double-line border |

### RoundedCorners

When `RoundedCorners='true'` and `BorderStyle='Single'`, corners use rounded box-drawing characters. Has no effect with Double borders.

```xml
<Container BorderStyle='Single' RoundedCorners='true'>
    <!-- Rounded corners -->
</Container>
```

### Border Space

A border consumes 1 character on each side. A Container with `Width='20ch'` and `BorderStyle='Single'` has a content area of 18ch wide and (Height - 2) tall.

### Button Borders

Buttons have their own border. `BorderColor` and `FocusBorderColor` control the border color in normal and focused states.

```xml
<Button BorderStyle='Single' BorderColor='#45475a' FocusBorderColor='#89b4fa'>
    Click Me
</Button>
```

With `BorderStyle='None'`, the button renders without border, acting like a clickable text label.

### Input Has No Visible Border

The Input widget has `BorderColor` and `FocusBorderColor` properties, but **they are not rendered**. To get a visible border around an Input, wrap it in a bordered Container:

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

Set Input Height to `"1ch"` (just the text line), Container Height to `"3ch"` (1 border + 1 text + 1 border). Use `BackgroundColor='Inherit'` on the Input so it blends with the Container.

## Focus States

Every widget has four color properties for focus:

| Property | When Used |
|----------|-----------|
| `BackgroundColor` | Widget is not focused |
| `ForegroundColor` | Widget is not focused |
| `FocusBackgroundColor` | Widget is focused |
| `FocusForegroundColor` | Widget is focused |

If the focus color is `Inherit`, it falls back to the non-focus color.

### Transparent Buttons

To make a Button blend with its parent background, use Inherit for both states:

```xml
<Container BackgroundColor='#1e1e2e'>
    <Button BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
        TextColor='#808080' FocusTextColor='#ffffff'
        BorderColor='#808080' FocusBorderColor='#ffffff'>
        Transparent Button
    </Button>
</Container>
```

### Disabled States

| Property | When Used |
|----------|-----------|
| `DisabledBackgroundColor` | Widget has `Disabled='true'` |
| `DisabledForegroundColor` | Widget has `Disabled='true'` |

Disabled widgets cannot receive focus or respond to input.

## Text Styles

| Value | Effect |
|-------|--------|
| `Normal` | No styling |
| `Bold` | Bold/bright text |
| `Italic` | Italic text |
| `BoldItalic` | Both bold and italic |
| `Underline` | Underlined text |
| `Strikethrough` | Strikethrough text |

For Text widgets, use the `Style` attribute:

```xml
<Text Style='Bold' ForegroundColor='#cdd6f4'>Bold heading</Text>
<Text Style='Italic' ForegroundColor='#a6adc8'>Italic subtext</Text>
```

For Buttons, use `TextStyle`:

```xml
<Button TextStyle='Bold'>Bold Button</Button>
```

## Style Element

The Style element applies properties to multiple widgets by Name or Group, similar to CSS. Place Style elements as children of a Container or StackPanel.

### Syntax

```xml
<Container>
    <!-- Match by Name -->
    <Style Name='myButton' BackgroundColor='#313244' ForegroundColor='#cdd6f4' />

    <!-- Match by Group -->
    <Style Group='toolbar' BackgroundColor='#181825' />

    <!-- Widgets -->
    <Button Name='myButton'>Styled by Name</Button>
    <Button Group='toolbar'>Styled by Group</Button>
    <Button Group='toolbar primary'>Also styled by toolbar</Button>
</Container>
```

### Matching Rules

- `Name` match: the widget's Name equals the style's Name (case-insensitive).
- `Group` match: any of the widget's space-separated Group values equals the style's Group value (case-insensitive).
- Styles are applied in document order. Later styles override earlier ones.

### Style Properties

Any property that can be set on a widget via XML can be used in a Style element:

```xml
<Style Group='navButton'
    Width='8ch' Height='3ch'
    BackgroundColor='Inherit' FocusBackgroundColor='Inherit'
    TextColor='#808080' FocusTextColor='#ffffff'
    BorderColor='#808080' FocusBorderColor='#ffffff'
    BorderStyle='Single' RoundedCorners='true' />
```

### Widget Groups

A widget's Group can contain multiple space-separated names:

```xml
<Button Group='toolbar primary'>Both toolbar and primary groups</Button>
```

A Style with `Group='toolbar'` matches this button because "toolbar" is one of its groups.

## Hover State

When mouse input is enabled, hovering over a focusable widget highlights it using `FocusBackgroundColor` and `FocusForegroundColor` — the same colors used for keyboard focus. No extra configuration needed.

You can also check the hover state in code:

```csharp
var btn = termui.GetWidget<Button>("myBtn");
if (((IWidget)btn).Hovered)
{
    // Widget is being hovered
}
```

# TermuiX

A modern, declarative terminal UI framework for .NET 9.0 that brings XML-based UI definition to the console.

![TermuiX Demo](https://github.com/Fabian2000/Termui/blob/main/Preview.png?raw=true)

## Why TermuiX?

TermuiX offers a unique approach to terminal UI development:

- **Zero Dependencies** - Pure .NET implementation with no external dependencies or native bindings
- **Declarative UI** - Define your interface in XML, similar to web development or XAML
- **Simple Rendering Model** - Straightforward render loop - call `Render()` when ready, no complex state management
- **Pure C#** - No need to learn platform-specific APIs or deal with interop complexities
- **Cross-Platform** - Works on any platform that supports .NET 9.0 and ANSI terminals

Perfect for building dashboards, TUI tools, interactive CLIs, and terminal-based applications.

## Features

- **Declarative XML-based UI** - Define your terminal UI using familiar XML syntax
- **Rich Widget Set** - Comprehensive collection of interactive widgets
- **Event-Driven Architecture** - Clean event handling for user interactions
- **Flexible Styling** - Support for colors, borders, text styles (bold, italic, underline, strikethrough)
- **Advanced Layout** - Positioned and scrollable containers with padding support
- **Keyboard Navigation** - Built-in Tab/Shift+Tab navigation and custom keyboard shortcuts
- **Multi-line Support** - Tables and inputs with multi-line text capabilities

## Available Widgets

### Interactive Widgets
- **Button** - Clickable buttons with customizable styling and focus states
- **Input** - Single-line and multi-line text input with password mode
- **Checkbox** - Toggle checkboxes with checked/unchecked states
- **RadioButton** - Mutually exclusive radio button groups
- **Slider** - Adjustable value slider with min/max/step configuration

### Display Widgets
- **Text** - Static or dynamic text with various alignments and styles
- **Table** - Multi-row, multi-column tables with borders and text styling
- **Chart** - Line charts with multiple data series and legends
- **ProgressBar** - Progress indicators and marquee animations
- **Line** - Horizontal and vertical separator lines
- **Container** - Layout containers with optional borders and scrolling

## Installation

```bash
dotnet add package TermuiX
```

## Quick Start

```csharp
using System.Threading;
using TermuiX.Widgets;

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow"
          Style="Bold">
        Welcome to TermuiX!
    </Text>

    <Button Name="myButton" PositionX="2ch" PositionY="3ch"
            BorderColor="White" TextColor="Cyan"
            RoundedCorners="true">
        Click Me
    </Button>
</Container>
""";

// Initialize and load UI
var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

// Get widget reference and attach event
var button = termui.GetWidget<Button>("myButton");
if (button is not null)
{
    bool running = true;
    button.Click += (sender, e) => running = false;

    try
    {
        while (running)
        {
            termui.Render();
            Thread.Sleep(16); // 60 FPS
        }
    }
    finally
    {
        TermuiX.TermuiX.DeInit();
    }
}
```

## Usage Examples

### Creating a Form

```xml
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Container PositionX="5ch" PositionY="5ch" Width="32ch" Height="3ch"
               BorderStyle="Single" RoundedCorners="true">
        <Input Name="nameInput" PositionX="0ch" PositionY="0ch"
               Width="30ch" Height="1ch"
               Placeholder="Enter your name..."
               FocusBackgroundColor="DarkBlue"
               FocusForegroundColor="White" />
    </Container>

    <Checkbox Name="agreeCheckbox" PositionX="5ch" PositionY="9ch" />
    <Text PositionX="7ch" PositionY="9ch">I agree to the terms</Text>

    <Button Name="submitButton" PositionX="5ch" PositionY="11ch"
            RoundedCorners="true">Submit</Button>
</Container>
```

### Creating a Table

```xml
<Table Name="dataTable" PositionX="5ch" PositionY="5ch"
       BorderStyle="Single" BorderColor="White"
       RoundedCorners="true">
    <TableRow>
        <TableCell Style="Bold">Name</TableCell>
        <TableCell Style="Bold">Age</TableCell>
        <TableCell Style="Bold">City</TableCell>
    </TableRow>
    <TableRow>
        <TableCell>Alice</TableCell>
        <TableCell>25</TableCell>
        <TableCell>New York</TableCell>
    </TableRow>
    <TableRow>
        <TableCell>Bob</TableCell>
        <TableCell>30</TableCell>
        <TableCell>London</TableCell>
    </TableRow>
</Table>
```

### Creating a Chart

```csharp
var chart = termui.GetWidget<Chart>("salesChart");
if (chart is not null)
{
    var series1 = new ChartDataSeries
    {
        Label = "Product A",
        Color = ConsoleColor.Green,
        Data = [10, 15, 13, 17, 22, 28, 35, 42]
    };

    chart.AddSeries(series1);
    chart.XLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug"];
}
```

## Widget Properties

### Common Properties

All widgets support these common properties:

- `Name` - Unique identifier for retrieving widgets via `GetWidget<T>()`
- `PositionX`, `PositionY` - Position in characters (e.g., "10ch")
- `Width`, `Height` - Dimensions in characters or percentage (e.g., "50ch", "100%")
- `PaddingLeft`, `PaddingRight`, `PaddingTop`, `PaddingBottom` - Padding in characters
- `ForegroundColor`, `BackgroundColor` - Standard console colors
- `FocusForegroundColor`, `FocusBackgroundColor` - Colors when focused
- `Visible` - Show/hide widget
- `CanFocus` - Enable/disable focus capability

### Container-Specific

- `BorderStyle` - `Single`, `Double`, or `None`
- `BorderColor` - Border color
- `RoundedCorners` - Enable rounded corners (single border only)
- `Scrollable` - Enable scrolling for overflow content

### Input-Specific

- `Placeholder` - Placeholder text
- `IsPassword` - Hide input characters
- `Multiline` - Enable multi-line input
- `Value` - Current text value

### Text Styles

- `Normal` - Regular text
- `Bold` - Bold text (Unicode mathematical alphanumeric symbols)
- `Italic` - Italic text (Unicode mathematical alphanumeric symbols)
- `BoldItalic` - Combined bold and italic
- `Underline` - Underlined text
- `Strikethrough` - Text with strikethrough

## Event Handling

### Button Click Events

```csharp
button.Click += (sender, e) =>
{
    Console.WriteLine("Button clicked!");
};
```

### Input Events

```csharp
input.TextChanged += (sender, e) =>
{
    Console.WriteLine($"New value: {input.Value}");
};

input.EnterPressed += (sender, e) =>
{
    Console.WriteLine("Enter key pressed!");
};
```

### Checkbox Events

```csharp
checkbox.CheckedChanged += (sender, e) =>
{
    Console.WriteLine($"Checked: {checkbox.Checked}");
};
```

### RadioButton Events

```csharp
radioButton.SelectionChanged += (sender, e) =>
{
    Console.WriteLine($"Selected: {radioButton.Selected}");
};
```

### Slider Events

```csharp
slider.ValueChanged += (sender, newValue) =>
{
    Console.WriteLine($"Value: {newValue}");
};
```

### Keyboard Shortcuts

```csharp
termui.Shortcut += (sender, key) =>
{
    if (key.Key == ConsoleKey.S) // Ctrl+S
    {
        // Handle save shortcut
    }
};
```

## Advanced Features

### Modal Dialogs

Create modal dialogs by toggling container visibility:

```csharp
var mainPage = termui.GetWidget<Container>("mainPage");
var confirmModal = termui.GetWidget<Container>("confirmModal");

// Show modal
mainPage.Visible = false;
confirmModal.Visible = true;
termui.SetFocus(modalYesButton);
```

### Scrollable Containers

```xml
<Container Width="30ch" Height="10ch" Scrollable="true">
    <!-- Content larger than container will be scrollable -->
    <RadioButton Name="option1" PositionX="2ch" PositionY="2ch" />
    <RadioButton Name="option2" PositionX="2ch" PositionY="3ch" />
    <!-- ... many more options ... -->
</Container>
```

Use Page Up/Page Down or Ctrl+Page Up/Page Down to scroll.

### Dynamic Updates

```csharp
// Update text dynamically
outputText.Content = "Updated message!";

// Update progress bar
progressBar.Value = 0.75; // 75%

// Update slider
volumeSlider.Value = 50;
```

## Console State Management

TermuiX automatically manages console state:

```csharp
// Initialize - clears screen, hides cursor
var termui = TermuiX.TermuiX.Init();

// DeInit - restores cursor, clears screen, resets colors
TermuiX.TermuiX.DeInit();
```

### Ctrl+C Handling

Ctrl+C automatically calls `DeInit()` to restore console state. You can control this behavior:

```csharp
// Disable automatic exit on Ctrl+C
TermuiX.TermuiX.AllowCancelKeyExit = false;
```

## Border Styles

TermuiX supports multiple border styles:

- **Single** - Single-line borders (┌─┐│└┘)
- **Double** - Double-line borders (╔═╗║╚╝)
- **RoundedCorners** - Rounded corners with single borders (╭─╮│╰╯)

```xml
<Container BorderStyle="Single" RoundedCorners="true">
    <!-- Content -->
</Container>
```

## Requirements

- .NET 9.0 or higher
- Terminal with Unicode support
- Terminal with ANSI color support

## Additional Features

### Reusable Components

Create reusable UI components by adding XML strings dynamically to containers:

```csharp
var container = termui.GetWidget<Container>("myContainer");

string cardComponent = """
    <Container Width="30ch" Height="5ch" BorderStyle="Single" BorderColor="White">
        <Text PositionX="1ch" PositionY="1ch">Card Title</Text>
    </Container>
    """;

container.Add(cardComponent);  // Components are automatically cloned
```

### Negative Positioning

Use negative positions to place widgets off-screen or create animated transitions:

```xml
<Container PositionX="-50ch" PositionY="0ch" Width="40ch" Height="100%">
    <!-- Sidebar that can slide in from the left -->
</Container>
```

```csharp
// Animate position changes
sidebar.PositionX = $"{currentPosition}ch";
```

### Widget Cloning

All widgets support deep and shallow cloning:

```csharp
var originalButton = termui.GetWidget<Button>("myButton");
var clonedButton = originalButton.Clone(deep: true);  // Deep clone with children
```

## Samples

Check out the included samples:

- **TermuiX.Demo** - Comprehensive demo with forms, charts, modals, and more
- **TermuiX.Demo2** - Table widget demonstration with multi-line text
- **TermuiX.Demo3** - Animated sidebar with burger menu using negative positioning
- **TermuiX.Demo4** - Component model demonstration with dynamic widget addition
- **TermuiX.Demo5** - Auto-scroll messenger with programmatic scroll control

Run samples:

```bash
cd samples/TermuiX.Demo
dotnet run

cd samples/TermuiX.Demo2
dotnet run

cd samples/TermuiX.Demo3
dotnet run

cd samples/TermuiX.Demo4
dotnet run

cd samples/TermuiX.Demo5
dotnet run
```

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

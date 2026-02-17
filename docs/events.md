# Events and Input

This document covers keyboard input, mouse input, the focus system, and scrolling.

## Keyboard Input

### Key Routing

When a key is pressed:

1. **Tab / Shift+Tab**: Moves focus forward or backward through focusable widgets.
2. **Ctrl+Key combinations**: Ctrl+A, Ctrl+C, Ctrl+V, and Ctrl+Enter are forwarded to the focused widget. Ctrl+PageUp/PageDown scroll horizontally. All other Ctrl+Key combinations fire the `Shortcut` event.
3. **PageUp / PageDown**: Scrolls the nearest scrollable parent vertically.
4. **Escape**: Forwarded to the focused widget AND fires the `Shortcut` event.
5. **All other keys**: Forwarded to the focused widget.

### Shortcuts

The `Shortcut` event on the TermuiX instance fires for Ctrl+Key combinations (except Ctrl+A/C/V/Enter and Ctrl+PageUp/PageDown) and for Escape.

```csharp
termui.Shortcut += (sender, key) =>
{
    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
    {
        switch (key.Key)
        {
            case ConsoleKey.F:
                // Ctrl+F: Focus search
                break;
            case ConsoleKey.N:
                // Ctrl+N: New item
                break;
            case ConsoleKey.Q:
                // Ctrl+Q: Quit
                break;
        }
    }

    if (key.Key == ConsoleKey.Escape)
    {
        // Close popups, cancel operations, etc.
    }
};
```

### Per-Widget Key Handling

Each focusable widget responds to keys when focused:

- **Button**: Enter and Space trigger Click.
- **Input**: Arrow keys move cursor, Backspace/Delete edit text, Enter fires EnterPressed (or inserts newline in multiline+CtrlEnter mode), Escape fires EscapePressed.
- **Checkbox**: Space and Enter toggle checked state.
- **RadioButton**: Space and Enter select the radio button.
- **Slider**: Left/Right change value by Step, Shift+Left/Right by Step*10, Home sets Min, End sets Max.
- **TreeView**: Up/Down navigate, Left/Right expand/collapse or navigate parent/child, Enter/Space toggle and select, Home/End jump to first/last.

### Ctrl+C Handling

By default, Ctrl+C terminates the application. To handle Ctrl+C yourself:

```csharp
TermuiXLib.AllowCancelKeyExit = false;
// Now Ctrl+C does NOT exit. Use Console.CancelKeyPress if you need to react to it.
```

## Mouse Input

Mouse input is enabled by default. Disable it with:

```csharp
termui.MouseEnabled = false;
```

### Mouse Event Types

```csharp
public enum MouseEventType
{
    LeftButtonPressed,
    LeftButtonReleased,
    RightButtonPressed,
    RightButtonReleased,
    WheelUp,
    WheelDown,
    Moved
}
```

### MouseEventArgs

```csharp
public readonly struct MouseEventArgs
{
    public int X { get; init; }              // Absolute screen column (0-based)
    public int Y { get; init; }              // Absolute screen row (0-based)
    public int LocalX { get; init; }         // X relative to widget's top-left
    public int LocalY { get; init; }         // Y relative to widget's top-left
    public MouseEventType EventType { get; init; }
    public bool Shift { get; init; }         // Whether Shift was held
}
```

### MouseClick Event

The `MouseClick` event on TermuiX fires for ALL mouse events (not just clicks). Filter by EventType:

```csharp
termui.MouseClick += (sender, args) =>
{
    if (args.EventType == MouseEventType.LeftButtonPressed)
    {
        // Close popups when clicking outside them
        ClosePopups();
    }
};
```

### Widget Mouse Handling

- **Button**: Left click fires Click. Right click fires RightClick.
- **Checkbox**: Left click toggles checked state.
- **RadioButton**: Left click selects the button.
- **Slider**: Left click and drag set value proportionally based on position.
- **TreeView**: Click on the expand indicator toggles expand/collapse. Click elsewhere on a row selects the node.

### Mouse Click and Focus

Left-clicking on a focusable widget gives it focus. Right-clicking also sets focus to the clicked widget.

## Focus System

### Focus Traversal

- **Tab**: Moves to the next focusable widget (wraps around).
- **Shift+Tab**: Moves to the previous focusable widget (wraps around).
- **Click**: Sets focus to the clicked widget.
- **SetFocus(widget)**: Sets focus in code.

Only visible, focusable, non-disabled widgets can receive focus.

### FocusChanged Event

```csharp
public class FocusChangedEventArgs : EventArgs
{
    public IWidget Widget { get; }           // The widget that received focus
    public FocusChangeReason Reason { get; } // Why focus changed
}

public enum FocusChangeReason
{
    Keyboard,     // Tab / Shift+Tab
    Click,        // Mouse click
    Programmatic  // SetFocus() called in code
}
```

```csharp
termui.FocusChanged += (sender, args) =>
{
    if (args.Reason == FocusChangeReason.Click)
    {
        ClosePopups();
    }
};
```

### Auto-Scroll on Focus

When focus changes, the nearest scrollable parent auto-scrolls to keep the focused widget visible.

### Programmatic Focus

```csharp
termui.SetFocus(someWidget);
```

SetFocus silently does nothing if the widget is not focusable, disabled, or invisible.

## Scrolling

### Scroll Triggers

| Input | Effect |
|-------|--------|
| Mouse wheel up/down | Scroll by 3 lines |
| Ctrl + mouse wheel | Horizontal scroll by 3 columns |
| PageUp / PageDown | Scroll by 1 line |
| Ctrl+PageUp / Ctrl+PageDown | Horizontal scroll by 1 column |
| Tab | Auto-scroll to keep focused widget visible |

If a container has only horizontal scrolling (no vertical), mouse wheel automatically routes to horizontal scroll without needing Ctrl.

### Scroll Properties

```csharp
bool ScrollX { get; }              // Whether horizontal scrolling is enabled
bool ScrollY { get; }              // Whether vertical scrolling is enabled
long ScrollOffsetX { get; set; }   // Current horizontal scroll position
long ScrollOffsetY { get; set; }   // Current vertical scroll position
```

## Popup Close Pattern

A common pattern for closing popups when the user clicks elsewhere, changes focus, or presses Escape:

```csharp
void ClosePopups()
{
    if (sortDropdown is not null) { sortDropdown.Visible = false; }
    if (contextMenu is not null) { contextMenu.Visible = false; }
}

// Close on click outside
termui.MouseClick += (sender, args) =>
{
    if (args.EventType == MouseEventType.LeftButtonPressed)
    {
        ClosePopups();
    }
};

// Close on focus change
termui.FocusChanged += (sender, args) =>
{
    if (args.Reason == FocusChangeReason.Click)
    {
        ClosePopups();
    }
};

// Close on Escape
termui.Shortcut += (sender, key) =>
{
    if (key.Key == ConsoleKey.Escape)
    {
        ClosePopups();
    }
};
```

Open the popup by setting `Visible = true` and positioning it with PositionX/PositionY.

namespace TermuiX;

/// <summary>
/// Specifies the type of mouse event.
/// </summary>
public enum MouseEventType
{
    /// <summary>Left button pressed.</summary>
    LeftButtonPressed,
    /// <summary>Left button released.</summary>
    LeftButtonReleased,
    /// <summary>Right button pressed.</summary>
    RightButtonPressed,
    /// <summary>Right button released.</summary>
    RightButtonReleased,
    /// <summary>Mouse wheel scrolled up.</summary>
    WheelUp,
    /// <summary>Mouse wheel scrolled down.</summary>
    WheelDown,
    /// <summary>Mouse moved.</summary>
    Moved
}

/// <summary>
/// Provides data for mouse events.
/// </summary>
public readonly struct MouseEventArgs
{
    /// <summary>The zero-based column (X) position of the mouse in console coordinates.</summary>
    public int X { get; init; }

    /// <summary>The zero-based row (Y) position of the mouse in console coordinates.</summary>
    public int Y { get; init; }

    /// <summary>The X position relative to the widget's top-left corner.</summary>
    public int LocalX { get; init; }

    /// <summary>The Y position relative to the widget's top-left corner.</summary>
    public int LocalY { get; init; }

    /// <summary>The type of mouse event.</summary>
    public MouseEventType EventType { get; init; }

    /// <summary>Whether the Shift key was held during the mouse event.</summary>
    public bool Shift { get; init; }
}

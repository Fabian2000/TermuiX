namespace TermuiX;

/// <summary>
/// Specifies the reason a focus change occurred.
/// </summary>
public enum FocusChangeReason
{
    /// <summary>Focus changed via keyboard (Tab / Shift+Tab).</summary>
    Keyboard,

    /// <summary>Focus changed because the user clicked a widget.</summary>
    Click,

    /// <summary>Focus changed programmatically via SetFocus().</summary>
    Programmatic
}

/// <summary>
/// Event data for the FocusChanged event.
/// </summary>
public class FocusChangedEventArgs : EventArgs
{
    /// <summary>The widget that received focus.</summary>
    public IWidget Widget { get; }

    /// <summary>Why the focus changed.</summary>
    public FocusChangeReason Reason { get; }

    internal FocusChangedEventArgs(IWidget widget, FocusChangeReason reason)
    {
        Widget = widget;
        Reason = reason;
    }
}

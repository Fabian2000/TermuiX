using System.Text;

namespace TermuiX
{
    /// <summary>
    /// Defines the core interface for all UI widgets in TermuiX.
    /// </summary>
    public interface IWidget
    {
        /// <summary>
        /// Gets or sets the unique name of the widget.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Gets or sets the group name for the widget.
        /// </summary>
        string? Group { get; set; }

        /// <summary>
        /// Gets or sets the parent widget.
        /// </summary>
        IWidget? Parent { get; set; }

        /// <summary>
        /// Gets the collection of child widgets.
        /// </summary>
        List<IWidget> Children { get; }

        /// <summary>
        /// Gets or sets the width of the widget (e.g., "100%", "50ch").
        /// </summary>
        string Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the widget (e.g., "100%", "10ch").
        /// </summary>
        string Height { get; set; }

        /// <summary>
        /// Gets or sets the minimum width constraint (e.g., "10ch", "20%"). Empty means no constraint.
        /// </summary>
        string MinWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum width constraint (e.g., "50ch", "80%"). Empty means no constraint.
        /// </summary>
        string MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum height constraint (e.g., "3ch", "10%"). Empty means no constraint.
        /// </summary>
        string MinHeight { get; set; }

        /// <summary>
        /// Gets or sets the maximum height constraint (e.g., "20ch", "50%"). Empty means no constraint.
        /// </summary>
        string MaxHeight { get; set; }

        /// <summary>
        /// Gets the computed width of the widget in characters after percentage calculations.
        /// This is automatically calculated during rendering and should not be set manually.
        /// </summary>
        int ComputedWidth { get; internal set; }

        /// <summary>
        /// Gets the computed height of the widget in characters after percentage calculations.
        /// This is automatically calculated during rendering and should not be set manually.
        /// </summary>
        int ComputedHeight { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this widget had a vertical scrollbar in the previous frame.
        /// Used for immediate-mode UI scrollbar space reservation.
        /// </summary>
        bool HasVerticalScrollbar { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this widget had a horizontal scrollbar in the previous frame.
        /// Used for immediate-mode UI scrollbar space reservation.
        /// </summary>
        bool HasHorizontalScrollbar { get; internal set; }

        /// <summary>
        /// Gets or sets the left padding of the widget.
        /// </summary>
        string PaddingLeft { get; set; }

        /// <summary>
        /// Gets or sets the top padding of the widget.
        /// </summary>
        string PaddingTop { get; set; }

        /// <summary>
        /// Gets or sets the right padding of the widget.
        /// </summary>
        string PaddingRight { get; set; }

        /// <summary>
        /// Gets or sets the bottom padding of the widget.
        /// </summary>
        string PaddingBottom { get; set; }

        /// <summary>
        /// Gets or sets the left margin of the widget.
        /// </summary>
        string MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the top margin of the widget.
        /// </summary>
        string MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the right margin of the widget.
        /// </summary>
        string MarginRight { get; set; }

        /// <summary>
        /// Gets or sets the bottom margin of the widget.
        /// </summary>
        string MarginBottom { get; set; }

        /// <summary>
        /// Gets or sets the X position of the widget relative to its parent.
        /// </summary>
        string PositionX { get; set; }

        /// <summary>
        /// Gets or sets the Y position of the widget relative to its parent.
        /// </summary>
        string PositionY { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget is visible.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether text wrapping is allowed.
        /// </summary>
        bool AllowWrapping { get; set; }

        /// <summary>
        /// Gets or sets the background color of the widget.
        /// </summary>
        Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color of the widget.
        /// </summary>
        Color ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color when the widget is focused.
        /// </summary>
        Color FocusBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color when the widget is focused.
        /// </summary>
        Color FocusForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget is currently focused.
        /// </summary>
        bool Focussed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is currently hovering over this widget.
        /// </summary>
        bool Hovered { get; set; }

        /// <summary>
        /// Gets a value indicating whether the widget can receive focus.
        /// </summary>
        bool CanFocus { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget is disabled.
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the background color when the widget is disabled.
        /// If null, the normal background color is used.
        /// </summary>
        Color? DisabledBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color when the widget is disabled.
        /// Defaults to DarkGray for better visual distinction.
        /// </summary>
        Color DisabledForegroundColor { get; set; }

        /// <summary>
        /// Gets a value indicating whether horizontal scrolling is enabled.
        /// </summary>
        bool ScrollX { get; }

        /// <summary>
        /// Gets a value indicating whether vertical scrolling is enabled.
        /// </summary>
        bool ScrollY { get; }

        /// <summary>
        /// Gets or sets the horizontal scroll offset.
        /// </summary>
        long ScrollOffsetX { get; set; }

        /// <summary>
        /// Gets or sets the vertical scroll offset.
        /// </summary>
        long ScrollOffsetY { get; set; }

        /// <summary>
        /// Gets the raw character array representation of the widget for rendering.
        /// </summary>
        /// <returns>A 2D array of Runes where each Rune represents one Unicode scalar value.</returns>
        Rune[][] GetRaw();

        /// <summary>
        /// Gets optional per-cell foreground and background colors for the raw content.
        /// Returns null by default — the renderer uses the widget's flat colors.
        /// Widgets that need per-cell coloring (e.g. TreeView selection highlighting) override this.
        /// </summary>
        (Color[][] fg, Color[][] bg)? GetRawColors() => null;

        /// <summary>
        /// Handles keyboard input when the widget is focused.
        /// </summary>
        /// <param name="keyInfo">The key press information.</param>
        void KeyPress(ConsoleKeyInfo keyInfo);

        /// <summary>
        /// Handles mouse input when the mouse interacts with this widget.
        /// Default implementation does nothing; widgets can override to handle mouse clicks.
        /// </summary>
        /// <param name="args">The mouse event data.</param>
        void MousePress(MouseEventArgs args) { }

        /// <summary>
        /// Creates a deep or shallow copy of the widget.
        /// </summary>
        /// <param name="deep">If true, recursively clones all children. If false, children are not cloned.</param>
        /// <returns>A new widget instance with copied properties and no parent reference.</returns>
        IWidget Clone(bool deep = true);
    }
}

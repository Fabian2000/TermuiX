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
        ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color of the widget.
        /// </summary>
        ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color when the widget is focused.
        /// </summary>
        ConsoleColor FocusBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color when the widget is focused.
        /// </summary>
        ConsoleColor FocusForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget is currently focused.
        /// </summary>
        bool Focussed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the widget can receive focus.
        /// </summary>
        bool CanFocus { get; }

        /// <summary>
        /// Gets a value indicating whether the widget is scrollable.
        /// </summary>
        bool Scrollable { get; }

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
        /// Handles keyboard input when the widget is focused.
        /// </summary>
        /// <param name="keyInfo">The key press information.</param>
        void KeyPress(ConsoleKeyInfo keyInfo);
    }
}

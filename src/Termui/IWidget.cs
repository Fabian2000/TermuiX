namespace Termui
{
    public interface IWidget
    {
        string? Name { get; set; }
        string? Group { get; set; }
        IWidget? Parent { get; set; }
        List<IWidget> Children { get; }
        string Width { get; set; }
        string Height { get; set; }
        string PaddingLeft { get; set; }
        string PaddingTop { get; set; }
        string PaddingRight { get; set; }
        string PaddingBottom { get; set; }
        string PositionX { get; set; }
        string PositionY { get; set; }
        bool Visible { get; set; }
        bool AllowWrapping { get; set; }
        
        ConsoleColor BackgroundColor { get; set; }
        ConsoleColor ForegroundColor { get; set; }

        ConsoleColor FocusBackgroundColor { get; set; }
        ConsoleColor FocusForegroundColor { get; set; }

        bool Focussed { get; set; }

        bool CanFocus { get; }

        bool Scrollable { get; }
        long ScrollOffsetX { get; set; }
        long ScrollOffsetY { get; set; }

        char[][] GetRaw();

        void KeyPress(ConsoleKeyInfo keyInfo);
    }
}
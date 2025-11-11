using Termui.Widgets;

// Create a container
var container = new Container
{
    Width = "100%",
    Height = "100%",
    BackgroundColor = ConsoleColor.DarkBlue
};

// Create a text widget
var title = new Text("Welcome to Termui!")
{
    PositionX = "2ch",
    PositionY = "1ch",
    ForegroundColor = ConsoleColor.Yellow
};

// Create buttons
var button1 = new Button("Click Me!")
{
    Width = "15ch",
    Height = "3ch",
    PositionX = "5ch",
    PositionY = "5ch",
    BorderColor = ConsoleColor.White,
    TextColor = ConsoleColor.Cyan,
    FocusBorderColor = ConsoleColor.Green,
    FocusTextColor = ConsoleColor.White,
    BackgroundColor = ConsoleColor.DarkBlue
};

var button2 = new Button("Another Button")
{
    Width = "20ch",
    Height = "3ch",
    PositionX = "5ch",
    PositionY = "9ch",
    BorderColor = ConsoleColor.White,
    TextColor = ConsoleColor.Magenta,
    FocusBorderColor = ConsoleColor.Green,
    FocusTextColor = ConsoleColor.White,
    BackgroundColor = ConsoleColor.DarkBlue
};

// Add click handlers
button1.Click += (sender, e) => Console.Title = "Button 1 clicked!";
button2.Click += (sender, e) => Console.Title = "Button 2 clicked!";

// Add widgets to container
container.Add(title);
container.Add(button1);
container.Add(button2);

// Initialize Termui and add container
var termui = Termui.Termui.Init();
termui.AddToWindow(container);

try
{
    // Render
    termui.Render();

    // Wait for key press
    Console.ReadKey(true);
}
finally
{
    // Clean up
    Termui.Termui.DeInit();
    Console.WriteLine("\n\nDemo finished!");
}

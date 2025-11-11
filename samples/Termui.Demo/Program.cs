using Termui.Widgets;

// XML-based UI definition with named widgets for event binding
var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Welcome to Termui!
    </Text>

    <Input Name="nameInput" PositionX="5ch" PositionY="5ch"
           Width="30ch" Height="1ch" Placeholder="Enter your name..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Input Name="passwordInput" PositionX="5ch" PositionY="7ch"
           Width="30ch" Height="1ch" IsPassword="true" Placeholder="Enter password..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Input Name="messageInput" PositionX="5ch" PositionY="10ch"
           Width="50ch" Height="5ch" Multiline="true" Placeholder="Enter message..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Button Name="submitButton" PositionX="5ch" PositionY="17ch"
            BorderColor="White" TextColor="Cyan"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Submit
    </Button>

    <Button Name="exitButton" PositionX="20ch" PositionY="17ch"
            BorderColor="White" TextColor="Magenta"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Exit
    </Button>

    <Text Name="outputText" PositionX="5ch" PositionY="20ch"
          Width="100ch" Height="10ch"
          ForegroundColor="Green" BackgroundColor="DarkBlue">
    </Text>
</Container>
""";

// Initialize Termui and load XML (parsed once, kept in DOM)
var termui = Termui.Termui.Init();
termui.LoadXml(xml);

// Bind events to named widgets
var nameInput = termui.GetWidget<Input>("nameInput");
var passwordInput = termui.GetWidget<Input>("passwordInput");
var messageInput = termui.GetWidget<Input>("messageInput");
var submitButton = termui.GetWidget<Button>("submitButton");
var exitButton = termui.GetWidget<Button>("exitButton");
var outputText = termui.GetWidget<Text>("outputText");

if (exitButton is not null && submitButton is not null)
{
    bool running = true;
    DateTime? messageDisplayedAt = null;
    exitButton.Click += (sender, e) => running = false;

    if (submitButton is not null && nameInput is not null && passwordInput is not null && messageInput is not null && outputText is not null)
    {
        submitButton.Click += (sender, e) =>
        {
            outputText.Content = $"Submitted! Name: {nameInput.Value}, Password: {passwordInput.Value}, Message: {messageInput.Value}";
            messageDisplayedAt = DateTime.Now;
        };
    }

    try
    {
        // User-controlled loop (like ImGui)
        while (running)
        {
            // Clear message after 2 seconds
            if (messageDisplayedAt.HasValue && (DateTime.Now - messageDisplayedAt.Value).TotalSeconds >= 2)
            {
                outputText!.Content = string.Empty;
                messageDisplayedAt = null;
            }

            // Render frame (input is processed automatically)
            termui.Render();

            // ~60 FPS
            Thread.Sleep(16);
        }
    }
    finally
    {
        // Clean up
        Termui.Termui.DeInit();
        Console.WriteLine("\n\nDemo finished!");
    }
}
else
{
    Console.WriteLine("Error: Could not find widgets");
}

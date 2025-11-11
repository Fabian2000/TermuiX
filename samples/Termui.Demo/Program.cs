// XML-based UI definition
var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text Width="25ch" Height="1ch" PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Welcome to Termui!
    </Text>

    <Button Width="15ch" Height="3ch" PositionX="5ch" PositionY="5ch"
            BorderColor="White" TextColor="Cyan"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Click Me!
    </Button>

    <Button Width="20ch" Height="3ch" PositionX="5ch" PositionY="9ch"
            BorderColor="White" TextColor="Magenta"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Another Button
    </Button>
</Container>
""";

// Initialize Termui and load XML (parsed once, kept in DOM)
var termui = Termui.Termui.Init();
termui.LoadXml(xml);

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

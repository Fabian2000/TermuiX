var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue" Scrollable="true">
    <Button Name="wideButton" PositionX="5ch" PositionY="5ch" Width="20ch" Height="3ch"
            BorderColor="White" TextColor="Cyan"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue" RoundedCorners="true">
        Click Me
    </Button>

    <Line PositionX="5ch" PositionY="10ch" Width="50ch"
          Orientation="Horizontal" Type="Solid"
          ForegroundColor="White" BackgroundColor="DarkBlue" />

    <Line PositionX="5ch" PositionY="12ch" Width="50ch"
          Orientation="Horizontal" Type="Double"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue" />

    <Line PositionX="5ch" PositionY="14ch" Width="50ch"
          Orientation="Horizontal" Type="Thick"
          ForegroundColor="Cyan" BackgroundColor="DarkBlue" />

    <Line PositionX="5ch" PositionY="16ch" Width="50ch"
          Orientation="Horizontal" Type="Dotted"
          ForegroundColor="Green" BackgroundColor="DarkBlue" />

    <Line PositionX="60ch" PositionY="5ch" Height="15ch"
          Orientation="Vertical" Type="Solid"
          ForegroundColor="White" BackgroundColor="DarkBlue" />

    <Line PositionX="62ch" PositionY="5ch" Height="15ch"
          Orientation="Vertical" Type="Double"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue" />

    <Line PositionX="64ch" PositionY="5ch" Height="15ch"
          Orientation="Vertical" Type="Thick"
          ForegroundColor="Cyan" BackgroundColor="DarkBlue" />

    <Line PositionX="66ch" PositionY="5ch" Height="15ch"
          Orientation="Vertical" Type="Dotted"
          ForegroundColor="Green" BackgroundColor="DarkBlue" />
</Container>
""";

var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

var wideButton = termui.GetWidget<TermuiX.Widgets.Button>("wideButton");

if (wideButton is not null)
{
    bool running = true;
    wideButton.Click += (sender, e) =>
    {
        Console.WriteLine("Wide button clicked!");
        running = false;
    };

    try
    {
        while (running)
        {
            termui.Render();
            Thread.Sleep(16);
        }
    }
    finally
    {
        TermuiX.TermuiX.DeInit();
        Console.WriteLine("\n\nDemo2 finished!");
    }
}
else
{
    Console.WriteLine("Error: Could not find wide button");
}

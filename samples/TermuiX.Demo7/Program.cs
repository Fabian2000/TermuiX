var xml = """
<Container Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <Text PositionX="2ch" PositionY="0ch"
          ForegroundColor="Yellow" BackgroundColor="Black"
          Style="Bold">
        Justify Demo (Vertical + Align Center)
    </Text>

    <!-- Start (default) -->
    <Text PositionX="1ch" PositionY="2ch" ForegroundColor="Gray">Start</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="Start"
                Width="16%" Height="80%"
                PositionX="0%" PositionY="3ch"
                BackgroundColor="DarkBlue">
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- End -->
    <Text PositionX="18%" PositionY="2ch" ForegroundColor="Gray">End</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="End"
                Width="16%" Height="80%"
                PositionX="16%" PositionY="3ch"
                BackgroundColor="DarkGreen">
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- Center -->
    <Text PositionX="35%" PositionY="2ch" ForegroundColor="Gray">Center</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="Center"
                Width="16%" Height="80%"
                PositionX="33%" PositionY="3ch"
                BackgroundColor="DarkMagenta">
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceBetween -->
    <Text PositionX="51%" PositionY="2ch" ForegroundColor="Gray">Between</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="SpaceBetween"
                Width="16%" Height="80%"
                PositionX="50%" PositionY="3ch"
                BackgroundColor="DarkRed">
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceAround -->
    <Text PositionX="68%" PositionY="2ch" ForegroundColor="Gray">Around</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="SpaceAround"
                Width="16%" Height="80%"
                PositionX="66%" PositionY="3ch"
                BackgroundColor="DarkCyan">
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceEvenly -->
    <Text PositionX="85%" PositionY="2ch" ForegroundColor="Gray">Evenly</Text>
    <StackPanel Direction="Vertical" Align="Center" Justify="SpaceEvenly"
                Width="16%" Height="80%"
                PositionX="83%" PositionY="3ch"
                BackgroundColor="DarkYellow">
        <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

</Container>
""";

var termui = TermuiX.TermuiX.Init();

try
{
    termui.LoadXml(xml);

    while (true)
    {
        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiX.TermuiX.DeInit();
}

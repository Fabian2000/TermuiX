var xml = """
<Container Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <Text PositionX="2ch" PositionY="0ch"
          ForegroundColor="Yellow" BackgroundColor="Black"
          Style="Bold">
        Justify Demo (Horizontal + Align Center)
    </Text>

    <!-- Start (default) -->
    <Text PositionX="2ch" PositionY="2ch" ForegroundColor="Gray">Start</Text>
    <StackPanel Direction="Horizontal" Justify="Start" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="3ch"
                BackgroundColor="DarkBlue">
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkBlue" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- End -->
    <Text PositionX="2ch" PositionY="8ch" ForegroundColor="Gray">End</Text>
    <StackPanel Direction="Horizontal" Justify="End" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="9ch"
                BackgroundColor="DarkGreen">
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- Center -->
    <Text PositionX="2ch" PositionY="14ch" ForegroundColor="Gray">Center</Text>
    <StackPanel Direction="Horizontal" Justify="Center" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="15ch"
                BackgroundColor="DarkMagenta">
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceBetween -->
    <Text PositionX="2ch" PositionY="20ch" ForegroundColor="Gray">SpaceBetween</Text>
    <StackPanel Direction="Horizontal" Justify="SpaceBetween" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="21ch"
                BackgroundColor="DarkRed">
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceAround -->
    <Text PositionX="2ch" PositionY="26ch" ForegroundColor="Gray">SpaceAround</Text>
    <StackPanel Direction="Horizontal" Justify="SpaceAround" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="27ch"
                BackgroundColor="DarkCyan">
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
    </StackPanel>

    <!-- SpaceEvenly -->
    <Text PositionX="2ch" PositionY="32ch" ForegroundColor="Gray">SpaceEvenly</Text>
    <StackPanel Direction="Horizontal" Justify="SpaceEvenly" Align="Center"
                Width="100%" Height="5ch"
                PositionX="0ch" PositionY="33ch"
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

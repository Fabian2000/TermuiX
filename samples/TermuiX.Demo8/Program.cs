var xml = """
<Container Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <Text PositionX="2ch" PositionY="0ch"
          ForegroundColor="Yellow" BackgroundColor="Black"
          Style="Bold">
        Wrap + Percentage Demo
    </Text>

    <!-- Percentage sizing: 60% + 40% = 100%, no gaps -->
    <Text PositionX="2ch" PositionY="2ch" ForegroundColor="Gray">60% + 40% (no gap)</Text>
    <StackPanel Direction="Horizontal"
                Width="100%" Height="3ch"
                PositionX="0ch" PositionY="3ch"
                BackgroundColor="DarkBlue">
        <Button Width="60%" BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">60%</Button>
        <Button Width="40%" MarginLeft="1ch" BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">40% + MarginLeft=1ch</Button>
    </StackPanel>

    <!-- Percentage sizing: 33% + 33% + 34% = 100% -->
    <Text PositionX="2ch" PositionY="6ch" ForegroundColor="Gray">33% + 33% + 34%</Text>
    <StackPanel Direction="Horizontal"
                Width="100%" Height="3ch"
                PositionX="0ch" PositionY="7ch"
                BackgroundColor="DarkBlue">
        <Button Width="33%" BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">33%</Button>
        <Button Width="33%" BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">33%</Button>
        <Button Width="34%" BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">34%</Button>
    </StackPanel>

    <!-- Wrap: 6 buttons that don't fit in one row -->
    <Text PositionX="2ch" PositionY="10ch" ForegroundColor="Gray">Wrap (6 buttons, auto-wrap)</Text>
    <StackPanel Direction="Horizontal" Wrap="true" Align="Center"
                Width="100%" Height="auto"
                PositionX="0ch" PositionY="11ch"
                BackgroundColor="DarkGreen">
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 1</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 2</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 3</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 4</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 5</Button>
        <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Button 6</Button>
    </StackPanel>

    <!-- Wrap vertical: buttons stacking down then wrapping to next column -->
    <Text PositionX="2ch" PositionY="18ch" ForegroundColor="Gray">Vertical Wrap (8 buttons)</Text>
    <StackPanel Direction="Vertical" Wrap="true" Align="Center"
                Width="100%" Height="9ch"
                PositionX="0ch" PositionY="19ch"
                BackgroundColor="DarkMagenta">
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">A</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">B</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">C</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">D</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">E</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">F</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">G</Button>
        <Button BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">H</Button>
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

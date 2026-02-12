var xml = """
<StackPanel Direction="Vertical" Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <Text ForegroundColor="Yellow" Style="Bold"> Min/Max Width/Height + Word-Wrap Demo</Text>

    <!-- Row 1: MinWidth test -->
    <StackPanel Direction="Horizontal" Width="100%" Height="5ch" BackgroundColor="DarkBlue" BorderStyle="Single">
        <Text ForegroundColor="Cyan" Width="30ch"> MinWidth Tests:</Text>
        <Button Width="5ch" MinWidth="12ch" BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Min12ch</Button>
        <Button Width="5ch" MinWidth="15ch" BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">Min15ch</Button>
        <Button Width="30ch" BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">Normal 30ch</Button>
    </StackPanel>

    <!-- Row 2: MaxWidth test -->
    <StackPanel Direction="Horizontal" Width="100%" Height="5ch" BackgroundColor="DarkCyan" BorderStyle="Single">
        <Text ForegroundColor="Cyan" Width="30ch"> MaxWidth Tests:</Text>
        <Button Width="50ch" MaxWidth="15ch" BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">Max15ch</Button>
        <Button Width="50ch" MaxWidth="20ch" BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">Max20ch</Button>
        <Button Width="30ch" BackgroundColor="DarkMagenta" FocusBackgroundColor="Gray" RoundedCorners="true">Normal 30ch</Button>
    </StackPanel>

    <!-- Row 3: MinHeight / MaxHeight test -->
    <StackPanel Direction="Horizontal" Width="100%" Height="auto" BackgroundColor="DarkGreen" BorderStyle="Single">
        <Text ForegroundColor="Cyan" Width="30ch"> MinH/MaxH Tests:</Text>
        <StackPanel Direction="Vertical" Width="20ch" Height="2ch" MinHeight="5ch" BackgroundColor="DarkYellow" BorderStyle="Single">
            <Text ForegroundColor="White"> H=2ch MinH=5ch</Text>
        </StackPanel>
        <StackPanel Direction="Vertical" Width="20ch" Height="20ch" MaxHeight="6ch" BackgroundColor="DarkMagenta" BorderStyle="Single">
            <Text ForegroundColor="White"> H=20ch MaxH=6ch</Text>
        </StackPanel>
    </StackPanel>

    <!-- Row 4: Word-Wrap test -->
    <StackPanel Direction="Horizontal" Width="100%" Height="auto" BackgroundColor="DarkRed" BorderStyle="Single">
        <Text ForegroundColor="Cyan" Width="30ch"> Word-Wrap Tests:</Text>
        <Text Width="25ch" Height="6ch" AllowWrapping="true" BackgroundColor="DarkBlue" ForegroundColor="White">This is a long text that should wrap at word boundaries when it exceeds the width.</Text>
        <Text Width="25ch" Height="6ch" AllowWrapping="false" BackgroundColor="DarkGreen" ForegroundColor="White">This text has wrapping disabled and will be truncated at the edge.</Text>
    </StackPanel>

</StackPanel>
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

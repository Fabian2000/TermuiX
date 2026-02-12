var xml = """
<StackPanel Direction="Vertical" Width="100%" Height="100%" BackgroundColor="#1a1a2e" ForegroundColor="#e0e0e0">

    <Text ForegroundColor="#ffd700" Style="Bold"> TrueColor (RGB) Demo</Text>

    <!-- Gradient row using hex colors -->
    <StackPanel Direction="Horizontal" Width="100%" Height="3ch">
        <Text Width="8ch" BackgroundColor="#ff0000" ForegroundColor="#ffffff"> Red  </Text>
        <Text Width="8ch" BackgroundColor="#ff4400" ForegroundColor="#ffffff"> </Text>
        <Text Width="8ch" BackgroundColor="#ff8800" ForegroundColor="#000000"> </Text>
        <Text Width="8ch" BackgroundColor="#ffcc00" ForegroundColor="#000000"> </Text>
        <Text Width="8ch" BackgroundColor="#ffff00" ForegroundColor="#000000"> Yellow</Text>
        <Text Width="8ch" BackgroundColor="#88ff00" ForegroundColor="#000000"> </Text>
        <Text Width="8ch" BackgroundColor="#00ff00" ForegroundColor="#000000"> Green</Text>
        <Text Width="8ch" BackgroundColor="#00ff88" ForegroundColor="#000000"> </Text>
        <Text Width="8ch" BackgroundColor="#00ffff" ForegroundColor="#000000"> Cyan </Text>
        <Text Width="8ch" BackgroundColor="#0088ff" ForegroundColor="#ffffff"> </Text>
        <Text Width="8ch" BackgroundColor="#0000ff" ForegroundColor="#ffffff"> Blue </Text>
        <Text Width="8ch" BackgroundColor="#8800ff" ForegroundColor="#ffffff"> </Text>
        <Text Width="8ch" BackgroundColor="#ff00ff" ForegroundColor="#ffffff"> Magenta</Text>
    </StackPanel>

    <!-- Theme-colored panels -->
    <StackPanel Direction="Horizontal" Width="100%" Height="auto">
        <StackPanel Direction="Vertical" Width="33%" Height="8ch" BackgroundColor="#16213e" BorderStyle="Single" ForegroundColor="#0f3460">
            <Text ForegroundColor="#e94560" Style="Bold"> Dark Theme</Text>
            <Text ForegroundColor="#e0e0e0"> Navy + Crimson</Text>
            <Button BackgroundColor="#e94560" FocusBackgroundColor="#ff6b81" TextColor="#ffffff">Action</Button>
        </StackPanel>
        <StackPanel Direction="Vertical" Width="33%" Height="8ch" BackgroundColor="#1b4332" BorderStyle="Single" ForegroundColor="#2d6a4f">
            <Text ForegroundColor="#95d5b2" Style="Bold"> Forest Theme</Text>
            <Text ForegroundColor="#d8f3dc"> Green tones</Text>
            <Button BackgroundColor="#52b788" FocusBackgroundColor="#74c69d" TextColor="#081c15">Action</Button>
        </StackPanel>
        <StackPanel Direction="Vertical" Width="34%" Height="8ch" BackgroundColor="#3c1642" BorderStyle="Single" ForegroundColor="#7b2d8e">
            <Text ForegroundColor="#f5b461" Style="Bold"> Purple Theme</Text>
            <Text ForegroundColor="#d6a2e8"> Warm accents</Text>
            <Button BackgroundColor="#f5b461" FocusBackgroundColor="#f8c291" TextColor="#3c1642">Action</Button>
        </StackPanel>
    </StackPanel>

    <!-- Mixed: ConsoleColor names + RGB hex -->
    <StackPanel Direction="Vertical" Width="100%" Height="auto" BackgroundColor="#2b2b2b" BorderStyle="Single">
        <Text ForegroundColor="Cyan"> Mixing ConsoleColor names with #hex:</Text>
        <StackPanel Direction="Horizontal" Width="100%" Height="3ch">
            <Text Width="20ch" BackgroundColor="DarkRed" ForegroundColor="White"> ConsoleColor</Text>
            <Text Width="3ch"> + </Text>
            <Text Width="20ch" BackgroundColor="#ff6347" ForegroundColor="#ffffff"> #FF6347 (Tomato)</Text>
            <Text Width="3ch"> + </Text>
            <Text Width="25ch" BackgroundColor="rgb(70,130,180)" ForegroundColor="#ffffff"> rgb(70,130,180) Steel</Text>
        </StackPanel>
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

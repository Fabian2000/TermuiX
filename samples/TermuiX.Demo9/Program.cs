var xml = """
<StackPanel Direction="Horizontal" Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <!-- LEFT HALF: Vertical with 2 wrapping StackPanels -->
    <StackPanel Direction="Vertical" Width="50%" Height="100%" BackgroundColor="Black">

        <Text ForegroundColor="Yellow" Style="Bold">Left: Vertical + Wrap</Text>

        <!-- First wrapping horizontal StackPanel -->
        <StackPanel Direction="Horizontal" Wrap="true"
                    Width="100%" Height="auto"
                    BackgroundColor="DarkBlue" BorderStyle="Single">
            <Text ForegroundColor="Cyan" Width="100%"> Panel 1 (Wrap=true)</Text>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A1</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A2</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A3</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A4</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A5</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A6</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A7</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">A8</Button>
        </StackPanel>

        <!-- Second wrapping horizontal StackPanel (should be pushed down by first) -->
        <StackPanel Direction="Horizontal" Wrap="true"
                    Width="100%" Height="auto"
                    BackgroundColor="DarkMagenta" BorderStyle="Single">
            <Text ForegroundColor="Cyan" Width="100%"> Panel 2 (Wrap=true)</Text>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B1</Button>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B2</Button>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B3</Button>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B4</Button>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B5</Button>
            <Button BackgroundColor="DarkRed" FocusBackgroundColor="Gray" RoundedCorners="true">B6</Button>
        </StackPanel>

    </StackPanel>

    <!-- RIGHT HALF: Testing scroll axes -->
    <StackPanel Direction="Vertical" Width="50%" Height="100%" BackgroundColor="Black">

        <Text ForegroundColor="Yellow" Style="Bold">Right: Scroll Axis Tests</Text>

        <!-- Scrollable="true" (both axes) + Wrap — overflow vertically -->
        <StackPanel Direction="Horizontal" Wrap="true" Scrollable="true"
                    Width="100%" Height="8ch"
                    BackgroundColor="DarkCyan" BorderStyle="Single">
            <Text ForegroundColor="Yellow" Width="100%"> Scrollable (both)</Text>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C1</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C2</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C3</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C4</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C5</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C6</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C7</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C8</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C9</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C10</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C11</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C12</Button>
        </StackPanel>

        <!-- ScrollY only — vertical overflow, no horizontal scroll -->
        <StackPanel Direction="Vertical" ScrollY="true"
                    Width="100%" Height="8ch"
                    BackgroundColor="DarkGreen" BorderStyle="Single">
            <Text ForegroundColor="Yellow" Width="100%"> ScrollY only</Text>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E1</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E2</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E3</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E4</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E5</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E6</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E7</Button>
            <Button BackgroundColor="DarkGreen" FocusBackgroundColor="Gray" RoundedCorners="true">E8</Button>
        </StackPanel>

        <!-- ScrollX only — horizontal overflow, no vertical scroll -->
        <StackPanel Direction="Horizontal" ScrollX="true"
                    Width="100%" Height="5ch"
                    BackgroundColor="DarkYellow" BorderStyle="Single">
            <Text ForegroundColor="Yellow"> ScrollX only</Text>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F1</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F2</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F3</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F4</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F5</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F6</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F7</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F8</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F9</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">F10</Button>
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

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

    <!-- RIGHT HALF: Testing scroll on both axes -->
    <StackPanel Direction="Vertical" Width="50%" Height="100%" BackgroundColor="Black">

        <Text ForegroundColor="Yellow" Style="Bold">Right: Wrap+Scroll Tests</Text>

        <!-- Top: Wrap=true + Scrollable Y — many items that wrap and overflow vertically -->
        <StackPanel Direction="Horizontal" Wrap="true" Scrollable="true"
                    Width="100%" Height="12ch"
                    BackgroundColor="DarkCyan" BorderStyle="Single">
            <Text ForegroundColor="Yellow" Width="100%"> Wrap + Scroll Y (PageUp/Down)</Text>
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
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C13</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C14</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C15</Button>
            <Button BackgroundColor="DarkCyan" FocusBackgroundColor="Gray" RoundedCorners="true">C16</Button>
        </StackPanel>

        <!-- Bottom: No wrap + Scrollable X — many items that overflow horizontally -->
        <StackPanel Direction="Horizontal" Wrap="false" Scrollable="true"
                    Width="100%" Height="6ch"
                    BackgroundColor="DarkYellow" BorderStyle="Single">
            <Text ForegroundColor="Yellow" Width="100%"> No Wrap + Scroll X (Ctrl+PgUp/PgDn)</Text>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D1</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D2</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D3</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D4</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D5</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D6</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D7</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D8</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D9</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D10</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D11</Button>
            <Button BackgroundColor="DarkYellow" FocusBackgroundColor="Gray" RoundedCorners="true">D12</Button>
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

var xml = """
<StackPanel Direction="Vertical" Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <!-- Styles: applied after parsing, last-wins for conflicts -->
    <Style Group="panel" BackgroundColor="#1e1e2e" ForegroundColor="#cdd6f4" BorderStyle="Single" />
    <Style Group="header" ForegroundColor="#f9e2af" Style="Bold" />
    <Style Group="accent" ForegroundColor="#89b4fa" />
    <Style Group="danger" BackgroundColor="#f38ba8" ForegroundColor="#1e1e2e" />
    <Style Group="success" BackgroundColor="#a6e3a1" ForegroundColor="#1e1e2e" />
    <Style Name="mainTitle" ForegroundColor="#cba6f7" />

    <Text Name="mainTitle"> Style System Demo</Text>

    <!-- panel group applied to multiple containers -->
    <StackPanel Direction="Horizontal" Width="100%" Height="auto">
        <StackPanel Direction="Vertical" Width="33%" Height="8ch" Group="panel">
            <Text Group="header"> Section A</Text>
            <Text Group="accent"> Styled via Group</Text>
        </StackPanel>
        <StackPanel Direction="Vertical" Width="33%" Height="8ch" Group="panel">
            <Text Group="header"> Section B</Text>
            <Text Group="accent"> Same panel style</Text>
        </StackPanel>
        <StackPanel Direction="Vertical" Width="34%" Height="8ch" Group="panel">
            <Text Group="header"> Section C</Text>
            <Text Group="accent"> Consistent look</Text>
        </StackPanel>
    </StackPanel>

    <!-- Multi-group: space-separated groups, last style wins for conflicts -->
    <StackPanel Direction="Horizontal" Width="100%" Height="auto">
        <Button Group="danger" Width="20ch">Delete</Button>
        <Button Group="success" Width="20ch">Save</Button>
        <Button Group="accent" Width="20ch" BackgroundColor="DarkGray" FocusBackgroundColor="Gray">Info (accent)</Button>
    </StackPanel>

    <!-- Name targeting (like CSS #id) -->
    <StackPanel Direction="Vertical" Width="100%" Height="auto" Group="panel">
        <Text Group="header"> Name vs Group Targeting</Text>
        <Text Group="accent"> Styles can target by Name (unique) or Group (multiple widgets)</Text>
        <Text ForegroundColor="Gray"> Group supports multiple values: Group="panel header"</Text>
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

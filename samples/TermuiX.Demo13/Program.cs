using TermuiX.Widgets;

var xml = """
<StackPanel Direction="Vertical" Width="100%" Height="100%" BackgroundColor="Black" ForegroundColor="White">

    <Text ForegroundColor="Yellow" Style="Bold"> TreeView Demo</Text>

    <StackPanel Direction="Horizontal" Width="100%" Height="90%">

        <Container Width="40ch" Height="100%" ScrollY="true" BackgroundColor="DarkBlue">
            <TreeView Name="fileTree" Width="40ch" BackgroundColor="DarkBlue" FocusBackgroundColor="DarkCyan" HighlightBackgroundColor="Gray" HighlightForegroundColor="White">
                <TreeNode Text="Project" Expanded="true">
                    <TreeNode Text="src" Expanded="true">
                        <TreeNode Text="Components">
                            <TreeNode Text="Button.cs" />
                            <TreeNode Text="Input.cs" />
                            <TreeNode Text="Slider.cs" />
                        </TreeNode>
                        <TreeNode Text="Models">
                            <TreeNode Text="User.cs" />
                            <TreeNode Text="Product.cs" />
                        </TreeNode>
                        <TreeNode Text="Program.cs" />
                    </TreeNode>
                    <TreeNode Text="tests">
                        <TreeNode Text="UnitTests.cs" />
                        <TreeNode Text="IntegrationTests.cs" />
                    </TreeNode>
                    <TreeNode Text="docs">
                        <TreeNode Text="README.md" />
                        <TreeNode Text="CHANGELOG.md" />
                        <TreeNode Text="API.md" />
                    </TreeNode>
                    <TreeNode Text=".gitignore" />
                    <TreeNode Text="Program.csproj" />
                </TreeNode>
            </TreeView>
        </Container>

        <StackPanel Direction="Vertical" Width="100%" Height="100%" BackgroundColor="#1a1a2e" BorderStyle="Single">
            <Text ForegroundColor="Cyan" Style="Bold"> Selected Node:</Text>
            <Text Name="selectedLabel" Width="100%" ForegroundColor="White"> (none)</Text>
            <Text />
            <Text ForegroundColor="Cyan" Style="Bold"> Last Event:</Text>
            <Text Name="eventLabel" Width="100%" ForegroundColor="Yellow"> (waiting...)</Text>
            <Text />
            <Text ForegroundColor="DarkGray"> Controls:</Text>
            <Text ForegroundColor="DarkGray"> Up/Down   Navigate</Text>
            <Text ForegroundColor="DarkGray"> Right     Expand</Text>
            <Text ForegroundColor="DarkGray"> Left      Collapse</Text>
            <Text ForegroundColor="DarkGray"> Enter     Toggle + Select</Text>
            <Text ForegroundColor="DarkGray"> Click     Select / Toggle</Text>
        </StackPanel>

    </StackPanel>

</StackPanel>
""";

var termui = TermuiX.TermuiX.Init();

try
{
    termui.LoadXml(xml);

    var tree = termui.GetWidget<TreeView>("fileTree")!;
    var selectedLabel = termui.GetWidget<Text>("selectedLabel")!;
    var eventLabel = termui.GetWidget<Text>("eventLabel")!;

    tree.NodeSelected += (_, node) =>
    {
        selectedLabel.Content = $" {node.Text}";
        eventLabel.Content = $" Selected: {node.Text}";
    };

    tree.NodeToggled += (_, node) =>
    {
        string state = node.IsExpanded ? "Expanded" : "Collapsed";
        eventLabel.Content = $" {state}: {node.Text}";
    };

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

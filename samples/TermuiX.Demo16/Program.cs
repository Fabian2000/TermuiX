using TermuiX;
using TermuiXLib = TermuiX.TermuiX;
using TermuiX.Widgets;

var termui = TermuiXLib.Init();
try
{
    termui.LoadXml(@"
    <StackPanel Direction='Vertical' Width='100%' Height='100%' BackgroundColor='#1e1e2e'>

        <!-- Header -->
        <StackPanel Direction='Horizontal' Width='100%' Height='3ch'
            BackgroundColor='#181825' PaddingLeft='2ch' PaddingRight='2ch'
            Justify='SpaceBetween' Align='Center'>
            <Text ForegroundColor='#89b4fa' Style='Bold'>TermuiX</Text>
            <Text ForegroundColor='#6c7086'>Declarative Terminal UI for .NET</Text>
        </StackPanel>

        <!-- Main Content: 2 columns -->
        <StackPanel Direction='Horizontal' Width='100%' Height='fill'
            PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='1ch' PaddingBottom='1ch'>

            <!-- Left Column: Navigation + Settings -->
            <StackPanel Direction='Vertical' Width='30ch' Height='100%' MarginRight='1ch'>

                <!-- Navigation Title -->
                <Text ForegroundColor='#89b4fa' Style='Bold'>Navigation</Text>
                <Line Width='100%' ForegroundColor='#45475a' />

                <!-- TreeView -->
                <TreeView Name='tree' Width='100%' Height='12ch'
                    ForegroundColor='#cdd6f4' BackgroundColor='#1e1e2e'
                    FocusBackgroundColor='#1e1e2e' FocusForegroundColor='#cdd6f4'
                    HighlightBackgroundColor='#313244' HighlightForegroundColor='#ffffff'>
                    <TreeNode Text='Dashboard' />
                    <TreeNode Text='Users' Expanded='true'>
                        <TreeNode Text='Active' />
                        <TreeNode Text='Invited' />
                        <TreeNode Text='Archived' />
                    </TreeNode>
                    <TreeNode Text='Analytics'>
                        <TreeNode Text='Overview' />
                        <TreeNode Text='Reports' />
                    </TreeNode>
                    <TreeNode Text='Settings'>
                        <TreeNode Text='General' />
                        <TreeNode Text='Security' />
                        <TreeNode Text='Billing' />
                    </TreeNode>
                </TreeView>

                <!-- Settings Title -->
                <Text ForegroundColor='#89b4fa' Style='Bold'>Settings</Text>
                <Line Width='100%' ForegroundColor='#45475a' />

                <!-- Username Input -->
                <Container Width='100%' Height='3ch' MarginTop='1ch'
                    BackgroundColor='#1e1e2e' BorderColor='#45475a'
                    FocusBorderColor='#89b4fa' BorderStyle='Single' RoundedCorners='true'>
                    <Input Name='nameInput' Width='100%' Height='1ch'
                        ForegroundColor='#cdd6f4'
                        FocusForegroundColor='#cdd6f4' />
                </Container>
                <Text ForegroundColor='#a6adc8' MarginTop='-3ch' MarginLeft='2ch'
                    BackgroundColor='#1e1e2e'> Username </Text>

                <!-- Theme Radio Buttons -->
                <Text ForegroundColor='#a6adc8' MarginTop='4ch'>Theme</Text>
                <StackPanel Direction='Horizontal' Width='100%' Height='2ch'>
                    <StackPanel Direction='Vertical' Width='auto' Height='auto'>
                        <RadioButton Name='r1' Group='theme' ForegroundColor='#cdd6f4'
                            FocusForegroundColor='#89b4fa' />
                        <RadioButton Name='r2' Group='theme' ForegroundColor='#cdd6f4'
                            FocusForegroundColor='#89b4fa' />
                    </StackPanel>
                    <StackPanel Direction='Vertical' Width='auto' Height='auto' MarginLeft='1ch'>
                        <Text ForegroundColor='#cdd6f4'>Dark</Text>
                        <Text ForegroundColor='#cdd6f4'>Light</Text>
                    </StackPanel>
                </StackPanel>

                <!-- Checkbox -->
                <StackPanel Direction='Horizontal' Width='100%' Height='1ch' MarginTop='1ch'>
                    <Checkbox Name='cb1' ForegroundColor='#cdd6f4'
                        FocusForegroundColor='#89b4fa' />
                    <Text ForegroundColor='#cdd6f4'>Notifications</Text>
                </StackPanel>

                <!-- Volume Slider -->
                <Text ForegroundColor='#a6adc8' MarginTop='1ch'>Volume</Text>
                <Slider Name='slider1' Width='100%' Height='1ch'
                    Value='75' Min='0' Max='100'
                    ForegroundColor='#89b4fa' BackgroundColor='#1e1e2e'
                    FocusForegroundColor='#89b4fa' FocusBackgroundColor='#313244' />
            </StackPanel>

            <!-- Right Column: Table + Chart -->
            <StackPanel Direction='Vertical' Width='fill' Height='100%'>

                <!-- Table Title -->
                <Text ForegroundColor='#89b4fa' Style='Bold'>Team</Text>
                <Line Width='100%' ForegroundColor='#45475a' />

                <!-- Table -->
                <Table Name='teamTable' Width='100%'
                    ForegroundColor='#cdd6f4' BackgroundColor='#1e1e2e'
                    BorderColor='#45475a'>
                    <TableRow>
                        <TableCell Style='Bold' ForegroundColor='#89b4fa'>Name</TableCell>
                        <TableCell Style='Bold' ForegroundColor='#89b4fa'>Role</TableCell>
                        <TableCell Style='Bold' ForegroundColor='#89b4fa'>Status</TableCell>
                    </TableRow>
                    <TableRow>
                        <TableCell>Alice</TableCell>
                        <TableCell>Lead Developer</TableCell>
                        <TableCell ForegroundColor='#a6e3a1'>Online</TableCell>
                    </TableRow>
                    <TableRow>
                        <TableCell>Bob</TableCell>
                        <TableCell>Designer</TableCell>
                        <TableCell ForegroundColor='#a6e3a1'>Online</TableCell>
                    </TableRow>
                    <TableRow>
                        <TableCell>Charlie</TableCell>
                        <TableCell>Backend Dev</TableCell>
                        <TableCell ForegroundColor='#f9e2af'>Away</TableCell>
                    </TableRow>
                    <TableRow>
                        <TableCell>Diana</TableCell>
                        <TableCell>DevOps</TableCell>
                        <TableCell ForegroundColor='#f38ba8'>Offline</TableCell>
                    </TableRow>
                </Table>

                <!-- Chart Title -->
                <Text ForegroundColor='#89b4fa' Style='Bold' MarginTop='1ch'>Analytics</Text>
                <Line Width='100%' ForegroundColor='#45475a' />

                <!-- Braille Chart -->
                <Chart Name='chart1' Width='100%' Height='fill'
                    ForegroundColor='#cdd6f4' BackgroundColor='#1e1e2e' />
            </StackPanel>
        </StackPanel>

        <!-- Footer -->
        <StackPanel Direction='Horizontal' Width='100%' Height='3ch'
            BackgroundColor='#181825' PaddingLeft='2ch' PaddingRight='2ch'
            Justify='SpaceBetween' Align='Center'>
            <StackPanel Direction='Horizontal' Width='auto' Height='1ch' Align='Center'>
                <Text ForegroundColor='#a6adc8' MarginRight='1ch'>Build </Text>
                <ProgressBar Name='progress' Width='20ch' Height='1ch' Value='0.65'
                    ForegroundColor='#a6e3a1' BackgroundColor='#313244' />
            </StackPanel>
            <StackPanel Direction='Horizontal' Width='auto' Height='1ch' Align='Center'>
                <Text ForegroundColor='#a6adc8' MarginRight='1ch'>Sync </Text>
                <ProgressBar Name='marquee' Width='15ch' Height='1ch'
                    ForegroundColor='#89b4fa' BackgroundColor='#313244' />
            </StackPanel>
        </StackPanel>
    </StackPanel>");

    // Configure Chart data
    var chart1 = termui.GetWidget<Chart>("chart1");
    chart1!.XLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    chart1.AddSeries(new ChartDataSeries
    {
        Label = "Revenue",
        Data = [12, 19, 15, 25, 22, 30, 28, 35, 40, 38, 45, 50],
        Color = Color.Parse("#a6e3a1")
    });
    chart1.AddSeries(new ChartDataSeries
    {
        Label = "Costs",
        Data = [8, 11, 13, 15, 14, 18, 20, 22, 25, 23, 28, 30],
        Color = Color.Parse("#f38ba8")
    });

    // Pre-select Dark theme
    var r1 = termui.GetWidget<RadioButton>("r1");
    r1!.Selected = true;

    // Pre-check notifications
    var cb1 = termui.GetWidget<Checkbox>("cb1");
    cb1!.Checked = true;

    var progress = termui.GetWidget<ProgressBar>("progress");
    progress!.ShowPercentage = false;

    var marquee = termui.GetWidget<ProgressBar>("marquee");
    marquee!.Mode = ProgressBarMode.Marquee;
    double progressValue = 0.0;
    var rng = new Random();
    double[] revenueBase = [12, 19, 15, 25, 22, 30, 28, 35, 40, 38, 45, 50];
    double[] costsBase = [8, 11, 13, 15, 14, 18, 20, 22, 25, 23, 28, 30];
    int chartTick = 0;

    while (true)
    {
        progressValue += 0.002;
        if (progressValue > 1.0) progressValue = 0.0;
        progress.Value = progressValue;

        // Animate chart every ~500ms
        chartTick++;
        if (chartTick % 30 == 0)
        {
            var revenue = chart1.Series[0].Data;
            var costs = chart1.Series[1].Data;
            for (int i = 0; i < 12; i++)
            {
                revenue[i] = revenueBase[i] + rng.Next(-3, 4);
                costs[i] = costsBase[i] + rng.Next(-2, 3);
            }
        }

        termui.Render();
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}

using System.Threading;
using TermuiX.Widgets;

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue" Scrollable="true">
    <Table Name="testTable" PositionX="5ch" PositionY="5ch"
           BorderStyle="Single" BorderColor="White"
           BackgroundColor="DarkBlue" ForegroundColor="White"
           FocusBackgroundColor="Red" FocusForegroundColor="Yellow"
           RoundedCorners="true">
        <TableRow>
            <TableCell Style="Bold">Name</TableCell>
            <TableCell Style="Bold">Age</TableCell>
            <TableCell Style="Bold">City</TableCell>
            <TableCell Style="Bold">Status</TableCell>
        </TableRow>
        <TableRow>
            <TableCell>Alice
&amp;
Bob</TableCell>
            <TableCell>25</TableCell>
            <TableCell>New York</TableCell>
            <TableCell>Active</TableCell>
        </TableRow>
        <TableRow>
            <TableCell>Bob
&amp;
Doris</TableCell>
            <TableCell>30</TableCell>
            <TableCell>London</TableCell>
            <TableCell>Pending</TableCell>
        </TableRow>
        <TableRow>
            <TableCell>Charlie</TableCell>
            <TableCell>35</TableCell>
            <TableCell>Paris</TableCell>
            <TableCell>Inactive</TableCell>
        </TableRow>
    </Table>
</Container>
""";

var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

var table = termui.GetWidget<Table>("testTable");

if (table is not null)
{
    bool running = true;

    try
    {
        while (running)
        {
            termui.Render();

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Q)
                {
                    running = false;
                }
            }

            Thread.Sleep(16);
        }
    }
    finally
    {
        TermuiX.TermuiX.DeInit();
        Console.WriteLine("\n\nDemo2 finished!");
    }
}
else
{
    Console.WriteLine("Error: Could not find table");
}

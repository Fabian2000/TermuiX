using System.Threading;
using TermuiX.Widgets;

// Demo for Component Model - Reusable UI components via Container.Add(xml)

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" Style="Bold">
        Demo 4: Component Model - Add reusable components dynamically
    </Text>

    <!-- Container for dynamically added components -->
    <Container Name="componentContainer" PositionX="2ch" PositionY="4ch"
               Width="60ch" Height="15ch"
               BorderStyle="Single" BorderColor="Cyan" RoundedCorners="true"
               BackgroundColor="Black" Scrollable="true" CanFocus="true">
    </Container>

    <Button Name="addBtn" PositionX="2ch" PositionY="20ch"
            BorderColor="Green" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Add Component
    </Button>

    <Button Name="clearBtn" PositionX="20ch" PositionY="20ch"
            BorderColor="Red" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Clear All
    </Button>

    <Button Name="exitBtn" PositionX="35ch" PositionY="20ch"
            BorderColor="White" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Exit
    </Button>
</Container>
""";

var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

var componentContainer = termui.GetWidget<Container>("componentContainer");
var addBtn = termui.GetWidget<Button>("addBtn");
var clearBtn = termui.GetWidget<Button>("clearBtn");
var exitBtn = termui.GetWidget<Button>("exitBtn");

bool running = true;
int componentCount = 0;

// Simple component template
string GetSimpleCard(int index)
{
    int yPos = index * 4;
    string[] colors = { "White", "Cyan", "Green", "Yellow", "Magenta" };
    string color = colors[index % colors.Length];

    return $"""
        <Container PositionX="1ch" PositionY="{yPos}ch"
                   Width="56ch" Height="3ch"
                   BorderStyle="Single" BorderColor="{color}"
                   BackgroundColor="DarkGray">
            <Text PositionX="1ch" PositionY="0ch"
                  ForegroundColor="{color}" Style="Bold"
                  Width="52ch">
                Component #{index + 1} - This is a reusable component
            </Text>
        </Container>
        """;
}

if (componentContainer is not null && addBtn is not null && clearBtn is not null && exitBtn is not null)
{
    addBtn.Click += (s, e) =>
    {
        string cardXml = GetSimpleCard(componentCount);
        componentContainer.Add(cardXml);
        componentCount++;
    };

    clearBtn.Click += (s, e) =>
    {
        componentContainer.Clear();
        componentCount = 0;
    };

    exitBtn.Click += (s, e) =>
    {
        running = false;
    };
}

try
{
    while (running)
    {
        termui.Render();
        Thread.Sleep(16);
    }
}
finally
{
    TermuiX.TermuiX.DeInit();
    Console.WriteLine("\n\nDemo4 finished!");
}

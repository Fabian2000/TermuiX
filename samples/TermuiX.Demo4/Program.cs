using System.Threading;
using TermuiX.Widgets;

// Demo for Component Model - Reusable UI components via Container.Add(xml)

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" Style="Bold">
        Demo 4: Component Model - Press 'Add' to add components dynamically
    </Text>

    <!-- Container for dynamically added components -->
    <Container Name="componentContainer" PositionX="2ch" PositionY="4ch"
               Width="70ch" Height="15ch"
               BorderStyle="Single" BorderColor="Cyan" RoundedCorners="true"
               BackgroundColor="Black" Scrollable="true">
    </Container>

    <Button Name="addBtn" PositionX="2ch" PositionY="20ch"
            BorderColor="Green" RoundedCorners="true"
            BackgroundColor="DarkBlue">
        Add Component
    </Button>

    <Button Name="clearBtn" PositionX="20ch" PositionY="20ch"
            BorderColor="Red" RoundedCorners="true"
            BackgroundColor="DarkBlue">
        Clear All
    </Button>

    <Button Name="exitBtn" PositionX="35ch" PositionY="20ch"
            BorderColor="White" RoundedCorners="true"
            BackgroundColor="DarkBlue">
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

// Define reusable component templates
string GetCardComponent(int index, string title, string content)
{
    int yPos = index * 5;
    return $"""
        <Container PositionX="2ch" PositionY="{yPos}ch"
                   Width="30ch" Height="4ch"
                   BorderStyle="Single" BorderColor="White" RoundedCorners="true"
                   BackgroundColor="DarkGray">
            <Text PositionX="1ch" PositionY="1ch"
                  ForegroundColor="Yellow" Style="Bold">
                {title}
            </Text>
            <Text PositionX="1ch" PositionY="2ch"
                  ForegroundColor="White">
                {content}
            </Text>
        </Container>
        """;
}

string GetButtonGroupComponent(int yPos)
{
    return $"""
        <Container PositionX="35ch" PositionY="{yPos}ch"
                   Width="30ch" Height="4ch"
                   BorderStyle="Single" BorderColor="Cyan" RoundedCorners="true"
                   BackgroundColor="DarkGray">
            <Button PositionX="1ch" PositionY="1ch"
                    BorderColor="Cyan" RoundedCorners="true"
                    BackgroundColor="Black">
                Option A
            </Button>
            <Button PositionX="12ch" PositionY="1ch"
                    BorderColor="Cyan" RoundedCorners="true"
                    BackgroundColor="Black">
                Option B
            </Button>
        </Container>
        """;
}

string GetFormComponent(int yPos)
{
    return $"""
        <Container PositionX="2ch" PositionY="{yPos}ch"
                   Width="63ch" Height="5ch"
                   BorderStyle="Single" BorderColor="Green" RoundedCorners="true"
                   BackgroundColor="DarkGray">
            <Text PositionX="1ch" PositionY="1ch"
                  ForegroundColor="Green" Style="Bold">
                User Form Component
            </Text>
            <Checkbox PositionX="1ch" PositionY="3ch" />
            <Text PositionX="3ch" PositionY="3ch"
                  ForegroundColor="White">
                Accept Terms
            </Text>
            <Button PositionX="20ch" PositionY="3ch"
                    BorderColor="Green" RoundedCorners="true"
                    BackgroundColor="Black">
                Submit
            </Button>
        </Container>
        """;
}

if (componentContainer != null && addBtn != null && clearBtn != null && exitBtn != null)
{
    addBtn.Click += (s, e) =>
    {
        componentCount++;
        int yPos = (componentCount - 1) * 6;

        // Add different components based on count
        if (componentCount % 3 == 1)
        {
            // Add a card component
            string cardXml = GetCardComponent(componentCount - 1, $"Card #{componentCount}", "Reusable card component");
            componentContainer.Add(cardXml);
        }
        else if (componentCount % 3 == 2)
        {
            // Add a button group
            string buttonGroupXml = GetButtonGroupComponent(yPos);
            componentContainer.Add(buttonGroupXml);
        }
        else
        {
            // Add a form component
            string formXml = GetFormComponent(yPos);
            componentContainer.Add(formXml);
        }
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

using System.Threading;
using TermuiX.Widgets;

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <!-- Main Content -->
    <Text PositionX="15ch" PositionY="10ch"
          ForegroundColor="White">
        Click the burger menu to open sidebar
    </Text>

    <!-- Burger Menu Button -->
    <Button Name="burgerBtn" PositionX="2ch" PositionY="1ch"
            BorderColor="White" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        ≡
    </Button>

    <!-- Sidebar Container -->
    <Container Name="sidebar" PositionX="-50ch" PositionY="0ch"
               Width="40ch" Height="100%"
               BackgroundColor="Black"
               BorderStyle="Single" BorderColor="White">

        <Text PositionX="2ch" PositionY="2ch"
              ForegroundColor="Yellow" Style="Bold">
            Menu
        </Text>

        <Button Name="menuBtn1" PositionX="2ch" PositionY="5ch"
                Width="35ch" BorderColor="White" RoundedCorners="true">
            Dashboard
        </Button>

        <Button Name="menuBtn2" PositionX="2ch" PositionY="8ch"
                Width="35ch" BorderColor="White" RoundedCorners="true">
            Settings
        </Button>

        <Button Name="menuBtn3" PositionX="2ch" PositionY="11ch"
                Width="35ch" BorderColor="White" RoundedCorners="true">
            Profile
        </Button>

        <Button Name="closeBtn" PositionX="2ch" PositionY="14ch"
                Width="35ch" BorderColor="Red" RoundedCorners="true">
            ✕ Close
        </Button>
    </Container>
</Container>
""";

var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

var burgerBtn = termui.GetWidget<Button>("burgerBtn");
var sidebar = termui.GetWidget<Container>("sidebar");
var closeBtn = termui.GetWidget<Button>("closeBtn");

bool running = true;
bool sidebarOpen = false;
bool animating = false;
int targetPosition = -50;
int currentPosition = -50;

// Animation speed (characters per frame)
const int animationSpeed = 3;

if (burgerBtn != null && sidebar != null && closeBtn != null)
{
    burgerBtn.Click += (s, e) =>
    {
        if (!animating)
        {
            sidebarOpen = true;
            targetPosition = 0;
            animating = true;
        }
    };

    closeBtn.Click += (s, e) =>
    {
        if (!animating)
        {
            sidebarOpen = false;
            targetPosition = -50;
            animating = true;
        }
    };
}

try
{
    while (running)
    {
        // Animation logic
        if (animating && sidebar != null)
        {
            if (currentPosition < targetPosition)
            {
                currentPosition = Math.Min(currentPosition + animationSpeed, targetPosition);
            }
            else if (currentPosition > targetPosition)
            {
                currentPosition = Math.Max(currentPosition - animationSpeed, targetPosition);
            }

            sidebar.PositionX = $"{currentPosition}ch";

            if (currentPosition == targetPosition)
            {
                animating = false;
            }
        }

        termui.Render();
        Thread.Sleep(16);
    }
}
finally
{
    TermuiX.TermuiX.DeInit();
    Console.WriteLine("\n\nDemo3 finished!");
}

using System.Threading;
using TermuiX;
using TermuiX.Widgets;

// Demo for Auto-Scroll to Bottom (like a Messenger)

var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" Style="Bold">
        Demo 5: Auto-Scroll Messenger - Press 'Send' to add messages
    </Text>

    <!-- Scrollable message container -->
    <Container Name="messageContainer" PositionX="2ch" PositionY="4ch"
               Width="70ch" Height="18ch"
               BorderStyle="Single" BorderColor="Cyan" RoundedCorners="true"
               BackgroundColor="Black" Scrollable="true">
    </Container>

    <Button Name="sendBtn" PositionX="2ch" PositionY="23ch"
            BorderColor="Green" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Send Message
    </Button>

    <Button Name="topBtn" PositionX="20ch" PositionY="23ch"
            BorderColor="Yellow" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Scroll to Top
    </Button>

    <Button Name="bottomBtn" PositionX="38ch" PositionY="23ch"
            BorderColor="Yellow" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Scroll to Bottom
    </Button>

    <Button Name="exitBtn" PositionX="60ch" PositionY="23ch"
            BorderColor="White" RoundedCorners="true"
            BackgroundColor="DarkBlue" TextColor="White">
        Exit
    </Button>
</Container>
""";

var termui = TermuiX.TermuiX.Init();
termui.LoadXml(xml);

var messageContainer = termui.GetWidget<Container>("messageContainer");
var sendBtn = termui.GetWidget<Button>("sendBtn");
var topBtn = termui.GetWidget<Button>("topBtn");
var bottomBtn = termui.GetWidget<Button>("bottomBtn");
var exitBtn = termui.GetWidget<Button>("exitBtn");

bool running = true;
int messageCount = 0;

string GetMessageComponent(int index, string sender, string message, ConsoleColor color)
{
    int yPos = index * 5;
    return $"""
        <Container PositionX="2ch" PositionY="{yPos}ch"
                   Width="63ch" Height="4ch"
                   BorderStyle="Single" BorderColor="{color}" RoundedCorners="true"
                   BackgroundColor="DarkGray">
            <Text PositionX="1ch" PositionY="1ch"
                  ForegroundColor="{color}" Style="Bold">
                {sender}
            </Text>
            <Text PositionX="1ch" PositionY="2ch"
                  ForegroundColor="White">
                {message}
            </Text>
        </Container>
        """;
}

void ScrollToBottom()
{
    if (messageContainer != null && messageCount > 0)
    {
        // Calculate total height of all messages
        int totalHeight = messageCount * 5;
        int containerHeight = 18 - 2; // Container height minus border

        // Calculate max scroll position
        int maxScroll = Math.Max(0, totalHeight - containerHeight);

        // Set scroll position to bottom
        ((IWidget)messageContainer).ScrollOffsetY = maxScroll;
    }
}

if (messageContainer != null && sendBtn != null && topBtn != null && bottomBtn != null && exitBtn != null)
{
    sendBtn.Click += (s, e) =>
    {
        messageCount++;

        // Alternate between user colors
        string sender = messageCount % 2 == 1 ? "Alice" : "Bob";
        ConsoleColor color = messageCount % 2 == 1 ? ConsoleColor.Cyan : ConsoleColor.Green;
        string[] messages =
        {
            "Hello! How are you?",
            "I'm doing great, thanks!",
            "What are you working on?",
            "Testing the auto-scroll feature!",
            "That's awesome!",
            "This messenger demo is working well.",
            "Yes, the component model is very useful.",
            "I agree! Keep up the good work.",
            "Thanks! Let me add more messages.",
            "Sure, go ahead!"
        };

        string message = messages[(messageCount - 1) % messages.Length];

        // Add message component
        string messageXml = GetMessageComponent(messageCount - 1, sender, message, color);
        messageContainer.Add(messageXml);

        // Auto-scroll to bottom after adding message
        ScrollToBottom();
    };

    topBtn.Click += (s, e) =>
    {
        if (messageContainer != null)
        {
            ((IWidget)messageContainer).ScrollOffsetY = 0;
        }
    };

    bottomBtn.Click += (s, e) =>
    {
        ScrollToBottom();
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
    Console.WriteLine("\n\nDemo5 finished!");
}

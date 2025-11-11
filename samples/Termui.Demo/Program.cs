using Termui.Widgets;

// XML-based UI definition with named widgets for event binding
var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue" Scrollable="true">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Welcome to Termui!
    </Text>

    <Input Name="nameInput" PositionX="5ch" PositionY="5ch"
           Width="30ch" Height="1ch" Placeholder="Enter your name..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Input Name="passwordInput" PositionX="5ch" PositionY="7ch"
           Width="30ch" Height="1ch" IsPassword="true" Placeholder="Enter password..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Input Name="messageInput" PositionX="5ch" PositionY="10ch"
           Width="50ch" Height="5ch" Multiline="true" Placeholder="Enter message..."
           BackgroundColor="DarkGray" ForegroundColor="White"
           FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Checkbox Name="agreeCheckbox" PositionX="5ch" PositionY="16ch"
              BackgroundColor="DarkBlue" ForegroundColor="White"
              FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Text PositionX="7ch" PositionY="16ch"
          ForegroundColor="White" BackgroundColor="DarkBlue">
        I agree to the terms
    </Text>

    <Container PositionX="60ch" PositionY="5ch" Width="30ch" Height="6ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               Scrollable="true">
        <Text PositionX="0ch" PositionY="0ch"
              ForegroundColor="Yellow" BackgroundColor="DarkBlue">
            Delivery Method:
        </Text>

        <RadioButton Name="deliveryStandard" PositionX="2ch" PositionY="2ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="2ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Standard (3-5 days)
        </Text>

        <RadioButton Name="deliveryExpress" PositionX="2ch" PositionY="3ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="3ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Express (1-2 days)
        </Text>

        <RadioButton Name="deliveryOvernight" PositionX="2ch" PositionY="4ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="4ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Overnight
        </Text>

        <RadioButton Name="deliverySameDay" PositionX="2ch" PositionY="5ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="5ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Same Day
        </Text>

        <RadioButton Name="deliveryScheduled" PositionX="2ch" PositionY="6ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="6ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Scheduled
        </Text>

        <RadioButton Name="deliveryInStore" PositionX="2ch" PositionY="7ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="7ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            In-Store Pickup
        </Text>

        <RadioButton Name="deliveryLocker" PositionX="2ch" PositionY="8ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="8ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Locker Pickup
        </Text>
    </Container>

    <Container PositionX="60ch" PositionY="13ch" Width="30ch" Height="6ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White">
        <Text PositionX="0ch" PositionY="0ch"
              ForegroundColor="Yellow" BackgroundColor="DarkBlue">
            Payment Method:
        </Text>

        <RadioButton Name="paymentCard" PositionX="2ch" PositionY="2ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="2ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Credit/Debit Card
        </Text>

        <RadioButton Name="paymentPaypal" PositionX="2ch" PositionY="3ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="3ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            PayPal
        </Text>

        <RadioButton Name="paymentBank" PositionX="2ch" PositionY="4ch"
                     BackgroundColor="DarkBlue" ForegroundColor="White"
                     FocusBackgroundColor="Gray" FocusForegroundColor="White" />
        <Text PositionX="4ch" PositionY="4ch"
              ForegroundColor="White" BackgroundColor="DarkBlue">
            Bank Transfer
        </Text>
    </Container>

    <Button Name="submitButton" PositionX="5ch" PositionY="18ch"
            BorderColor="White" TextColor="Cyan"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Submit
    </Button>

    <Button Name="exitButton" PositionX="20ch" PositionY="18ch"
            BorderColor="White" TextColor="Magenta"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue">
        Exit
    </Button>

    <Text Name="outputText" PositionX="5ch" PositionY="22ch"
          Width="100ch" Height="10ch"
          ForegroundColor="Green" BackgroundColor="DarkBlue">
    </Text>

    <Text PositionX="5ch" PositionY="25ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Progress Bar:
    </Text>
    <ProgressBar Name="progressBar" PositionX="20ch" PositionY="25ch"
                 Width="40ch" Mode="Progress" Value="0.65"
                 ShowPercentage="true"
                 ForegroundColor="Green" BackgroundColor="DarkBlue" />

    <Text PositionX="5ch" PositionY="27ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Marquee:
    </Text>
    <ProgressBar Name="marqueeBar" PositionX="20ch" PositionY="27ch"
                 Width="40ch" Mode="Marquee"
                 ForegroundColor="Cyan" BackgroundColor="DarkBlue" />

    <Chart Name="salesChart" PositionX="5ch" PositionY="30ch"
           Width="80ch" Height="12ch"
           ShowLegend="true" ShowAxes="true"
           ForegroundColor="White" BackgroundColor="DarkBlue" />
</Container>
""";

// Initialize Termui and load XML (parsed once, kept in DOM)
var termui = Termui.Termui.Init();
termui.LoadXml(xml);

// Bind events to named widgets
var nameInput = termui.GetWidget<Input>("nameInput");
var passwordInput = termui.GetWidget<Input>("passwordInput");
var messageInput = termui.GetWidget<Input>("messageInput");
var agreeCheckbox = termui.GetWidget<Checkbox>("agreeCheckbox");
var deliveryStandard = termui.GetWidget<RadioButton>("deliveryStandard");
var deliveryExpress = termui.GetWidget<RadioButton>("deliveryExpress");
var deliveryOvernight = termui.GetWidget<RadioButton>("deliveryOvernight");
var submitButton = termui.GetWidget<Button>("submitButton");
var exitButton = termui.GetWidget<Button>("exitButton");
var outputText = termui.GetWidget<Text>("outputText");
var salesChart = termui.GetWidget<Termui.Widgets.Chart>("salesChart");

// Setup chart with sample data
if (salesChart is not null)
{
    // Sales data series
    var series1 = new Termui.Widgets.ChartDataSeries
    {
        Label = "Product A",
        Color = ConsoleColor.Green,
        Data = [10, 15, 13, 17, 22, 28, 35, 42, 38, 45, 52, 58]
    };

    var series2 = new Termui.Widgets.ChartDataSeries
    {
        Label = "Product B",
        Color = ConsoleColor.Cyan,
        Data = [5, 8, 12, 18, 25, 20, 22, 28, 35, 40, 38, 42]
    };

    var series3 = new Termui.Widgets.ChartDataSeries
    {
        Label = "Product C",
        Color = ConsoleColor.Yellow,
        Data = [20, 18, 22, 19, 25, 30, 28, 32, 35, 38, 42, 48]
    };

    salesChart.AddSeries(series1);
    salesChart.AddSeries(series2);
    salesChart.AddSeries(series3);
    salesChart.XLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
}

if (exitButton is not null && submitButton is not null)
{
    bool running = true;
    DateTime? messageDisplayedAt = null;
    exitButton.Click += (sender, e) => running = false;

    // Keyboard shortcuts
    termui.Shortcut += (sender, key) =>
    {
        if (key.Key == ConsoleKey.S) // Ctrl+S
        {
            if (submitButton is not null)
            {
                termui.SetFocus(submitButton);
            }
        }
    };

    if (submitButton is not null && nameInput is not null && passwordInput is not null && messageInput is not null && agreeCheckbox is not null && outputText is not null)
    {
        submitButton.Click += (sender, e) =>
        {
            string delivery = "None";
            if (deliveryStandard?.Selected == true) delivery = "Standard";
            else if (deliveryExpress?.Selected == true) delivery = "Express";
            else if (deliveryOvernight?.Selected == true) delivery = "Overnight";

            outputText.Content = $"Submitted!\nName: {nameInput.Value}\nPassword: {passwordInput.Value}\nMessage: {messageInput.Value}\nAgreed: {agreeCheckbox.Checked}\nDelivery: {delivery}";
            messageDisplayedAt = DateTime.Now;
        };
    }

    try
    {
        // User-controlled loop (like ImGui)
        while (running)
        {
            // Clear message after 2 seconds
            if (messageDisplayedAt.HasValue && (DateTime.Now - messageDisplayedAt.Value).TotalSeconds >= 2)
            {
                outputText!.Content = string.Empty;
                messageDisplayedAt = null;
            }

            // Render frame (input is processed automatically)
            termui.Render();

            // 60 FPS (16ms interval)
            Thread.Sleep(16);
        }
    }
    finally
    {
        // Clean up
        Termui.Termui.DeInit();
        Console.WriteLine("\n\nDemo finished!");
    }
}
else
{
    Console.WriteLine("Error: Could not find widgets");
}

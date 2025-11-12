using TermuiX.Widgets;

// XML-based UI definition with named widgets for event binding
var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
    <Container Name="mainPage" Width="100%" Height="100%" BackgroundColor="DarkBlue" Scrollable="true" Visible="true">
    <Text PositionX="2ch" PositionY="1ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue"
          Style="Bold">
        W e l c o m e - t o - T e r m u i X !
    </Text>

    <Text PositionX="2ch" PositionY="3ch"
          ForegroundColor="Cyan" BackgroundColor="DarkBlue"
          Style="Italic">
        Italic Demo
    </Text>

    <Text PositionX="20ch" PositionY="3ch"
          ForegroundColor="Green" BackgroundColor="DarkBlue"
          Style="BoldItalic">
        Bold Italic
    </Text>

    <Text PositionX="35ch" PositionY="3ch"
          ForegroundColor="Magenta" BackgroundColor="DarkBlue"
          Style="Underline">
        Underlined
    </Text>

    <Text PositionX="50ch" PositionY="3ch"
          ForegroundColor="Red" BackgroundColor="DarkBlue"
          Style="Strikethrough">
        Strikethrough
    </Text>

    <Container PositionX="5ch" PositionY="5ch" Width="32ch" Height="3ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               RoundedCorners="true">
        <Input Name="nameInput" PositionX="0ch" PositionY="0ch"
               Width="30ch" Height="1ch" Placeholder="Enter your name..."
               BackgroundColor="DarkBlue" ForegroundColor="White"
               FocusBackgroundColor="DarkBlue" FocusForegroundColor="White" />
    </Container>

    <Container PositionX="5ch" PositionY="9ch" Width="32ch" Height="3ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               RoundedCorners="true">
        <Input Name="passwordInput" PositionX="0ch" PositionY="0ch"
               Width="30ch" Height="1ch" IsPassword="true" Placeholder="Enter password..."
               BackgroundColor="DarkBlue" ForegroundColor="White"
               FocusBackgroundColor="DarkBlue" FocusForegroundColor="White" />
    </Container>

    <Container PositionX="5ch" PositionY="13ch" Width="52ch" Height="7ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               RoundedCorners="true">
        <Input Name="messageInput" PositionX="0ch" PositionY="0ch"
               Width="50ch" Height="5ch" Multiline="true" Placeholder="Enter message..."
               BackgroundColor="DarkBlue" ForegroundColor="White"
               FocusBackgroundColor="DarkBlue" FocusForegroundColor="White" />
    </Container>

    <Checkbox Name="agreeCheckbox" PositionX="5ch" PositionY="21ch"
              BackgroundColor="DarkBlue" ForegroundColor="White"
              FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Text PositionX="7ch" PositionY="21ch"
          ForegroundColor="White" BackgroundColor="DarkBlue">
        I agree to the terms
    </Text>

    <Container PositionX="60ch" PositionY="5ch" Width="30ch" Height="6ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               RoundedCorners="true" Scrollable="true">
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
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White"
               RoundedCorners="true">
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

    <Button Name="submitButton" PositionX="5ch" PositionY="23ch"
            BorderColor="White" TextColor="Cyan"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue" RoundedCorners="true">
        Submit
    </Button>

    <Button Name="exitButton" PositionX="20ch" PositionY="23ch"
            BorderColor="White" TextColor="Magenta"
            FocusBorderColor="Green" FocusTextColor="White"
            BackgroundColor="DarkBlue" RoundedCorners="true">
        Exit
    </Button>

    <Text Name="outputText" PositionX="5ch" PositionY="27ch"
          Width="100ch" Height="10ch"
          ForegroundColor="Green" BackgroundColor="DarkBlue">
    </Text>

    <Text PositionX="5ch" PositionY="30ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Progress Bar:
    </Text>
    <ProgressBar Name="progressBar" PositionX="20ch" PositionY="30ch"
                 Width="40ch" Mode="Progress" Value="0.65"
                 ShowPercentage="true"
                 ForegroundColor="Green" BackgroundColor="DarkBlue" />

    <Text PositionX="5ch" PositionY="32ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Marquee:
    </Text>
    <ProgressBar Name="marqueeBar" PositionX="20ch" PositionY="32ch"
                 Width="40ch" Mode="Marquee"
                 ForegroundColor="Cyan" BackgroundColor="DarkBlue" />

    <Text PositionX="5ch" PositionY="34ch"
          ForegroundColor="Yellow" BackgroundColor="DarkBlue">
        Volume:
    </Text>
    <Slider Name="volumeSlider" PositionX="20ch" PositionY="34ch"
            Width="40ch" Height="1ch"
            Min="0" Max="100" Value="75" Step="5"
            ShowValue="true"
            BackgroundColor="DarkBlue" ForegroundColor="Cyan"
            FocusBackgroundColor="Gray" FocusForegroundColor="White" />

    <Chart Name="salesChart" PositionX="5ch" PositionY="37ch"
           Width="80ch" Height="12ch"
           ShowLegend="true" ShowAxes="true"
           ForegroundColor="White" BackgroundColor="DarkBlue" />
    </Container>

    <Container Name="confirmModal" Width="100%" Height="100%" BackgroundColor="DarkGray" Visible="false">
        <Container Name="modalDialog" PositionX="30ch" PositionY="10ch" Width="60ch" Height="12ch"
                   BackgroundColor="White" ForegroundColor="Black" BorderStyle="Double"
                   RoundedCorners="true">
            <Text Name="modalTitle" PositionX="2ch" PositionY="1ch"
                  ForegroundColor="Blue" BackgroundColor="White">
                Confirmation
            </Text>
            <Text Name="modalMessage" PositionX="2ch" PositionY="3ch" Width="54ch" Height="2ch"
                  ForegroundColor="Black" BackgroundColor="White">
                Do you really want to submit?
            </Text>
            <Button Name="modalYes" PositionX="15ch" PositionY="6ch" Width="12ch"
                    BorderColor="DarkGreen" TextColor="DarkGreen"
                    FocusBorderColor="Green" FocusTextColor="White"
                    BackgroundColor="White" FocusBackgroundColor="DarkGreen"
                    RoundedCorners="true">
                Yes
            </Button>
            <Button Name="modalNo" PositionX="35ch" PositionY="6ch" Width="12ch"
                    BorderColor="DarkRed" TextColor="DarkRed"
                    FocusBorderColor="Red" FocusTextColor="White"
                    BackgroundColor="White" FocusBackgroundColor="DarkRed"
                    RoundedCorners="true">
                No
            </Button>
        </Container>
    </Container>
</Container>
""";

// Initialize TermuiX and load XML (parsed once, kept in DOM)
var termui = TermuiX.TermuiX.Init();
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
var salesChart = termui.GetWidget<TermuiX.Widgets.Chart>("salesChart");

// Modal widgets
var mainPage = termui.GetWidget<Container>("mainPage");
var confirmModal = termui.GetWidget<Container>("confirmModal");
var modalDialog = termui.GetWidget<Container>("modalDialog");
var modalYes = termui.GetWidget<Button>("modalYes");
var modalNo = termui.GetWidget<Button>("modalNo");

// Center modal dialog
if (modalDialog is not null)
{
    int termWidth = Console.WindowWidth;
    int termHeight = Console.WindowHeight;
    int dialogWidth = 60; // From XML: Width="60ch"
    int dialogHeight = 12; // From XML: Height="12ch"

    int centerX = (termWidth - dialogWidth) / 2;
    int centerY = (termHeight - dialogHeight) / 2;

    modalDialog.PositionX = $"{centerX}ch";
    modalDialog.PositionY = $"{centerY}ch";
}

// Setup chart with sample data
if (salesChart is not null)
{
    // Sales data series
    var series1 = new TermuiX.Widgets.ChartDataSeries
    {
        Label = "Product A",
        Color = ConsoleColor.Green,
        Data = [10, 15, 13, 17, 22, 28, 35, 42, 38, 45, 52, 58]
    };

    var series2 = new TermuiX.Widgets.ChartDataSeries
    {
        Label = "Product B",
        Color = ConsoleColor.Cyan,
        Data = [5, 8, 12, 18, 25, 20, 22, 28, 35, 40, 38, 42]
    };

    var series3 = new TermuiX.Widgets.ChartDataSeries
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

    if (submitButton is not null && nameInput is not null && passwordInput is not null && messageInput is not null && agreeCheckbox is not null && outputText is not null && confirmModal is not null && mainPage is not null && modalYes is not null && modalNo is not null)
    {
        submitButton.Click += (sender, e) =>
        {
            // Show confirmation modal
            mainPage.Visible = false;
            confirmModal.Visible = true;
            termui.SetFocus(modalYes);
        };

        // Handle modal Yes button
        modalYes.Click += (sender, e) =>
        {
            // Hide modal
            confirmModal.Visible = false;
            mainPage.Visible = true;

            // Submit
            string delivery = "None";
            if (deliveryStandard?.Selected == true) delivery = "Standard";
            else if (deliveryExpress?.Selected == true) delivery = "Express";
            else if (deliveryOvernight?.Selected == true) delivery = "Overnight";

            outputText.Content = $"Submitted!\nName: {nameInput.Value}\nPassword: {passwordInput.Value}\nMessage: {messageInput.Value}\nAgreed: {agreeCheckbox.Checked}\nDelivery: {delivery}";
            messageDisplayedAt = DateTime.Now;

            // Return focus to submit button
            termui.SetFocus(submitButton);
        };

        // Handle modal No button
        modalNo.Click += (sender, e) =>
        {
            // Hide modal
            confirmModal.Visible = false;
            mainPage.Visible = true;

            // Return focus to submit button
            termui.SetFocus(submitButton);
        };
    }

    try
    {
        int lastTermWidth = Console.WindowWidth;
        int lastTermHeight = Console.WindowHeight;

        // User-controlled loop (like ImGui)
        while (running)
        {
            // Clear message after 2 seconds
            if (messageDisplayedAt.HasValue && (DateTime.Now - messageDisplayedAt.Value).TotalSeconds >= 2)
            {
                outputText!.Content = string.Empty;
                messageDisplayedAt = null;
            }

            // Re-center modal if terminal size changed
            if (modalDialog is not null && (Console.WindowWidth != lastTermWidth || Console.WindowHeight != lastTermHeight))
            {
                lastTermWidth = Console.WindowWidth;
                lastTermHeight = Console.WindowHeight;

                const int dialogWidth = 60;
                const int dialogHeight = 12;
                int centerX = (lastTermWidth - dialogWidth) / 2;
                int centerY = (lastTermHeight - dialogHeight) / 2;

                modalDialog.PositionX = $"{centerX}ch";
                modalDialog.PositionY = $"{centerY}ch";
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
        TermuiX.TermuiX.DeInit();
        Console.WriteLine("\n\nDemo finished!");
    }
}
else
{
    Console.WriteLine("Error: Could not find widgets");
}

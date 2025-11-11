using Termui.Widgets;

// XML-based UI definition with named widgets for event binding
var xml = """
<Container Width="100%" Height="100%" BackgroundColor="DarkBlue">
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

    <Container PositionX="60ch" PositionY="5ch" Width="30ch" Height="10ch"
               BackgroundColor="DarkBlue" BorderStyle="Single" ForegroundColor="White">
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

if (exitButton is not null && submitButton is not null)
{
    bool running = true;
    DateTime? messageDisplayedAt = null;
    exitButton.Click += (sender, e) => running = false;

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

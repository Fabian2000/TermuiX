using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.AiChat.Components;

public class ChatArea
{
    private readonly TermuiXLib _termui;
    private readonly OllamaClient _ollama;

    private Button? _sidebarButton;
    private Button? _modelButton;
    private Button? _menuButton;

    // Model dropdown
    private Container? _modelDropdown;
    private StackPanel? _modelList;
    private readonly List<Button> _modelButtons = [];
    private string _selectedModel = "";
    private bool _modelClickWired;
    private bool _dropdownWasOpen;

    // Menu dropdown
    private Container? _menuDropdown;
    private Button? _menuDeleteBtn;
    private bool _menuClickWired;
    private bool _menuWasOpen;

    // Empty state placeholder
    private StackPanel? _emptyState;

    // Chat input
    private Input? _chatInput;
    private Button? _stopButton;
    private bool _chatInputWired;

    // Chat state
    private StackPanel? _messagesContainer;
    private StackPanel? _messagesInner;
    private readonly List<ChatMessage> _history = [];
    private Text? _currentAssistantText;
    private string _currentAssistantContent = "";
    private bool _isStreaming;
    private int _msgCounter;

    public string SelectedModel => _selectedModel;
    public bool IsModelDropdownOpen => _modelDropdown?.Visible == true;
    public bool IsMenuDropdownOpen => _menuDropdown?.Visible == true;
    public bool IsStreaming => _isStreaming;

    public event EventHandler<string>? ModelChanged;
    public event EventHandler? DeleteChatRequested;
    public event EventHandler<string>? MessageSent;

    public ChatArea(TermuiXLib termui, OllamaClient ollama)
    {
        _termui = termui;
        _ollama = ollama;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='chatArea' Direction='Vertical' Width='fill' Height='100%' BackgroundColor='#1e1e2e'>
    <StackPanel Name='chatTopBar' Direction='Horizontal' Width='100%' Height='3ch' Justify='SpaceBetween'>
        <StackPanel Name='chatTopLeft' Direction='Horizontal' Height='3ch'>
            <Button Name='btnChatSidebar' Width='6ch' Height='3ch' BorderStyle='None' PaddingLeft='0ch' PaddingRight='0ch' PaddingTop='1ch' PaddingBottom='0ch' BackgroundColor='#1e1e2e' FocusBackgroundColor='#313244' TextColor='#cdd6f4' FocusTextColor='#cdd6f4' Visible='false'>▌█</Button>
            <Button Name='btnModel' Height='3ch' BorderStyle='None' PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='1ch' PaddingBottom='0ch' BackgroundColor='#1e1e2e' FocusBackgroundColor='#313244' TextColor='#6c7086' FocusTextColor='#cdd6f4'>loading…</Button>
        </StackPanel>
        <StackPanel Name='chatTopRight' Direction='Horizontal' Height='3ch'>
            <Button Name='btnMenu' Width='6ch' Height='3ch' BorderStyle='None' PaddingLeft='0ch' PaddingRight='1ch' PaddingTop='1ch' PaddingBottom='0ch' BackgroundColor='#1e1e2e' FocusBackgroundColor='#313244' TextColor='#cdd6f4' FocusTextColor='#cdd6f4'>⋯</Button>
        </StackPanel>
    </StackPanel>
    <StackPanel Direction='Horizontal' Width='100%' Justify='Center' BackgroundColor='#1e1e2e'>
        <Line Orientation='Horizontal' Width='80%' ForegroundColor='#313244' />
    </StackPanel>
    <Container Name='chatMessagesWrap' Width='100%' Height='fill' BackgroundColor='#1e1e2e'>
        <StackPanel Name='chatMessages' Direction='Vertical' Width='100%' Height='100%' ScrollY='true' Align='Center' BackgroundColor='#1e1e2e'>
            <StackPanel Name='chatMessagesInner' Direction='Vertical' Width='80%'>
            </StackPanel>
        </StackPanel>
        <StackPanel Name='emptyState' Direction='Vertical' Width='100%' Height='100%' Justify='Center' Align='Center'>
            <Text BackgroundColor='#1e1e2e' ForegroundColor='#6c7086'>Start a conversation by typing a message below.</Text>
        </StackPanel>
    </Container>
    <StackPanel Name='chatInputBar' Direction='Horizontal' Width='100%' Height='7ch' Justify='Center' Align='Center' BackgroundColor='#1e1e2e'>
        <StackPanel Name='chatInputBorder' Direction='Horizontal' Width='80%' Height='5ch'
            BackgroundColor='#1e1e2e' BorderStyle='Single' RoundedCorners='true'
            ForegroundColor='#45475a' PaddingLeft='1ch' PaddingRight='1ch'>
            <Input Name='chatInput' Width='fill' Height='100%' Multiline='true' SubmitKey='Enter'
                BackgroundColor='#1e1e2e' ForegroundColor='#cdd6f4'
                FocusBackgroundColor='#1e1e2e' FocusForegroundColor='#cdd6f4'
                CursorColor='#cdd6f4' PlaceholderColor='#6c7086' Placeholder='Send a message... (CTRL+⏎ for new line)'
                BorderStyle='None' />
            <Button Name='btnStop' Width='6ch' Height='1ch' BorderStyle='None'
                PaddingLeft='0ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
                BackgroundColor='#1e1e2e' FocusBackgroundColor='#1e1e2e'
                TextColor='#f38ba8' FocusTextColor='#cdd6f4' Visible='false'>■ Stop</Button>
        </StackPanel>
    </StackPanel>
</StackPanel>";
    }

    public string BuildOverlayXml()
    {
        return @"
<Container Name='modelDropdown' Width='24ch' Height='10ch' PositionX='7ch' PositionY='3ch'
    BackgroundColor='#313244' ForegroundColor='#cdd6f4' BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <StackPanel Name='modelList' Direction='Vertical' Width='22ch' Height='8ch' ScrollY='true'>
    </StackPanel>
</Container>
<Container Name='menuDropdown' Width='18ch' Height='3ch' PositionX='0ch' PositionY='3ch'
    BackgroundColor='#313244' ForegroundColor='#cdd6f4' BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='btnMenuDelete' Width='16ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#313244' FocusBackgroundColor='#45475a'
        TextColor='#f38ba8' FocusTextColor='#f38ba8' TextAlign='Left'>✕ Delete Chat</Button>
</Container>";
    }

    public void Initialize()
    {
        _sidebarButton = _termui.GetWidget<Button>("btnChatSidebar");
        _modelButton = _termui.GetWidget<Button>("btnModel");
        _menuButton = _termui.GetWidget<Button>("btnMenu");
        _emptyState = _termui.GetWidget<StackPanel>("emptyState");
        _chatInput = _termui.GetWidget<Input>("chatInput");
        _messagesContainer = _termui.GetWidget<StackPanel>("chatMessages");
        _messagesInner = _termui.GetWidget<StackPanel>("chatMessagesInner");

        var dropdown = _termui.GetWidget<Container>("modelDropdown");
        if (dropdown is not null && _modelDropdown is null)
        {
            _modelDropdown = dropdown;
            _modelList = _termui.GetWidget<StackPanel>("modelList");
        }

        if (_modelButton is not null && !_modelClickWired)
        {
            _modelClickWired = true;
            _modelButton.Click += (_, _) => ToggleModelDropdown();
        }

        var menuDd = _termui.GetWidget<Container>("menuDropdown");
        if (menuDd is not null && _menuDropdown is null)
        {
            _menuDropdown = menuDd;
            _menuDeleteBtn = _termui.GetWidget<Button>("btnMenuDelete");
        }

        if (_menuButton is not null && !_menuClickWired)
        {
            _menuClickWired = true;
            _menuButton.Click += (_, _) => ToggleMenuDropdown();
        }

        if (_menuDeleteBtn is not null)
        {
            _menuDeleteBtn.Click += (_, _) =>
            {
                CloseMenuDropdown();
                DeleteChatRequested?.Invoke(this, EventArgs.Empty);
            };
        }

        _stopButton = _termui.GetWidget<Button>("btnStop");
        if (_stopButton is not null)
            _stopButton.Click += (_, _) => StopStreaming();

        if (_chatInput is not null && !_chatInputWired)
        {
            _chatInputWired = true;

            _chatInput.EnterPressed += (_, text) =>
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                if (_isStreaming) return;
                _chatInput.Value = "";
                MessageSent?.Invoke(this, text.Trim());
            };
        }
    }

    public async Task LoadModelsAsync()
    {
        var models = await _ollama.ListModelsAsync();

        if (models.Count == 0)
        {
            _selectedModel = "";
            if (_modelButton is not null)
                _modelButton.Text = "no models";
            return;
        }

        // Select first model
        _selectedModel = models[0];
        if (_modelButton is not null)
            _modelButton.Text = FormatModelName(_selectedModel);

        // Populate dropdown
        if (_modelList is null) return;

        foreach (var model in models)
        {
            var escaped = System.Security.SecurityElement.Escape(FormatModelName(model));
            var xml = $@"<Button Name='mdl_{model.Replace(':', '_').Replace('.', '_')}' Width='22ch' Height='1ch' BorderStyle='None'
                PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#cdd6f4' FocusTextColor='#cdd6f4' TextAlign='Left'>{escaped}</Button>";
            _modelList.Add(xml);

            var btnName = $"mdl_{model.Replace(':', '_').Replace('.', '_')}";
            var btn = _termui.GetWidget<Button>(btnName);
            if (btn is not null)
            {
                var m = model;
                btn.Click += (_, _) => SelectModel(m);
                _modelButtons.Add(btn);
            }
        }
    }

    private void SelectModel(string model)
    {
        _selectedModel = model;
        if (_modelButton is not null)
            _modelButton.Text = FormatModelName(model);
        CloseModelDropdown();
        ModelChanged?.Invoke(this, model);
    }

    private void ToggleModelDropdown()
    {
        if (_modelDropdown is null) return;

        // ClosePopups() already closed the dropdown before this click handler runs.
        // If it was open before ClosePopups, this click was meant to close it — don't reopen.
        if (_dropdownWasOpen)
        {
            _dropdownWasOpen = false;
            return;
        }

        // Dynamic X: get sidebar computed width + line (1ch) + button offset
        int sidebarW = 0;
        var sidebarPanel = _termui.GetWidget<StackPanel>("sidebar");
        var sidebarLine = _termui.GetWidget<Line>("sidebarLine");
        if (sidebarPanel is not null && sidebarPanel.Visible)
            sidebarW += (sidebarPanel as IWidget).ComputedWidth;
        if (sidebarLine is not null && sidebarLine.Visible)
            sidebarW += (sidebarLine as IWidget).ComputedWidth;

        // btnChatSidebar width (6ch if visible, 0 if not)
        int btnOffset = (_sidebarButton?.Visible == true) ? 6 : 0;

        _modelDropdown.PositionX = $"{sidebarW + btnOffset}ch";
        _modelDropdown.PositionY = "3ch";
        _modelDropdown.Visible = true;

        if (_modelButtons.Count > 0)
            _termui.SetFocus(_modelButtons[0]);
    }

    public void CloseModelDropdown()
    {
        if (_modelDropdown is null) return;
        _dropdownWasOpen = _modelDropdown.Visible;
        _modelDropdown.Visible = false;
    }

    public bool ContainsDropdownWidget(IWidget widget)
        => _modelButtons.Contains(widget) || widget == _menuDeleteBtn;

    // Menu dropdown

    private void ToggleMenuDropdown()
    {
        if (_menuDropdown is null) return;

        if (_menuWasOpen)
        {
            _menuWasOpen = false;
            return;
        }

        // Position right-aligned: console width - dropdown width
        int dropdownW = 18;
        _menuDropdown.PositionX = $"{Console.WindowWidth - dropdownW}ch";
        _menuDropdown.PositionY = "3ch";
        _menuDropdown.Visible = true;

        if (_menuDeleteBtn is not null)
            _termui.SetFocus(_menuDeleteBtn);
    }

    public void CloseMenuDropdown()
    {
        if (_menuDropdown is null) return;
        _menuWasOpen = _menuDropdown.Visible;
        _menuDropdown.Visible = false;
    }

    private static string FormatModelName(string model)
    {
        // Strip ":latest" suffix for cleaner display
        if (model.EndsWith(":latest", StringComparison.OrdinalIgnoreCase))
            return model[..^7];
        return model;
    }

    public void AddUserMessage(string text)
    {
        if (_messagesInner is null) return;

        SetHasMessages(true);
        _history.Add(new ChatMessage("user", text));

        var id = $"msg_{_msgCounter++}";
        var escaped = System.Security.SecurityElement.Escape(text);
        var xml = $@"<StackPanel Direction='Vertical' Width='100%' Align='End' MarginTop='1ch'>
            <StackPanel Direction='Vertical' MaxWidth='80%'
                BackgroundColor='#313244' BorderStyle='Single' RoundedCorners='true'
                ForegroundColor='#45475a' PaddingLeft='1ch' PaddingRight='1ch'>
                <Text Name='{id}' Width='100%' BackgroundColor='#313244' ForegroundColor='#cdd6f4'>{escaped}</Text>
            </StackPanel>
        </StackPanel>";
        _messagesInner.Add(xml);
        ScrollToBottom();
    }

    public void BeginAssistantMessage()
    {
        if (_messagesInner is null) return;

        bool wasAtBottom = IsAtBottom();
        _isStreaming = true;
        _currentAssistantContent = "";

        var id = $"msg_{_msgCounter++}";
        var xml = $@"<StackPanel Direction='Vertical' Width='100%' MarginTop='1ch'>
            <Text Name='{id}' Width='100%' BackgroundColor='#1e1e2e' ForegroundColor='#6c7086'>●●●</Text>
        </StackPanel>";
        _messagesInner.Add(xml);

        _currentAssistantText = _termui.GetWidget<Text>(id);

        // Disable input while streaming, show stop button
        if (_chatInput is not null)
        {
            _chatInput.Placeholder = "AI is responding…";
            _chatInput.Disabled = true;
        }
        if (_stopButton is not null)
            _stopButton.Visible = true;
        if (wasAtBottom) ScrollToBottom();
    }

    public void AppendAssistantToken(string token)
    {
        if (_currentAssistantText is null) return;

        bool wasAtBottom = IsAtBottom();

        // On first real token, switch from indicator to content color
        if (_currentAssistantContent.Length == 0)
            _currentAssistantText.ForegroundColor = global::TermuiX.Color.Parse("#cdd6f4");

        _currentAssistantContent += token;
        _currentAssistantText.Content = _currentAssistantContent;

        // Recalculate height based on wrapped line count
        var w = (IWidget)_currentAssistantText;
        int availableWidth = w.Parent?.ComputedWidth ?? 80;
        if (availableWidth <= 0) availableWidth = 80;
        int lines = 1;
        int col = 0;
        foreach (var rune in _currentAssistantContent.EnumerateRunes())
        {
            if (rune.Value == '\n') { lines++; col = 0; continue; }
            int rw = Text.GetRuneDisplayWidth(rune);
            if (col + rw > availableWidth) { lines++; col = 0; }
            col += rw;
        }
        _currentAssistantText.Height = $"{lines}ch";
        if (wasAtBottom) ScrollToBottom();
    }

    private bool IsAtBottom()
    {
        if (_messagesContainer is null || _messagesInner is null) return true;
        var container = (IWidget)_messagesContainer;
        var inner = (IWidget)_messagesInner;
        long maxScroll = Math.Max(0, inner.ComputedHeight - container.ComputedHeight);
        // Use generous tolerance — ComputedHeight may be stale during streaming
        return maxScroll == 0 || container.ScrollOffsetY >= maxScroll - 5;
    }

    private void ScrollToBottom()
    {
        if (_messagesContainer is null || _messagesInner is null) return;
        var container = (IWidget)_messagesContainer;
        var inner = (IWidget)_messagesInner;
        long maxScroll = Math.Max(0, inner.ComputedHeight - container.ComputedHeight);
        container.ScrollOffsetY = maxScroll;
    }

    public void EndAssistantMessage()
    {
        _isStreaming = false;
        if (_currentAssistantContent.Length > 0)
            _history.Add(new ChatMessage("assistant", _currentAssistantContent));
        _currentAssistantText = null;
        _currentAssistantContent = "";

        // Re-enable input after streaming, hide stop button
        if (_stopButton is not null)
            _stopButton.Visible = false;
        if (_chatInput is not null)
        {
            _chatInput.Placeholder = "Send a message... (CTRL+⏎ for new line)";
            _chatInput.Disabled = false;
            _termui.SetFocus(_chatInput);
        }
    }

    public void AddAssistantMessage(string text)
    {
        if (_messagesInner is null) return;

        SetHasMessages(true);
        _history.Add(new ChatMessage("assistant", text));

        var id = $"msg_{_msgCounter++}";
        var escaped = System.Security.SecurityElement.Escape(text);
        var xml = $@"<StackPanel Direction='Vertical' Width='100%' MarginTop='1ch'>
            <Text Name='{id}' Width='100%' BackgroundColor='#1e1e2e' ForegroundColor='#cdd6f4'>{escaped}</Text>
        </StackPanel>";
        _messagesInner.Add(xml);
        ScrollToBottom();
    }

    public void LoadHistory(List<ChatMessage> messages)
    {
        ClearChat();
        foreach (var msg in messages)
        {
            if (msg.Role == "user")
                AddUserMessage(msg.Content);
            else if (msg.Role == "assistant")
                AddAssistantMessage(msg.Content);
        }
    }

    private static readonly ChatMessage SystemPrompt = new("system",
        "Respond in plain text only. Do not use markdown, bullet points, headers, bold, italic, code blocks, or any formatting. Just write natural sentences.");

    public async Task SendAndStreamAsync(string userText)
    {
        if (string.IsNullOrEmpty(_selectedModel)) return;

        AddUserMessage(userText);
        BeginAssistantMessage();

        var messages = new List<ChatMessage> { SystemPrompt };
        messages.AddRange(_history);

        try
        {
            await foreach (var token in _ollama.ChatStreamAsync(_selectedModel, messages))
            {
                AppendAssistantToken(token);
            }
        }
        finally
        {
            EndAssistantMessage();
        }
    }

    public List<ChatMessage> GetHistory() => _history;

    public void ClearChat()
    {
        _history.Clear();
        _currentAssistantText = null;
        _currentAssistantContent = "";
        _isStreaming = false;

        // Remove all message widgets from inner panel
        if (_messagesInner is not null)
        {
            var toRemove = new List<IWidget>();
            foreach (var child in ((IWidget)_messagesInner).Children)
            {
                if (child != (IWidget?)_emptyState)
                    toRemove.Add(child);
            }
            foreach (var child in toRemove)
                _messagesInner.Remove(child);
        }

        SetHasMessages(false);
    }

    public void SetHasMessages(bool hasMessages)
    {
        if (_emptyState is not null)
            _emptyState.Visible = !hasMessages;

        if (!hasMessages && _messagesContainer is not null)
            ((IWidget)_messagesContainer).ScrollOffsetY = 0;
    }

    public void StopStreaming()
    {
        if (!_isStreaming) return;
        _ollama.CancelStream();
    }

    public void FocusInput()
    {
        if (_chatInput is not null)
            _termui.SetFocus(_chatInput);
    }


    public Button? GetSidebarButton() => _sidebarButton;
    public Button? GetModelButton() => _modelButton;
    public Button? GetMenuButton() => _menuButton;
}

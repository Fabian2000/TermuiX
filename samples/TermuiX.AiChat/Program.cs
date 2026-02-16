using TermuiX.AiChat;
using TermuiX.AiChat.Components;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

var termui = TermuiXLib.Init();
using var ollama = new OllamaClient();
var store = new ChatStore();

try
{
    termui.LoadXml("""
    <Container Name="appRoot" Width="100%" Height="100%">
        <StackPanel Name="root" Direction="Horizontal" Width="100%" Height="100%">
        </StackPanel>
    </Container>
    """);

    var appRoot = termui.GetWidget<Container>("appRoot");
    var root = termui.GetWidget<StackPanel>("root");

    var sidebar = new Sidebar(termui);
    root?.Add(sidebar.BuildXml());
    sidebar.Initialize();

    root?.Add("<Line Name='sidebarLine' Orientation='Vertical' Height='100%' ForegroundColor='#313244' />");

    var chatArea = new ChatArea(termui, ollama);
    root?.Add(chatArea.BuildXml());
    chatArea.Initialize();

    // Confirm dialog
    var confirmDialog = new ConfirmDialog(termui);

    // Overlays (added to appRoot so they float above everything)
    appRoot?.Add(sidebar.BuildContextMenuXml());
    appRoot?.Add(chatArea.BuildOverlayXml());
    appRoot?.Add(confirmDialog.BuildXml());
    sidebar.Initialize(); // Re-initialize to pick up context menu widgets
    chatArea.Initialize(); // Re-initialize to pick up dropdown widgets
    confirmDialog.Initialize();

    // Load models from Ollama (prefer last used model)
    _ = chatArea.LoadModelsAsync(store.GetPreferredModel());

    // Load persisted chats into sidebar (oldest first so Insert(1,...) puts newest on top)
    var chats = store.ListChats();
    for (int i = chats.Count - 1; i >= 0; i--)
    {
        var chat = chats[i];
        if (chat.Messages.Count == 0)
        {
            store.DeleteChat(chat.Id);
            continue;
        }
        sidebar.AddChat(chat.Id, chat.Title);
    }

    void ClosePopups()
    {
        sidebar.CloseContextMenu();
        sidebar.CloseRenameOverlay();
        chatArea.CloseModelDropdown();
        chatArea.CloseMenuDropdown();
    }

    // Close all popups on any mouse click (fires before widget click handlers)
    termui.MouseClick += (_, e) =>
    {
        if (e.EventType == TermuiX.MouseEventType.LeftButtonPressed ||
            e.EventType == TermuiX.MouseEventType.RightButtonPressed)
        {
            ClosePopups();
        }
    };

    // Close popups when focus moves via keyboard (Tab)
    termui.FocusChanged += (_, e) =>
    {
        if (e.Reason == TermuiX.FocusChangeReason.Keyboard)
            ClosePopups();
    };

    // Persist model preference when user switches model
    chatArea.ModelChanged += (_, model) => store.SetPreferredModel(model);

    // Helper: save current chat to disk
    string? currentChatId = null;

    void SaveCurrentChat()
    {
        if (currentChatId is null) return;
        var history = chatArea.GetHistory();
        if (history.Count == 0) return;

        var existing = store.LoadChat(currentChatId);
        if (existing is not null)
        {
            existing.Messages = history;
            existing.Model = chatArea.SelectedModel;
            store.SaveChat(existing);
        }
    }

    // Sidebar context menu delete → confirm dialog
    sidebar.ChatDeleted += (_, chatId) =>
    {
        confirmDialog.Show("Delete this chat?", () =>
        {
            sidebar.RemoveChat(chatId);
            store.DeleteChat(chatId);
            if (currentChatId == chatId)
            {
                chatArea.ClearChat();
                currentChatId = null;
                sidebar.SetActiveChat(null);
            }
        });
    };

    // Menu (⋯) delete → confirm dialog
    chatArea.DeleteChatRequested += (_, _) =>
    {
        if (currentChatId is null) return;
        var idToDelete = currentChatId;
        confirmDialog.Show("Delete this chat?", () =>
        {
            sidebar.RemoveChat(idToDelete);
            store.DeleteChat(idToDelete);
            chatArea.ClearChat();
            currentChatId = null;
            sidebar.SetActiveChat(null);
        });
    };

    // Sidebar rename → persist
    sidebar.ChatRenamed += (_, chatId) =>
    {
        var button = termui.GetWidget<Button>($"chat_{chatId}");
        if (button is not null)
            store.RenameChat(chatId, button.Text);
    };

    // Sidebar chat selected → switch chat
    sidebar.ChatSelected += (_, chatId) =>
    {
        if (chatId == currentChatId) return;
        if (chatArea.IsStreaming) return;

        // Save current chat before switching
        SaveCurrentChat();

        // Load selected chat
        var data = store.LoadChat(chatId);
        if (data is not null)
        {
            chatArea.LoadHistory(data.Messages);
            currentChatId = chatId;
            sidebar.SetActiveChat(chatId);
            chatArea.FocusInput();
        }
    };

    // Sidebar toggle
    var chatSidebarButton = chatArea.GetSidebarButton();

    async Task ToggleSidebar()
    {
        if (sidebar.IsOpen)
        {
            await sidebar.CollapseAsync();
            if (chatSidebarButton is not null) chatSidebarButton.Visible = true;
        }
        else
        {
            await sidebar.ExpandAsync();
            if (chatSidebarButton is not null) chatSidebarButton.Visible = false;
        }
    }

    var sidebarButton = sidebar.GetSidebarButton();
    if (sidebarButton is not null)
        sidebarButton.Click += async (_, _) => await ToggleSidebar();
    if (chatSidebarButton is not null)
        chatSidebarButton.Click += async (_, _) => await ToggleSidebar();

    // Chat message sending
    chatArea.MessageSent += async (_, text) =>
    {
        if (chatArea.IsStreaming) return;

        // Create new chat if none active
        if (currentChatId is null)
        {
            var chatData = store.CreateChat(chatArea.SelectedModel);
            currentChatId = chatData.Id;
            var title = text.Length > 30 ? text[..29] + "…" : text;
            store.RenameChat(chatData.Id, title);
            sidebar.AddChat(chatData.Id, title);
            sidebar.SetActiveChat(chatData.Id);
        }
        else
        {
            sidebar.BumpChat(currentChatId);
        }

        await chatArea.SendAndStreamAsync(text);

        // Save after response completes
        SaveCurrentChat();
    };

    // New chat button
    var newChatButton = sidebar.GetNewChatButton();
    if (newChatButton is not null)
    {
        newChatButton.Click += (_, _) =>
        {
            if (chatArea.IsStreaming) return;
            SaveCurrentChat();
            chatArea.ClearChat();
            currentChatId = null;
            sidebar.SetActiveChat(null);
            chatArea.FocusInput();
        };
    }

    chatArea.FocusInput();

    while (true)
    {
        termui.Render(skipUnchanged: true);
        await Task.Delay(16);
    }
}
finally
{
    TermuiXLib.DeInit();
}

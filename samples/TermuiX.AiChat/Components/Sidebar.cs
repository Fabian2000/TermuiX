using System.Security;
using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.AiChat.Components;

public class Sidebar
{
    private const int SidebarWidth = 30;
    private const int MaxTitleLength = 24;
    private const int AnimationStep = 2;
    private const int AnimationDelayMs = 16;

    private readonly TermuiXLib _termui;
    private StackPanel? _sidebarPanel;
    private Button? _sidebarButton;
    private Button? _newChatButton;
    private StackPanel? _chatList;
    private Line? _sidebarLine;

    // Context menu
    private Container? _contextMenu;
    private Button? _ctxRename;
    private Button? _ctxDelete;
    private string? _contextMenuTargetId;

    // Rename overlay
    private Container? _renameOverlay;
    private Input? _renameInput;
    private readonly Dictionary<string, string> _chatTitles = new();
    public bool IsRenameOpen => _renameOverlay?.Visible is true;

    // Active chat highlight
    private string? _activeChatId;

    // Animation state
    public bool IsOpen { get; private set; } = true;

    // Chat selection
    public event EventHandler<string>? ChatSelected;
    public event EventHandler<string>? ChatDeleted;
    public event EventHandler<string>? ChatRenamed;

    public Sidebar(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='sidebar' Direction='Vertical' Width='30ch' Height='100%' BackgroundColor='#181825'>
    <StackPanel Direction='Horizontal' Width='100%' Height='3ch' Justify='SpaceBetween' PaddingLeft='1ch' PaddingRight='0ch'>
        <Text BackgroundColor='#181825' ForegroundColor='#cdd6f4' MarginTop='1ch'>▟▜█▛▙\n▜▟█▙▛</Text>
        <Button Name='btnSidebar' Width='6ch' Height='3ch' BorderStyle='None' PaddingLeft='0ch' PaddingRight='0ch' PaddingTop='1ch' PaddingBottom='0ch' BackgroundColor='#181825' FocusBackgroundColor='#313244' TextColor='#cdd6f4' FocusTextColor='#cdd6f4'>▌█</Button>
    </StackPanel>
    <StackPanel Name='chatListPanel' Direction='Vertical' Width='100%' Height='fill' ScrollY='true'>
        <Button Name='btnNewChat' Width='100%' Height='3ch' BorderStyle='None' PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='1ch' PaddingBottom='0ch' BackgroundColor='#181825' FocusBackgroundColor='#313244' TextColor='#cdd6f4' FocusTextColor='#cdd6f4' TextAlign='Left'>✎ New Chat</Button>
    </StackPanel>
</StackPanel>";
    }

    public string BuildContextMenuXml()
    {
        return @"
<Container Name='chatContextMenu' Width='16ch' Height='4ch' PositionX='0ch' PositionY='0ch'
    BackgroundColor='#313244' ForegroundColor='#cdd6f4' BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='ctxChatRename' Width='14ch' Height='1ch' PositionX='0ch' PositionY='0ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#313244' FocusBackgroundColor='#45475a' TextColor='#cdd6f4' FocusTextColor='#cdd6f4' TextAlign='Left'>✎ Rename</Button>
    <Button Name='ctxChatDelete' Width='14ch' Height='1ch' PositionX='0ch' PositionY='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#313244' FocusBackgroundColor='#45475a' TextColor='#f38ba8' FocusTextColor='#f38ba8' TextAlign='Left'>✕ Delete</Button>
</Container>
<Container Name='renameOverlay' Width='30ch' Height='3ch' PositionX='0ch' PositionY='0ch'
    BackgroundColor='#181825' Visible='false'>
    <Input Name='renameInput' Width='100%' Height='3ch' BackgroundColor='#181825' ForegroundColor='#cdd6f4'
        FocusBackgroundColor='#181825' FocusForegroundColor='#cdd6f4'
        BorderColor='#89b4fa' FocusBorderColor='#89b4fa' CursorColor='#cdd6f4'
        PaddingLeft='1ch' PaddingRight='1ch' />
</Container>";
    }

    public void Initialize()
    {
        _sidebarPanel = _termui.GetWidget<StackPanel>("sidebar");
        _sidebarButton = _termui.GetWidget<Button>("btnSidebar");
        _newChatButton = _termui.GetWidget<Button>("btnNewChat");
        _chatList = _termui.GetWidget<StackPanel>("chatListPanel");

        _sidebarLine = _termui.GetWidget<Line>("sidebarLine");

        _contextMenu = _termui.GetWidget<Container>("chatContextMenu");
        _ctxRename = _termui.GetWidget<Button>("ctxChatRename");
        _ctxDelete = _termui.GetWidget<Button>("ctxChatDelete");

        _renameOverlay = _termui.GetWidget<Container>("renameOverlay");
        _renameInput = _termui.GetWidget<Input>("renameInput");

        if (_ctxRename is not null)
            _ctxRename.Click += (_, _) => { OpenRenameOverlay(); CloseContextMenu(); };
        if (_ctxDelete is not null)
            _ctxDelete.Click += (_, _) => { OnContextDelete(); CloseContextMenu(); };

        if (_renameInput is not null)
        {
            _renameInput.EnterPressed += (_, newTitle) =>
            {
                if (_contextMenuTargetId is not null && !string.IsNullOrWhiteSpace(newTitle))
                {
                    var id = _contextMenuTargetId;
                    var button = _termui.GetWidget<Button>($"chat_{id}");
                    if (button is not null)
                    {
                        var truncated = TruncateTitle(newTitle);
                        button.Text = truncated;
                        _chatTitles[id] = newTitle;
                    }
                    ChatRenamed?.Invoke(this, id);
                }
                CloseRenameOverlay();
            };
            _renameInput.EscapePressed += (_, _) => CloseRenameOverlay();
        }
    }

    public void AddChat(string id, string title)
    {
        if (_chatList is null) return;

        _chatTitles[id] = title;
        var truncated = TruncateTitle(title);

        var xml = $@"<Button Name='chat_{id}' Width='100%' Height='3ch' BorderStyle='None'
            PaddingLeft='1ch' PaddingRight='1ch' PaddingTop='1ch' PaddingBottom='0ch'
            BackgroundColor='#181825' FocusBackgroundColor='#313244'
            TextColor='#a6adc8' FocusTextColor='#cdd6f4' TextAlign='Left'>{SecurityElement.Escape(truncated)}</Button>";

        // Insert after btnNewChat (index 1), so newest chats appear at top
        _chatList.Insert(1, xml);

        var button = _termui.GetWidget<Button>($"chat_{id}");
        if (button is not null)
        {
            button.Click += (_, _) => ChatSelected?.Invoke(this, id);
            button.RightClick += (_, args) => ShowContextMenu(args.X, args.Y, id);
        }
    }

    public void BumpChat(string id)
    {
        if (_chatList is null) return;

        var button = _termui.GetWidget<Button>($"chat_{id}");
        if (button is null) return;

        // Remove from current position and re-insert at top (after btnNewChat)
        _chatList.Remove(button);
        _chatList.Insert(1, button);
    }

    public void RemoveChat(string id)
    {
        if (_chatList is null) return;

        var button = _termui.GetWidget<Button>($"chat_{id}");
        if (button is not null)
            _chatList.Remove(button);
    }

    // Context menu

    private void ShowContextMenu(int x, int y, string chatId)
    {
        if (_contextMenu is null) return;

        _contextMenuTargetId = chatId;

        const int menuW = 16, menuH = 4;
        if (x + menuW > Console.WindowWidth) x = Console.WindowWidth - menuW;
        if (y + menuH > Console.WindowHeight) y = Console.WindowHeight - menuH;

        _contextMenu.PositionX = $"{x}ch";
        _contextMenu.PositionY = $"{y}ch";
        _contextMenu.Visible = true;

        if (_ctxRename is not null)
            _termui.SetFocus(_ctxRename);
    }

    public void CloseContextMenu()
    {
        if (_contextMenu is null) return;
        _contextMenu.Visible = false;
    }

    public bool IsContextMenuOpen => _contextMenu?.Visible is true;

    public bool ContainsContextMenuWidget(IWidget widget)
        => widget == _ctxRename || widget == _ctxDelete;

    private void OpenRenameOverlay()
    {
        if (_renameOverlay is null || _renameInput is null || _contextMenuTargetId is null || _chatList is null) return;

        var button = _termui.GetWidget<Button>($"chat_{_contextMenuTargetId}");
        if (button is null) return;

        // Find button index in chatListPanel children
        int index = ((IWidget)_chatList).Children.IndexOf(button);
        if (index < 0) return;

        // Y = sidebar header (3ch) + index * button height (3ch) + 1 - scroll offset
        int y = 3 + index * 3 + 1 - (int)((IWidget)_chatList).ScrollOffsetY;

        _renameOverlay.PositionX = "0ch";
        _renameOverlay.PositionY = $"{y}ch";
        _renameOverlay.Visible = true;

        // Pre-fill with full title
        _renameInput.Value = _chatTitles.GetValueOrDefault(_contextMenuTargetId, "");
        _termui.SetFocus(_renameInput);
    }

    public void CloseRenameOverlay()
    {
        if (_renameOverlay is null) return;
        _renameOverlay.Visible = false;
    }

    public bool ContainsRenameWidget(IWidget widget)
        => widget == _renameInput;

    private void OnContextDelete()
    {
        if (_contextMenuTargetId is not null)
        {
            var id = _contextMenuTargetId;
            ChatDeleted?.Invoke(this, id);
        }
    }

    // Sidebar collapse animation

    public async Task CollapseAsync()
    {
        if (!IsOpen || _sidebarPanel is null) return;
        IsOpen = false;

        int currentWidth = SidebarWidth;
        while (currentWidth > 0)
        {
            currentWidth = Math.Max(0, currentWidth - AnimationStep);
            _sidebarPanel.Width = $"{currentWidth}ch";
            await Task.Delay(AnimationDelayMs);
        }

        _sidebarPanel.Visible = false;
        if (_sidebarLine is not null) _sidebarLine.Visible = false;
    }

    public async Task ExpandAsync()
    {
        if (IsOpen || _sidebarPanel is null) return;
        IsOpen = true;
        _sidebarPanel.Visible = true;
        if (_sidebarLine is not null) _sidebarLine.Visible = true;

        int currentWidth = 0;
        while (currentWidth < SidebarWidth)
        {
            currentWidth = Math.Min(SidebarWidth, currentWidth + AnimationStep);
            _sidebarPanel.Width = $"{currentWidth}ch";
            await Task.Delay(AnimationDelayMs);
        }
    }

    // Helpers

    private static string TruncateTitle(string title)
        => title.Length > MaxTitleLength ? title[..(MaxTitleLength - 1)] + "…" : title;

    public void SetActiveChat(string? id)
    {
        // Reset previous
        if (_activeChatId is not null)
        {
            var prev = _termui.GetWidget<Button>($"chat_{_activeChatId}");
            if (prev is not null)
            {
                prev.BackgroundColor = global::TermuiX.Color.Parse("#181825");
                prev.TextColor = global::TermuiX.Color.Parse("#a6adc8");
            }
        }

        _activeChatId = id;

        // Highlight new
        if (id is not null)
        {
            var btn = _termui.GetWidget<Button>($"chat_{id}");
            if (btn is not null)
            {
                btn.BackgroundColor = global::TermuiX.Color.Parse("#313244");
                btn.TextColor = global::TermuiX.Color.Parse("#cdd6f4");
            }
        }
    }

    public Button? GetSidebarButton() => _sidebarButton;
    public Button? GetNewChatButton() => _newChatButton;
    public StackPanel? GetChatList() => _chatList;
}

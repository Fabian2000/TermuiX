using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.AiChat.Components;

public class ConfirmDialog
{
    private readonly TermuiXLib _termui;

    private StackPanel? _overlay;
    private Text? _messageText;
    private Button? _deleteBtn;
    private Button? _cancelBtn;

    private Action? _onConfirm;

    public bool IsOpen => _overlay?.Visible is true;

    public ConfirmDialog(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='confirmOverlay' Direction='Vertical' Width='100%' Height='100%'
    BackgroundColor='#181825' Visible='false' Justify='Center' Align='Center'>
    <StackPanel Name='confirmBox' Direction='Vertical' PaddingLeft='1ch' PaddingRight='1ch'
        BackgroundColor='#313244' ForegroundColor='#cdd6f4'
        BorderStyle='Single' RoundedCorners='true'>
        <Text Name='confirmMessage' BackgroundColor='#313244' ForegroundColor='#cdd6f4'>Delete this chat?</Text>
        <StackPanel Direction='Horizontal' MarginTop='1ch'>
            <Button Name='btnConfirmDelete' Height='3ch'
                BorderStyle='Single' RoundedCorners='true'
                PaddingLeft='1ch' PaddingRight='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#f38ba8'
                TextColor='#f38ba8' FocusTextColor='#1e1e2e'>Delete</Button>
            <Button Name='btnConfirmCancel' Height='3ch'
                BorderStyle='Single' RoundedCorners='true'
                PaddingLeft='1ch' PaddingRight='1ch' MarginLeft='1ch'
                BackgroundColor='#313244' FocusBackgroundColor='#45475a'
                TextColor='#cdd6f4' FocusTextColor='#cdd6f4'>Cancel</Button>
        </StackPanel>
    </StackPanel>
</StackPanel>";
    }

    public void Initialize()
    {
        _overlay = _termui.GetWidget<StackPanel>("confirmOverlay");
        _messageText = _termui.GetWidget<Text>("confirmMessage");
        _deleteBtn = _termui.GetWidget<Button>("btnConfirmDelete");
        _cancelBtn = _termui.GetWidget<Button>("btnConfirmCancel");

        if (_deleteBtn is not null)
        {
            _deleteBtn.Click += (_, _) =>
            {
                var action = _onConfirm;
                Close();
                action?.Invoke();
            };
        }

        if (_cancelBtn is not null)
            _cancelBtn.Click += (_, _) => Close();
    }

    public void Show(string message, Action onConfirm)
    {
        if (_overlay is null) return;

        _onConfirm = onConfirm;

        if (_messageText is not null)
            _messageText.Content = message;

        _overlay.Visible = true;

        if (_cancelBtn is not null)
            _termui.SetFocus(_cancelBtn);
    }

    public void Close()
    {
        if (_overlay is null) return;
        _overlay.Visible = false;
        _onConfirm = null;
    }

    public bool ContainsWidget(IWidget widget)
        => widget == _deleteBtn || widget == _cancelBtn;
}

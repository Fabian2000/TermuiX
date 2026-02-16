using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class DialogManager
{
    private readonly TermuiXLib _termui;

    // Confirm dialog
    private StackPanel? _confirmOverlay;
    private Text? _confirmMessage;
    private Button? _confirmYes;
    private Button? _confirmNo;
    private Action? _onConfirm;

    // Input dialog
    private StackPanel? _inputOverlay;
    private Text? _inputTitle;
    private Input? _dialogInput;
    private Action<string>? _onInputSubmit;

    // Properties dialog
    private StackPanel? _propsOverlay;
    private Text? _propsName;
    private Text? _propsType;
    private Text? _propsSize;
    private Text? _propsCreated;
    private Text? _propsModified;
    private Text? _propsPermissions;
    private Button? _propsClose;

    public bool IsConfirmOpen => _confirmOverlay?.Visible is true;
    public bool IsInputDialogOpen => _inputOverlay?.Visible is true;
    public bool IsPropsOpen => _propsOverlay?.Visible is true;
    public bool IsAnyDialogOpen => IsConfirmOpen || IsInputDialogOpen || IsPropsOpen;

    public DialogManager(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='confirmOverlay' Direction='Vertical' Width='100%' Height='100%'
    BackgroundColor='#191919' Visible='false' Justify='Center' Align='Center'>
    <StackPanel Direction='Vertical' Width='40ch' PaddingLeft='2ch' PaddingRight='2ch' PaddingTop='1ch' PaddingBottom='1ch'
        BackgroundColor='#2a2a2a' BorderStyle='Single' RoundedCorners='true' ForegroundColor='#333333'>
        <Text Name='confirmMsg' Width='100%' BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'>Are you sure?</Text>
        <StackPanel Direction='Horizontal' Width='100%' MarginTop='1ch' Justify='End'>
            <Button Name='confirmNo' Height='3ch'
                BorderStyle='Single' RoundedCorners='true' PaddingLeft='2ch' PaddingRight='2ch'
                BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a'
                TextColor='#d0d0d0' FocusTextColor='#ffffff'>Cancel</Button>
            <Button Name='confirmYes' Height='3ch' MarginLeft='2ch'
                BorderStyle='Single' RoundedCorners='true' PaddingLeft='2ch' PaddingRight='2ch'
                BackgroundColor='#2a2a2a' FocusBackgroundColor='#cc5555'
                TextColor='#cc5555' FocusTextColor='#ffffff'>Delete</Button>
        </StackPanel>
    </StackPanel>
</StackPanel>

<StackPanel Name='inputOverlay' Direction='Vertical' Width='100%' Height='100%'
    BackgroundColor='#191919' Visible='false' Justify='Center' Align='Center'>
    <StackPanel Direction='Vertical' PaddingLeft='2ch' PaddingRight='2ch' PaddingTop='1ch' PaddingBottom='1ch'
        BackgroundColor='#2a2a2a' BorderStyle='Single' RoundedCorners='true' ForegroundColor='#333333'>
        <Text Name='inputDialogTitle' BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'>Enter name:</Text>
        <Input Name='dialogInput' Width='30ch' Height='3ch' MarginTop='1ch'
            BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'
            FocusBackgroundColor='#2a2a2a' FocusForegroundColor='#d0d0d0'
            BorderColor='#5588cc' FocusBorderColor='#5588cc' CursorColor='#d0d0d0' />
    </StackPanel>
</StackPanel>

<StackPanel Name='propsOverlay' Direction='Vertical' Width='100%' Height='100%'
    BackgroundColor='#191919' Visible='false' Justify='Center' Align='Center'>
    <StackPanel Direction='Vertical' Width='50ch' PaddingLeft='2ch' PaddingRight='2ch' PaddingTop='1ch' PaddingBottom='1ch'
        BackgroundColor='#2a2a2a' BorderStyle='Single' RoundedCorners='true' ForegroundColor='#333333'>
        <Text Width='100%' BackgroundColor='#2a2a2a' ForegroundColor='#5588cc' Style='Bold'>ℹ Properties</Text>
        <Text Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#333333' MarginTop='1ch'>──────────────────────────────────────────────</Text>
        <Text Name='propsName' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'>Name:</Text>
        <Text Name='propsType' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#909090'>Type:</Text>
        <Text Name='propsSize' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#909090'>Size:</Text>
        <Text Name='propsCreated' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#909090'>Created:</Text>
        <Text Name='propsModified' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#909090'>Modified:</Text>
        <Text Name='propsPerms' Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#909090'>Attribs:</Text>
        <Text Width='100%' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#333333' MarginTop='1ch'>──────────────────────────────────────────────</Text>
        <StackPanel Direction='Horizontal' Width='100%' MarginTop='1ch' Justify='Center'>
            <Button Name='propsClose' Width='12ch' Height='3ch'
                BorderStyle='Single' RoundedCorners='true'
                BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a'
                TextColor='#d0d0d0' FocusTextColor='#ffffff'>Close</Button>
        </StackPanel>
    </StackPanel>
</StackPanel>";
    }

    public void Initialize()
    {
        _confirmOverlay = _termui.GetWidget<StackPanel>("confirmOverlay");
        _confirmMessage = _termui.GetWidget<Text>("confirmMsg");
        _confirmYes = _termui.GetWidget<Button>("confirmYes");
        _confirmNo = _termui.GetWidget<Button>("confirmNo");

        _inputOverlay = _termui.GetWidget<StackPanel>("inputOverlay");
        _inputTitle = _termui.GetWidget<Text>("inputDialogTitle");
        _dialogInput = _termui.GetWidget<Input>("dialogInput");

        _propsOverlay = _termui.GetWidget<StackPanel>("propsOverlay");
        _propsName = _termui.GetWidget<Text>("propsName");
        _propsType = _termui.GetWidget<Text>("propsType");
        _propsSize = _termui.GetWidget<Text>("propsSize");
        _propsCreated = _termui.GetWidget<Text>("propsCreated");
        _propsModified = _termui.GetWidget<Text>("propsModified");
        _propsPermissions = _termui.GetWidget<Text>("propsPerms");
        _propsClose = _termui.GetWidget<Button>("propsClose");

        if (_confirmYes is not null)
            _confirmYes.Click += (_, _) => { var a = _onConfirm; CloseConfirm(); a?.Invoke(); };
        if (_confirmNo is not null)
            _confirmNo.Click += (_, _) => CloseConfirm();

        if (_dialogInput is not null)
        {
            _dialogInput.EnterPressed += (_, text) =>
            {
                var action = _onInputSubmit;
                CloseInputDialog();
                if (!string.IsNullOrWhiteSpace(text)) action?.Invoke(text.Trim());
            };
            _dialogInput.EscapePressed += (_, _) => CloseInputDialog();
        }

        if (_propsClose is not null)
            _propsClose.Click += (_, _) => CloseProperties();
    }

    // --- Confirm Dialog ---

    public void ShowConfirm(string message, Action onConfirm)
    {
        if (_confirmOverlay is null) return;
        _onConfirm = onConfirm;
        if (_confirmMessage is not null) _confirmMessage.Content = message;
        _confirmOverlay.Visible = true;
        if (_confirmNo is not null) _termui.SetFocus(_confirmNo);
    }

    public void CloseConfirm()
    {
        if (_confirmOverlay is null) return;
        _confirmOverlay.Visible = false;
        _onConfirm = null;
    }

    // --- Input Dialog ---

    public void ShowInputDialog(string title, string defaultValue, Action<string> onSubmit)
    {
        if (_inputOverlay is null || _dialogInput is null) return;
        _onInputSubmit = onSubmit;
        if (_inputTitle is not null) _inputTitle.Content = title;
        _dialogInput.Value = defaultValue;
        _inputOverlay.Visible = true;
        _termui.SetFocus(_dialogInput);
    }

    public void CloseInputDialog()
    {
        if (_inputOverlay is null) return;
        _inputOverlay.Visible = false;
        _onInputSubmit = null;
    }

    // --- Properties Dialog ---

    public void ShowProperties(string path)
    {
        if (_propsOverlay is null) return;

        try
        {
            bool isDir = Directory.Exists(path);
            bool isFile = File.Exists(path);
            if (!isDir && !isFile) return;

            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(name)) name = path;

            if (_propsName is not null)
                _propsName.Content = $"Name:     {name}";

            if (_propsType is not null)
            {
                if (isDir)
                    _propsType.Content = "Type:     Directory";
                else
                {
                    var ext = Path.GetExtension(path);
                    _propsType.Content = $"Type:     File ({(string.IsNullOrEmpty(ext) ? "no ext" : ext)})";
                }
            }

            if (_propsSize is not null)
            {
                if (isFile)
                {
                    var fi = new FileInfo(path);
                    _propsSize.Content = $"Size:     {FormatSize(fi.Length)}";
                }
                else
                {
                    try
                    {
                        var di = new DirectoryInfo(path);
                        var count = di.GetFileSystemInfos().Length;
                        _propsSize.Content = $"Contents: {count} items";
                    }
                    catch
                    {
                        _propsSize.Content = "Contents: Access denied";
                    }
                }
            }

            if (_propsCreated is not null)
            {
                var created = isFile ? File.GetCreationTime(path) : Directory.GetCreationTime(path);
                _propsCreated.Content = $"Created:  {created:yyyy-MM-dd HH:mm}";
            }

            if (_propsModified is not null)
            {
                var modified = isFile ? File.GetLastWriteTime(path) : Directory.GetLastWriteTime(path);
                _propsModified.Content = $"Modified: {modified:yyyy-MM-dd HH:mm}";
            }

            if (_propsPermissions is not null)
            {
                try
                {
                    var attrs = File.GetAttributes(path);
                    var parts = new List<string>();
                    if ((attrs & FileAttributes.ReadOnly) != 0) parts.Add("ReadOnly");
                    if ((attrs & FileAttributes.Hidden) != 0) parts.Add("Hidden");
                    if ((attrs & FileAttributes.System) != 0) parts.Add("System");
                    _propsPermissions.Content = $"Attribs:  {(parts.Count > 0 ? string.Join(", ", parts) : "Normal")}";
                }
                catch
                {
                    _propsPermissions.Content = "Attribs:  Unknown";
                }
            }

            _propsOverlay.Visible = true;
            if (_propsClose is not null) _termui.SetFocus(_propsClose);
        }
        catch { }
    }

    public void CloseProperties()
    {
        if (_propsOverlay is null) return;
        _propsOverlay.Visible = false;
    }

    public void CloseAll()
    {
        CloseConfirm();
        CloseInputDialog();
        CloseProperties();
    }

    public bool ContainsWidget(IWidget widget)
        => widget == _confirmYes || widget == _confirmNo
        || widget == _dialogInput
        || widget == _propsClose;

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return $"{size:0.#} {units[i]}";
    }
}

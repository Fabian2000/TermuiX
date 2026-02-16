using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class ContextMenu
{
    private readonly TermuiXLib _termui;
    private StackPanel? _menu;
    private Button? _ctxOpen;
    private Button? _ctxCopy;
    private Button? _ctxMove;
    private Button? _ctxPaste;
    private Button? _ctxRename;
    private Button? _ctxDelete;
    private Button? _ctxCompress;
    private Text? _ctxSep1;
    private Button? _ctxNew;
    private Button? _ctxMultiSel;
    private Text? _ctxSep2;
    private Button? _ctxProps;
    private string? _targetPath;

    // Submenus
    private StackPanel? _newSubmenu;
    private Button? _ctxNewFile;
    private Button? _ctxNewFolder;

    private StackPanel? _compressSubmenu;
    private Button? _ctxCompressTarGz;
    private Button? _ctxCompressZip;

    private Button? _ctxExtract;

    public bool IsOpen => _menu?.Visible is true;
    public bool IsSubmenuOpen => _newSubmenu?.Visible is true || _compressSubmenu?.Visible is true;
    public string? TargetPath => _targetPath;
    public bool IsEmptyAreaTarget { get; private set; }

    public event EventHandler<string>? OpenRequested;
    public event EventHandler<string>? CopyRequested;
    public event EventHandler<string>? MoveRequested;
    public event EventHandler? PasteRequested;
    public event EventHandler<string>? RenameRequested;
    public event EventHandler<string>? DeleteRequested;
    public event EventHandler? NewFileRequested;
    public event EventHandler? NewFolderRequested;
    public event EventHandler? MultiSelectRequested;
    public event EventHandler<string>? PropertiesRequested;
    public event EventHandler<(string path, string format)>? CompressRequested;
    public event EventHandler<string>? ExtractRequested;

    public ContextMenu(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        // Main menu + submenu as separate positioned panels
        return @"
<StackPanel Name='ctxMenu' Direction='Vertical' Width='20ch' Height='auto'
    PositionX='0ch' PositionY='0ch'
    BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'
    BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='ctxOpen' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📂 Open</Button>
    <Button Name='ctxCopy' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📋 Copy</Button>
    <Button Name='ctxMove' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📦 Move</Button>
    <Button Name='ctxPaste' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#555555' FocusTextColor='#555555' TextAlign='Left' Disabled='true'>📌 Paste</Button>
    <Button Name='ctxRename' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>✎ Rename</Button>
    <Button Name='ctxDelete' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#cc5555' FocusTextColor='#cc5555' TextAlign='Left'>✕ Delete</Button>
    <Button Name='ctxCompress' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>🗜 Compress    ▸</Button>
    <Button Name='ctxExtract' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📤 Extract</Button>
    <Text Name='ctxSep1' Width='18ch' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#333333'>──────────────────</Text>
    <Button Name='ctxNew' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>+ New           ▸</Button>
    <Button Name='ctxMultiSel' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#5588cc' FocusTextColor='#5588cc' TextAlign='Left'>☑ Multi Select</Button>
    <Text Name='ctxSep2' Width='18ch' Height='1ch' BackgroundColor='#2a2a2a' ForegroundColor='#333333'>──────────────────</Text>
    <Button Name='ctxProps' Width='18ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#909090' FocusTextColor='#d0d0d0' TextAlign='Left'>ℹ Properties</Button>
</StackPanel>

<StackPanel Name='ctxNewSubmenu' Direction='Vertical' Width='18ch' Height='auto'
    PositionX='0ch' PositionY='0ch'
    BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'
    BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='ctxNewFile' Width='16ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📄 File</Button>
    <Button Name='ctxNewFolder' Width='16ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📁 Folder</Button>
</StackPanel>

<StackPanel Name='ctxCompressSubmenu' Direction='Vertical' Width='18ch' Height='auto'
    PositionX='0ch' PositionY='0ch'
    BackgroundColor='#2a2a2a' ForegroundColor='#d0d0d0'
    BorderStyle='Single' RoundedCorners='true' Visible='false'>
    <Button Name='ctxCompressTarGz' Width='16ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📦 tar.gz</Button>
    <Button Name='ctxCompressZip' Width='16ch' Height='1ch' BorderStyle='None'
        PaddingLeft='1ch' PaddingRight='0ch' PaddingTop='0ch' PaddingBottom='0ch'
        BackgroundColor='#2a2a2a' FocusBackgroundColor='#3a3a3a' TextColor='#d0d0d0' FocusTextColor='#ffffff' TextAlign='Left'>📦 zip</Button>
</StackPanel>";
    }

    public void Initialize()
    {
        _menu = _termui.GetWidget<StackPanel>("ctxMenu");
        _ctxOpen = _termui.GetWidget<Button>("ctxOpen");
        _ctxCopy = _termui.GetWidget<Button>("ctxCopy");
        _ctxMove = _termui.GetWidget<Button>("ctxMove");
        _ctxPaste = _termui.GetWidget<Button>("ctxPaste");
        _ctxRename = _termui.GetWidget<Button>("ctxRename");
        _ctxDelete = _termui.GetWidget<Button>("ctxDelete");
        _ctxSep1 = _termui.GetWidget<Text>("ctxSep1");
        _ctxNew = _termui.GetWidget<Button>("ctxNew");
        _ctxMultiSel = _termui.GetWidget<Button>("ctxMultiSel");
        _ctxSep2 = _termui.GetWidget<Text>("ctxSep2");
        _ctxProps = _termui.GetWidget<Button>("ctxProps");

        _ctxCompress = _termui.GetWidget<Button>("ctxCompress");
        _ctxExtract = _termui.GetWidget<Button>("ctxExtract");

        _newSubmenu = _termui.GetWidget<StackPanel>("ctxNewSubmenu");
        _ctxNewFile = _termui.GetWidget<Button>("ctxNewFile");
        _ctxNewFolder = _termui.GetWidget<Button>("ctxNewFolder");

        _compressSubmenu = _termui.GetWidget<StackPanel>("ctxCompressSubmenu");
        _ctxCompressTarGz = _termui.GetWidget<Button>("ctxCompressTarGz");
        _ctxCompressZip = _termui.GetWidget<Button>("ctxCompressZip");

        if (_ctxOpen is not null)
            _ctxOpen.Click += (_, _) => { if (_targetPath is not null) OpenRequested?.Invoke(this, _targetPath); Close(); };
        if (_ctxCopy is not null)
            _ctxCopy.Click += (_, _) => { if (_targetPath is not null) CopyRequested?.Invoke(this, _targetPath); Close(); };
        if (_ctxMove is not null)
            _ctxMove.Click += (_, _) => { if (_targetPath is not null) MoveRequested?.Invoke(this, _targetPath); Close(); };
        if (_ctxPaste is not null)
            _ctxPaste.Click += (_, _) => { PasteRequested?.Invoke(this, EventArgs.Empty); Close(); };
        if (_ctxRename is not null)
            _ctxRename.Click += (_, _) => { if (_targetPath is not null) RenameRequested?.Invoke(this, _targetPath); Close(); };
        if (_ctxDelete is not null)
            _ctxDelete.Click += (_, _) => { if (_targetPath is not null) DeleteRequested?.Invoke(this, _targetPath); Close(); };

        // "New ▸" opens submenu
        if (_ctxNew is not null)
            _ctxNew.Click += (_, _) => ToggleNewSubmenu();

        if (_ctxNewFile is not null)
            _ctxNewFile.Click += (_, _) => { NewFileRequested?.Invoke(this, EventArgs.Empty); Close(); };
        if (_ctxNewFolder is not null)
            _ctxNewFolder.Click += (_, _) => { NewFolderRequested?.Invoke(this, EventArgs.Empty); Close(); };

        // "Compress ▸" opens compress submenu
        if (_ctxCompress is not null)
            _ctxCompress.Click += (_, _) => ToggleCompressSubmenu();

        if (_ctxCompressTarGz is not null)
            _ctxCompressTarGz.Click += (_, _) => { if (_targetPath is not null) CompressRequested?.Invoke(this, (_targetPath, "tar.gz")); Close(); };
        if (_ctxCompressZip is not null)
            _ctxCompressZip.Click += (_, _) => { if (_targetPath is not null) CompressRequested?.Invoke(this, (_targetPath, "zip")); Close(); };

        if (_ctxExtract is not null)
            _ctxExtract.Click += (_, _) => { if (_targetPath is not null) ExtractRequested?.Invoke(this, _targetPath); Close(); };

        if (_ctxMultiSel is not null)
            _ctxMultiSel.Click += (_, _) => { MultiSelectRequested?.Invoke(this, EventArgs.Empty); Close(); };
        if (_ctxProps is not null)
            _ctxProps.Click += (_, _) => { if (_targetPath is not null) PropertiesRequested?.Invoke(this, _targetPath); Close(); };
    }

    private int CountVisibleRowsAbove(Button? target)
    {
        int count = 0;
        Button?[] order = [_ctxOpen, _ctxCopy, _ctxMove, _ctxPaste, _ctxRename, _ctxDelete, _ctxCompress, _ctxExtract];
        Text?[] seps = [_ctxSep1];

        foreach (var btn in order)
        {
            if (btn == target) return count;
            if (btn?.Visible is true) count++;
        }

        foreach (var sep in seps)
        {
            if (sep?.Visible is true) count++;
        }

        // After sep1: New, MultiSel, sep2, Props
        Button?[] afterSep = [_ctxNew, _ctxMultiSel];
        foreach (var btn in afterSep)
        {
            if (btn == target) return count;
            if (btn?.Visible is true) count++;
        }

        return count;
    }

    private void ShowSubmenuAt(StackPanel submenu, int rowIndex, int subW, int subH, Button? focusTarget)
    {
        if (_menu is null) return;

        int menuX = ParseCh(_menu.PositionX);
        int menuY = ParseCh(_menu.PositionY);
        int menuW = 20;

        int subX = menuX + menuW;
        int subY = menuY + rowIndex + 1; // +1 for border

        if (subX + subW > Console.WindowWidth) subX = menuX - subW;
        if (subY + subH > Console.WindowHeight) subY = Console.WindowHeight - subH;

        submenu.PositionX = $"{subX}ch";
        submenu.PositionY = $"{subY}ch";
        submenu.Visible = true;

        if (focusTarget is not null) _termui.SetFocus(focusTarget);
    }

    private void CloseAllSubmenus()
    {
        if (_newSubmenu is not null) _newSubmenu.Visible = false;
        if (_compressSubmenu is not null) _compressSubmenu.Visible = false;
    }

    private void ToggleNewSubmenu()
    {
        if (_newSubmenu is null || _menu is null) return;

        if (_newSubmenu.Visible)
        {
            _newSubmenu.Visible = false;
            return;
        }

        CloseAllSubmenus();

        int rowIndex = 0;
        if (_ctxOpen?.Visible is true) rowIndex++;
        if (_ctxCopy?.Visible is true) rowIndex++;
        if (_ctxMove?.Visible is true) rowIndex++;
        if (_ctxPaste?.Visible is true) rowIndex++;
        if (_ctxRename?.Visible is true) rowIndex++;
        if (_ctxDelete?.Visible is true) rowIndex++;
        if (_ctxCompress?.Visible is true) rowIndex++;
        if (_ctxExtract?.Visible is true) rowIndex++;
        if (_ctxSep1?.Visible is true) rowIndex++;

        ShowSubmenuAt(_newSubmenu, rowIndex, 18, 4, _ctxNewFile);
    }

    private void ToggleCompressSubmenu()
    {
        if (_compressSubmenu is null || _menu is null) return;

        if (_compressSubmenu.Visible)
        {
            _compressSubmenu.Visible = false;
            return;
        }

        CloseAllSubmenus();

        int rowIndex = 0;
        if (_ctxOpen?.Visible is true) rowIndex++;
        if (_ctxCopy?.Visible is true) rowIndex++;
        if (_ctxMove?.Visible is true) rowIndex++;
        if (_ctxPaste?.Visible is true) rowIndex++;
        if (_ctxRename?.Visible is true) rowIndex++;
        if (_ctxDelete?.Visible is true) rowIndex++;

        ShowSubmenuAt(_compressSubmenu, rowIndex, 18, 4, _ctxCompressTarGz);
    }

    private static int ParseCh(string value)
    {
        if (value.EndsWith("ch") && int.TryParse(value[..^2].Trim(), out int v))
            return v;
        return 0;
    }

    public void Show(int x, int y, string targetPath, bool hasPaste, bool isEmptyArea = false)
    {
        if (_menu is null) return;

        _targetPath = targetPath;
        IsEmptyAreaTarget = isEmptyArea;

        // Hide submenus
        CloseAllSubmenus();

        // Show/hide based on context
        if (_ctxOpen is not null) _ctxOpen.Visible = !isEmptyArea;
        if (_ctxCopy is not null) _ctxCopy.Visible = !isEmptyArea;
        if (_ctxMove is not null) _ctxMove.Visible = !isEmptyArea;
        if (_ctxRename is not null) _ctxRename.Visible = !isEmptyArea;
        if (_ctxDelete is not null) _ctxDelete.Visible = !isEmptyArea;
        if (_ctxCompress is not null) _ctxCompress.Visible = !isEmptyArea;
        bool isArchive = !isEmptyArea && IsArchiveFile(targetPath);
        if (_ctxExtract is not null) _ctxExtract.Visible = isArchive;
        if (_ctxSep1 is not null) _ctxSep1.Visible = !isEmptyArea;
        if (_ctxSep2 is not null) _ctxSep2.Visible = !isEmptyArea;
        if (_ctxProps is not null) _ctxProps.Visible = !isEmptyArea;

        // Enable/disable paste
        if (_ctxPaste is not null)
        {
            _ctxPaste.Disabled = !hasPaste;
            _ctxPaste.TextColor = hasPaste
                ? global::TermuiX.Color.Parse("#d0d0d0")
                : global::TermuiX.Color.Parse("#555555");
            _ctxPaste.FocusTextColor = hasPaste
                ? global::TermuiX.Color.Parse("#ffffff")
                : global::TermuiX.Color.Parse("#555555");
        }

        // Calculate menu height based on visible items
        int visibleItems = 0;
        if (!isEmptyArea) visibleItems += 9; // Open,Copy,Move,Paste,Rename,Delete,Compress,Sep1,Sep2
        else visibleItems += 1; // just Paste
        if (isArchive) visibleItems += 1; // Extract
        visibleItems += 2; // New, MultiSel
        if (!isEmptyArea) visibleItems += 1; // Props

        int menuH = visibleItems + 2; // +2 for border
        int menuW = 20;
        if (x + menuW > Console.WindowWidth) x = Console.WindowWidth - menuW;
        if (y + menuH > Console.WindowHeight) y = Console.WindowHeight - menuH;

        _menu.PositionX = $"{x}ch";
        _menu.PositionY = $"{y}ch";
        _menu.Visible = true;

        // Focus first visible item
        if (isEmptyArea)
        {
            if (_ctxPaste is not null && !_ctxPaste.Disabled) _termui.SetFocus(_ctxPaste);
            else if (_ctxNew is not null) _termui.SetFocus(_ctxNew);
        }
        else
        {
            if (_ctxOpen is not null) _termui.SetFocus(_ctxOpen);
        }
    }

    public void Close()
    {
        if (_menu is not null) _menu.Visible = false;
        CloseAllSubmenus();
    }

    public bool ContainsWidget(IWidget widget)
        => widget == _ctxOpen || widget == _ctxCopy || widget == _ctxMove
        || widget == _ctxPaste || widget == _ctxRename || widget == _ctxDelete
        || widget == _ctxCompress || widget == _ctxExtract
        || widget == _ctxNew || widget == _ctxMultiSel || widget == _ctxProps
        || widget == _ctxNewFile || widget == _ctxNewFolder
        || widget == _ctxCompressTarGz || widget == _ctxCompressZip;

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".gz", ".tar",
    };

    private static bool IsArchiveFile(string path)
    {
        if (!File.Exists(path)) return false;
        if (path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase)) return true;
        string ext = Path.GetExtension(path);
        return ArchiveExtensions.Contains(ext);
    }
}

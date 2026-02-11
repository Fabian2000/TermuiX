using System.Text;
using System.Xml.Linq;
using TermuiX.Widgets;

namespace TermuiX;

internal static class XmlParser
{
    private static readonly Dictionary<string, Func<Dictionary<string, string>, IWidget>> _customWidgetFactories = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Func<Dictionary<string, string>, string>> _customComponentFactories = new(StringComparer.OrdinalIgnoreCase);

    internal static void RegisterWidget(string tagName, Func<Dictionary<string, string>, IWidget> factory)
    {
        _customWidgetFactories[tagName] = factory;
    }

    internal static void UnregisterWidget(string tagName)
    {
        _customWidgetFactories.Remove(tagName);
    }

    internal static void RegisterComponent(string tagName, Func<Dictionary<string, string>, string> xmlFactory)
    {
        _customComponentFactories[tagName] = xmlFactory;
    }

    internal static void UnregisterComponent(string tagName)
    {
        _customComponentFactories.Remove(tagName);
    }

    public static IWidget Parse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidOperationException("XML document has no root element");

        return ParseElement(root);
    }

    private static int CalculateDisplayWidth(string text)
    {
        int totalWidth = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            totalWidth += GetRuneDisplayWidth(rune);
        }
        return totalWidth;
    }

    private static int GetRuneDisplayWidth(Rune rune)
    {
        int value = rune.Value;

        // Control characters
        if (value == 0 || Rune.IsControl(rune))
            return 0;

        // Explicit zero-width characters
        if (value == 0x200B ||                          // Zero-Width Space
            value == 0x200C ||                          // Zero-Width Non-Joiner
            value == 0x200D ||                          // Zero-Width Joiner
            value == 0x2060 ||                          // Word Joiner
            value == 0xFEFF ||                          // Zero-Width No-Break Space (BOM)
            (value >= 0xFE00 && value <= 0xFE0F) ||     // Variation Selectors
            (value >= 0xE0100 && value <= 0xE01EF) ||   // Variation Selectors Supplement
            (value >= 0xE0000 && value <= 0xE007F))     // Tags Block
        {
            return 0;
        }

        // Hangul Jungseong/Jongseong (combining vowels/finals in Korean syllables)
        if ((value >= 0x1160 && value <= 0x11FF) ||
            (value >= 0xD7B0 && value <= 0xD7C6) ||
            (value >= 0xD7CB && value <= 0xD7FB))
        {
            return 0;
        }

        // Combining marks and format characters
        var runeCategory = Rune.GetUnicodeCategory(rune);
        if (runeCategory == System.Globalization.UnicodeCategory.NonSpacingMark ||
            runeCategory == System.Globalization.UnicodeCategory.EnclosingMark ||
            runeCategory == System.Globalization.UnicodeCategory.Format)
        {
            return 0;
        }

        // East Asian Wide and Fullwidth characters
        if ((value >= 0x1100 && value <= 0x115F) ||   // Hangul Jamo
            (value >= 0x231A && value <= 0x231B) ||   // ⌚⌛
            (value >= 0x2329 && value <= 0x232A) ||   // 〈〉
            (value >= 0x23E9 && value <= 0x23EC) ||   // ⏩⏪⏫⏬
            value == 0x23F0 ||                        // ⏰
            value == 0x23F3 ||                        // ⏳
            (value >= 0x25FD && value <= 0x25FE) ||   // ◽◾
            (value >= 0x2614 && value <= 0x2615) ||   // ☔☕
            (value >= 0x2648 && value <= 0x2653) ||   // ♈-♓
            value == 0x267F ||                        // ♿
            value == 0x2693 ||                        // ⚓
            value == 0x26A1 ||                        // ⚡
            (value >= 0x26AA && value <= 0x26AB) ||   // ⚪⚫
            (value >= 0x26BD && value <= 0x26BE) ||   // ⚽⚾
            (value >= 0x26C4 && value <= 0x26C5) ||   // ⛄⛅
            value == 0x26CE ||                        // ⛎
            value == 0x26D4 ||                        // ⛔
            value == 0x26EA ||                        // ⛪
            (value >= 0x26F2 && value <= 0x26F3) ||   // ⛲⛳
            value == 0x26F5 ||                        // ⛵
            value == 0x26FA ||                        // ⛺
            value == 0x26FD ||                        // ⛽
            value == 0x2705 ||                        // ✅
            (value >= 0x270A && value <= 0x270B) ||   // ✊✋
            value == 0x2728 ||                        // ✨
            value == 0x274C ||                        // ❌
            value == 0x274E ||                        // ❎
            (value >= 0x2753 && value <= 0x2755) ||   // ❓❔❕
            value == 0x2757 ||                        // ❗
            (value >= 0x2795 && value <= 0x2797) ||   // ➕➖➗
            value == 0x27B0 ||                        // ➰
            value == 0x27BF ||                        // ➿
            (value >= 0x2B1B && value <= 0x2B1C) ||   // ⬛⬜
            value == 0x2B50 ||                        // ⭐
            value == 0x2B55 ||                        // ⭕
            (value >= 0x2E80 && value <= 0x9FFF) ||   // CJK Radicals, Kangxi, CJK Unified
            (value >= 0xA000 && value <= 0xA4C6) ||   // Yi Syllables + Radicals
            (value >= 0xA960 && value <= 0xA97C) ||   // Hangul Jamo Extended-A
            (value >= 0xAC00 && value <= 0xD7A3) ||   // Hangul Syllables
            (value >= 0xF900 && value <= 0xFAFF) ||   // CJK Compatibility Ideographs
            (value >= 0xFE10 && value <= 0xFE19) ||   // Vertical Forms
            (value >= 0xFE30 && value <= 0xFE6B) ||   // CJK Compatibility Forms
            (value >= 0xFF00 && value <= 0xFF60) ||   // Fullwidth Forms
            (value >= 0xFFE0 && value <= 0xFFE6))     // Fullwidth Signs
        {
            return 2;
        }

        // Emoji wide ranges (per wcwidth, U+1F000+)
        if (value == 0x1F004 ||                        // 🀄
            value == 0x1F0CF ||                        // 🃏
            value == 0x1F18E ||                        // 🆎
            (value >= 0x1F191 && value <= 0x1F19A) || // 🆑-🆚
            (value >= 0x1F200 && value <= 0x1F202) || // 🈀-🈂
            (value >= 0x1F210 && value <= 0x1F23B) || // 🈐-🈻
            (value >= 0x1F240 && value <= 0x1F248) || // 🈴-🉈
            (value >= 0x1F250 && value <= 0x1F251) || // 🉐🉑
            (value >= 0x1F260 && value <= 0x1F265) || // 🉠-🉥
            (value >= 0x1F300 && value <= 0x1F320) ||
            (value >= 0x1F32D && value <= 0x1F335) ||
            (value >= 0x1F337 && value <= 0x1F37C) ||
            (value >= 0x1F37E && value <= 0x1F393) ||
            (value >= 0x1F3A0 && value <= 0x1F3CA) ||
            (value >= 0x1F3CF && value <= 0x1F3D3) ||
            (value >= 0x1F3E0 && value <= 0x1F3F0) ||
            value == 0x1F3F4 ||
            (value >= 0x1F3F8 && value <= 0x1F43E) ||
            value == 0x1F440 ||
            (value >= 0x1F442 && value <= 0x1F4FC) ||
            (value >= 0x1F4FF && value <= 0x1F53D) ||
            (value >= 0x1F54B && value <= 0x1F54E) ||
            (value >= 0x1F550 && value <= 0x1F567) ||
            value == 0x1F57A ||
            (value >= 0x1F595 && value <= 0x1F596) ||
            value == 0x1F5A4 ||
            (value >= 0x1F5FB && value <= 0x1F64F) ||
            (value >= 0x1F680 && value <= 0x1F6C5) ||
            value == 0x1F6CC ||
            (value >= 0x1F6D0 && value <= 0x1F6D2) ||
            (value >= 0x1F6D5 && value <= 0x1F6D7) ||
            (value >= 0x1F6DC && value <= 0x1F6DF) ||
            (value >= 0x1F6EB && value <= 0x1F6EC) ||
            (value >= 0x1F6F4 && value <= 0x1F6FC) ||
            (value >= 0x1F7E0 && value <= 0x1F7EB) ||
            value == 0x1F7F0 ||
            (value >= 0x1F90C && value <= 0x1F93A) ||
            (value >= 0x1F93C && value <= 0x1F945) ||
            (value >= 0x1F947 && value <= 0x1F9FF) ||
            (value >= 0x1FA70 && value <= 0x1FA7C) || // 🩰-🩼
            (value >= 0x1FA80 && value <= 0x1FA88) || // 🪀-🪈
            (value >= 0x1FA90 && value <= 0x1FABD) || // 🪐-🪽
            (value >= 0x1FABF && value <= 0x1FAC5) || // 🪿-🫅
            (value >= 0x1FACE && value <= 0x1FADB) || // 🫎-🫛
            (value >= 0x1FAE0 && value <= 0x1FAE8) || // 🫠-🫨
            (value >= 0x1FAF0 && value <= 0x1FAF8))   // 🫰-🫸
        {
            return 2;
        }

        // CJK Supplementary + Tangut + Nushu + Katakana Extended
        if ((value >= 0x16FE0 && value <= 0x16FE3) ||
            (value >= 0x16FF0 && value <= 0x16FF1) ||
            (value >= 0x17000 && value <= 0x187F7) || // Tangut
            (value >= 0x18800 && value <= 0x18CD5) || // Tangut Components + Khitan
            (value >= 0x18D00 && value <= 0x18D08) ||
            (value >= 0x1AFF0 && value <= 0x1AFF3) || // Katakana Extended
            (value >= 0x1AFF5 && value <= 0x1AFFB) ||
            (value >= 0x1AFFD && value <= 0x1AFFE) ||
            (value >= 0x1B000 && value <= 0x1B122) || // Kana Supplement/Extended
            value == 0x1B132 ||
            (value >= 0x1B150 && value <= 0x1B152) ||
            value == 0x1B155 ||
            (value >= 0x1B164 && value <= 0x1B167) ||
            (value >= 0x1B170 && value <= 0x1B2FB) || // Nushu
            (value >= 0x20000 && value <= 0x2FA1D) || // CJK Extensions B-G + Compatibility
            (value >= 0x30000 && value <= 0x3FFFD))   // CJK Extension H+
        {
            return 2;
        }

        // Default: 1 cell
        return 1;
    }

    private static IWidget ParseElement(XElement element)
    {
        // Extract attributes into a dictionary
        var attributes = element.Attributes()
            .ToDictionary(
                attr => attr.Name.LocalName,
                attr => attr.Value,
                StringComparer.OrdinalIgnoreCase
            );

        // Check custom component registry first (components that return XML)
        if (_customComponentFactories.TryGetValue(element.Name.LocalName, out var componentFactory))
        {
            string componentXml = componentFactory(attributes);
            return Parse(componentXml);
        }

        // Check custom widget registry second (direct IWidget implementations)
        if (_customWidgetFactories.TryGetValue(element.Name.LocalName, out var widgetFactory))
        {
            IWidget customWidget = widgetFactory(attributes);

            // For custom widgets that are containers, parse their children
            if (customWidget is Container customContainer)
            {
                foreach (var childElement in element.Elements())
                {
                    var childWidget = ParseElement(childElement);
                    customContainer.Add(childWidget);
                }
            }

            return customWidget;
        }

        IWidget widget = element.Name.LocalName.ToLower() switch
        {
            "container" => new Container(),
            "text" => new Text(),
            "button" => new Button(),
            "input" => new Input(),
            "checkbox" => new Checkbox(),
            "radiobutton" => new RadioButton(),
            "progressbar" => new ProgressBar(),
            "chart" => new Chart(),
            "slider" => new Slider(),
            "line" => new Line(),
            "table" => new Table(),
            "stackpanel" => new StackPanel(),
            _ => throw new NotSupportedException($"Widget type '{element.Name.LocalName}' is not supported")
        };

        string? textContent = null;
        if (!string.IsNullOrWhiteSpace(element.Value))
        {
            textContent = element.Value.Trim();
        }

        if (widget is Text textWidget && textContent is not null)
        {
            textWidget.Content = textContent;
        }
        else if (widget is Button buttonWidget && textContent is not null)
        {
            buttonWidget.Text = textContent;
        }
        else if (widget is Input inputWidget && textContent is not null)
        {
            inputWidget.Value = textContent;
        }

        foreach (var attr in element.Attributes())
        {
            SetWidgetProperty(widget, attr.Name.LocalName, attr.Value);
        }

        if (widget is Button button && textContent is not null)
        {
            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase)))
            {
                int autoWidth = CalculateDisplayWidth(textContent) + 4;
                button.Width = $"{autoWidth}ch";
            }

            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Height", StringComparison.OrdinalIgnoreCase)))
            {
                int autoHeight = 3;
                button.Height = $"{autoHeight}ch";
            }
        }

        if (widget is Text text)
        {
            if (textContent is not null)
            {
                if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase)))
                {
                    var lines = textContent.Split('\n');
                    int maxWidth = lines.Max(line => CalculateDisplayWidth(line));
                    text.Width = $"{maxWidth}ch";
                }

                if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Height", StringComparison.OrdinalIgnoreCase)))
                {
                    int lineCount = textContent.Split('\n').Length;
                    text.Height = $"{lineCount}ch";
                }
            }
            else
            {
                // If text is empty/whitespace and no explicit size is set, use 0ch to avoid contributing to scrollbar calculations
                if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase)))
                {
                    text.Width = "0ch";
                }

                if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Height", StringComparison.OrdinalIgnoreCase)))
                {
                    text.Height = "0ch";
                }
            }
        }

        if (widget is Container container)
        {
            foreach (var childElement in element.Elements())
            {
                var childWidget = ParseElement(childElement);
                container.Add(childWidget);
            }
        }
        else if (widget is Table table)
        {
            foreach (var rowElement in element.Elements())
            {
                if (rowElement.Name.LocalName.Equals("TableRow", StringComparison.OrdinalIgnoreCase))
                {
                    var row = ParseTableRow(rowElement);
                    table.AddRow(row);
                }
            }
        }

        return widget;
    }

    private static TableRow ParseTableRow(XElement element)
    {
        var row = new TableRow();

        foreach (var cellElement in element.Elements())
        {
            if (cellElement.Name.LocalName.Equals("TableCell", StringComparison.OrdinalIgnoreCase))
            {
                var cell = ParseTableCell(cellElement);
                row.AddCell(cell);
            }
        }

        return row;
    }

    private static TableCell ParseTableCell(XElement element)
    {
        var cell = new TableCell();

        foreach (var attr in element.Attributes())
        {
            SetTableCellProperty(cell, attr.Name.LocalName, attr.Value);
        }

        // Check if cell contains a widget or text
        if (element.Elements().Any())
        {
            var childWidget = ParseElement(element.Elements().First());
            cell.Widget = childWidget;
        }
        else if (!string.IsNullOrWhiteSpace(element.Value))
        {
            cell.Text = element.Value.Trim();
        }

        return cell;
    }

    private static void SetTableCellProperty(TableCell cell, string propertyName, string value)
    {
        switch (propertyName.ToLowerInvariant())
        {
            case "text":
                cell.Text = value;
                break;
            case "backgroundcolor":
                cell.BackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "foregroundcolor":
                cell.ForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "style":
                cell.Style = Enum.Parse<TextStyle>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetWidgetProperty(IWidget widget, string propertyName, string value)
    {
        string propLower = propertyName.ToLowerInvariant();

        // Common properties for all widgets
        switch (propLower)
        {
            case "name":
                widget.Name = value;
                break;
            case "group":
                widget.Group = value;
                break;
            case "width":
                widget.Width = value;
                break;
            case "height":
                widget.Height = value;
                break;
            case "positionx":
                widget.PositionX = value;
                break;
            case "positiony":
                widget.PositionY = value;
                break;
            case "visible":
                widget.Visible = bool.Parse(value);
                break;
            case "allowwrapping":
                widget.AllowWrapping = bool.Parse(value);
                break;
            case "backgroundcolor":
                widget.BackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "foregroundcolor":
                widget.ForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "focusbackgroundcolor":
                widget.FocusBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "focusforegroundcolor":
                widget.FocusForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "paddingleft":
                widget.PaddingLeft = value;
                break;
            case "paddingtop":
                widget.PaddingTop = value;
                break;
            case "paddingright":
                widget.PaddingRight = value;
                break;
            case "paddingbottom":
                widget.PaddingBottom = value;
                break;
            case "marginleft":
                widget.MarginLeft = value;
                break;
            case "margintop":
                widget.MarginTop = value;
                break;
            case "marginright":
                widget.MarginRight = value;
                break;
            case "marginbottom":
                widget.MarginBottom = value;
                break;
        }

        // Widget-specific properties (StackPanel before Container since it inherits from it)
        if (widget is StackPanel stackPanel)
        {
            SetStackPanelProperty(stackPanel, propLower, value);
        }
        else if (widget is Container container)
        {
            SetContainerProperty(container, propLower, value);
        }
        else if (widget is Text text)
        {
            SetTextProperty(text, propLower, value);
        }
        else if (widget is Button button)
        {
            SetButtonProperty(button, propLower, value);
        }
        else if (widget is Input input)
        {
            SetInputProperty(input, propLower, value);
        }
        else if (widget is Checkbox checkbox)
        {
            SetCheckboxProperty(checkbox, propLower, value);
        }
        else if (widget is RadioButton radioButton)
        {
            SetRadioButtonProperty(radioButton, propLower, value);
        }
        else if (widget is ProgressBar progressBar)
        {
            SetProgressBarProperty(progressBar, propLower, value);
        }
        else if (widget is Chart chart)
        {
            SetChartProperty(chart, propLower, value);
        }
        else if (widget is Slider slider)
        {
            SetSliderProperty(slider, propLower, value);
        }
        else if (widget is Line line)
        {
            SetLineProperty(line, propLower, value);
        }
        else if (widget is Table table)
        {
            SetTableProperty(table, propLower, value);
        }
    }

    private static void SetStackPanelProperty(StackPanel stackPanel, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "direction":
                stackPanel.Direction = Enum.Parse<StackDirection>(value, ignoreCase: true);
                break;
            case "justify":
                stackPanel.Justify = Enum.Parse<StackJustify>(value, ignoreCase: true);
                break;
            case "align":
                stackPanel.Align = Enum.Parse<StackAlign>(value, ignoreCase: true);
                break;
            case "wrap":
                stackPanel.Wrap = bool.Parse(value);
                break;
            default:
                // Fall through to container properties (scrollable, borderstyle, etc.)
                SetContainerProperty(stackPanel, propertyName, value);
                break;
        }
    }

    private static void SetContainerProperty(Container container, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "scrollable":
                container.Scrollable = bool.Parse(value);
                break;
            case "borderstyle":
                container.BorderStyle = Enum.Parse<BorderStyle>(value, ignoreCase: true);
                break;
            case "roundedcorners":
                container.RoundedCorners = bool.Parse(value);
                break;
        }
    }

    private static void SetTextProperty(Text text, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "content":
                text.Content = value;
                break;
            case "textalign":
                text.TextAlign = Enum.Parse<TextAlign>(value, ignoreCase: true);
                break;
            case "style":
                text.Style = Enum.Parse<TextStyle>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetButtonProperty(Button button, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "text":
                button.Text = value;
                break;
            case "borderstyle":
                button.BorderStyle = Enum.Parse<BorderStyle>(value, ignoreCase: true);
                break;
            case "roundedcorners":
                button.RoundedCorners = bool.Parse(value);
                break;
            case "bordercolor":
                button.BorderColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "textcolor":
                button.TextColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "focusbordercolor":
                button.FocusBorderColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "focustextcolor":
                button.FocusTextColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "textstyle":
                button.TextStyle = Enum.Parse<TextStyle>(value, ignoreCase: true);
                break;
            case "textalign":
                button.TextAlign = Enum.Parse<TextAlign>(value, ignoreCase: true);
                break;
            case "disabled":
                button.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                button.DisabledBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabledforegroundcolor":
                button.DisabledForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetInputProperty(Input input, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "value":
                input.Value = value;
                break;
            case "multiline":
                input.Multiline = bool.Parse(value);
                break;
            case "ispassword":
                input.IsPassword = bool.Parse(value);
                break;
            case "placeholder":
                input.Placeholder = value;
                break;
            case "bordercolor":
                input.BorderColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "focusbordercolor":
                input.FocusBorderColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "placeholdercolor":
                input.PlaceholderColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "cursorcolor":
                input.CursorColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabled":
                input.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                input.DisabledBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabledforegroundcolor":
                input.DisabledForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetCheckboxProperty(Checkbox checkbox, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "checked":
                checkbox.Checked = bool.Parse(value);
                break;
            case "disabled":
                checkbox.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                checkbox.DisabledBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabledforegroundcolor":
                checkbox.DisabledForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetRadioButtonProperty(RadioButton radioButton, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "checked":
                radioButton.Selected = bool.Parse(value);
                break;
            case "disabled":
                radioButton.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                radioButton.DisabledBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabledforegroundcolor":
                radioButton.DisabledForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetProgressBarProperty(ProgressBar progressBar, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "value":
                progressBar.Value = double.Parse(value);
                break;
            case "mode":
                progressBar.Mode = Enum.Parse<ProgressBarMode>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetChartProperty(Chart chart, string propertyName, string value)
    {
        // Chart has no additional properties beyond IWidget
    }

    private static void SetSliderProperty(Slider slider, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "value":
                slider.Value = double.Parse(value);
                break;
            case "min":
                slider.Min = double.Parse(value);
                break;
            case "max":
                slider.Max = double.Parse(value);
                break;
            case "step":
                slider.Step = double.Parse(value);
                break;
            case "disabled":
                slider.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                slider.DisabledBackgroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
            case "disabledforegroundcolor":
                slider.DisabledForegroundColor = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetLineProperty(Line line, string propertyName, string value)
    {
        switch (propertyName)
        {
            case "orientation":
                line.Orientation = Enum.Parse<LineOrientation>(value, ignoreCase: true);
                break;
            case "type":
                line.Type = Enum.Parse<LineType>(value, ignoreCase: true);
                break;
        }
    }

    private static void SetTableProperty(Table table, string propertyName, string value)
    {
        // Table has no additional properties beyond IWidget
    }
}

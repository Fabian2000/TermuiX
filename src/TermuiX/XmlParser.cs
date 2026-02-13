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

        var styles = new List<(string? name, string? group, Dictionary<string, string> properties)>();
        var widget = ParseElement(root, styles);
        ApplyStyles(widget, styles);
        return widget;
    }

    private static void ApplyStyles(IWidget root, List<(string? name, string? group, Dictionary<string, string> properties)> styles)
    {
        foreach (var (name, group, properties) in styles)
        {
            ApplyStyleToTree(root, name, group, properties);
        }
    }

    private static void ApplyStyleToTree(IWidget widget, string? name, string? group, Dictionary<string, string> properties)
    {
        bool matches = false;

        // Match by Name (like CSS #id)
        if (name != null && widget.Name != null &&
            widget.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            matches = true;
        }

        // Match by Group (like CSS .class) — widget can have multiple space-separated groups
        if (group != null && widget.Group != null)
        {
            var widgetGroups = widget.Group.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var wg in widgetGroups)
            {
                if (wg.Equals(group, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                    break;
                }
            }
        }

        if (matches)
        {
            foreach (var (propName, propValue) in properties)
            {
                SetWidgetProperty(widget, propName, propValue);
            }
        }

        foreach (var child in widget.Children)
        {
            ApplyStyleToTree(child, name, group, properties);
        }
    }

    private static int CalculateDisplayWidth(string text)
    {
        int totalWidth = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            totalWidth += Widgets.Text.GetRuneDisplayWidth(rune);
        }
        return totalWidth;
    }

    private static IWidget ParseElement(XElement element, List<(string? name, string? group, Dictionary<string, string> properties)>? styles = null)
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
                    var childWidget = ParseElement(childElement, styles);
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
            "treeview" => new TreeView(),
            "line" => new Line(),
            "table" => new Table(),
            "stackpanel" => new StackPanel(),
            _ => throw new NotSupportedException($"Widget type '{element.Name.LocalName}' is not supported")
        };

        string? textContent = null;
        if (!string.IsNullOrWhiteSpace(element.Value))
        {
            textContent = element.Value.Trim().Replace("\\n", "\n");
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
                    // If Width was explicitly set (e.g. "100%"), text wrapping is dynamic
                    // and height depends on the actual rendered width. Use "auto" so the
                    // layout pass computes height from GetRaw() on every frame.
                    bool hasExplicitWidth = element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase));
                    if (hasExplicitWidth)
                    {
                        text.Height = "auto";
                    }
                    else
                    {
                        int lineCount = textContent.Split('\n').Length;
                        text.Height = $"{lineCount}ch";
                    }
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
                if (childElement.Name.LocalName.Equals("Style", StringComparison.OrdinalIgnoreCase))
                {
                    if (styles != null)
                    {
                        var styleAttrs = childElement.Attributes()
                            .ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.OrdinalIgnoreCase);
                        styleAttrs.TryGetValue("Name", out var styleName);
                        styleAttrs.TryGetValue("Group", out var styleGroup);
                        styleAttrs.Remove("Name");
                        styleAttrs.Remove("Group");
                        styles.Add((styleName, styleGroup, styleAttrs));
                    }
                    continue;
                }
                var childWidget = ParseElement(childElement, styles);
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
        else if (widget is TreeView treeView)
        {
            foreach (var childElement in element.Elements())
            {
                if (childElement.Name.LocalName.Equals("TreeNode", StringComparison.OrdinalIgnoreCase))
                {
                    var node = ParseTreeNode(childElement);
                    treeView.Root.AddChild(node);
                }
            }
        }

        return widget;
    }

    private static TreeNode ParseTreeNode(XElement element)
    {
        string text = element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName.Equals("Text", StringComparison.OrdinalIgnoreCase))?.Value
            ?? element.Value.Trim();

        bool expanded = element.Attributes()
            .Any(a => a.Name.LocalName.Equals("Expanded", StringComparison.OrdinalIgnoreCase)
                && bool.Parse(a.Value));

        var node = new TreeNode(text) { IsExpanded = expanded };

        foreach (var childElement in element.Elements())
        {
            if (childElement.Name.LocalName.Equals("TreeNode", StringComparison.OrdinalIgnoreCase))
            {
                node.AddChild(ParseTreeNode(childElement));
            }
        }

        return node;
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
                cell.BackgroundColor = Color.Parse(value);
                break;
            case "foregroundcolor":
                cell.ForegroundColor = Color.Parse(value);
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
            case "minwidth":
                widget.MinWidth = value;
                break;
            case "maxwidth":
                widget.MaxWidth = value;
                break;
            case "minheight":
                widget.MinHeight = value;
                break;
            case "maxheight":
                widget.MaxHeight = value;
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
                widget.BackgroundColor = Color.Parse(value);
                break;
            case "foregroundcolor":
                widget.ForegroundColor = Color.Parse(value);
                break;
            case "focusbackgroundcolor":
                widget.FocusBackgroundColor = Color.Parse(value);
                break;
            case "focusforegroundcolor":
                widget.FocusForegroundColor = Color.Parse(value);
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
            case "scrollx":
                container.ScrollX = bool.Parse(value);
                break;
            case "scrolly":
                container.ScrollY = bool.Parse(value);
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
                button.BorderColor = Color.Parse(value);
                break;
            case "textcolor":
                button.TextColor = Color.Parse(value);
                break;
            case "focusbordercolor":
                button.FocusBorderColor = Color.Parse(value);
                break;
            case "focustextcolor":
                button.FocusTextColor = Color.Parse(value);
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
                button.DisabledBackgroundColor = Color.Parse(value);
                break;
            case "disabledforegroundcolor":
                button.DisabledForegroundColor = Color.Parse(value);
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
            case "submitkey":
                input.SubmitKey = Enum.Parse<SubmitKeyMode>(value, ignoreCase: true);
                break;
            case "placeholder":
                input.Placeholder = value;
                break;
            case "bordercolor":
                input.BorderColor = Color.Parse(value);
                break;
            case "focusbordercolor":
                input.FocusBorderColor = Color.Parse(value);
                break;
            case "placeholdercolor":
                input.PlaceholderColor = Color.Parse(value);
                break;
            case "cursorcolor":
                input.CursorColor = Color.Parse(value);
                break;
            case "disabled":
                input.Disabled = bool.Parse(value);
                break;
            case "disabledbackgroundcolor":
                input.DisabledBackgroundColor = Color.Parse(value);
                break;
            case "disabledforegroundcolor":
                input.DisabledForegroundColor = Color.Parse(value);
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
                checkbox.DisabledBackgroundColor = Color.Parse(value);
                break;
            case "disabledforegroundcolor":
                checkbox.DisabledForegroundColor = Color.Parse(value);
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
                radioButton.DisabledBackgroundColor = Color.Parse(value);
                break;
            case "disabledforegroundcolor":
                radioButton.DisabledForegroundColor = Color.Parse(value);
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
                slider.DisabledBackgroundColor = Color.Parse(value);
                break;
            case "disabledforegroundcolor":
                slider.DisabledForegroundColor = Color.Parse(value);
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

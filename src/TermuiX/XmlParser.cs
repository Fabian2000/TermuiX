using System.Xml.Linq;
using TermuiX.Widgets;

namespace TermuiX;

internal static class XmlParser
{
    public static IWidget Parse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidOperationException("XML document has no root element");

        return ParseElement(root);
    }

    private static IWidget ParseElement(XElement element)
    {
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
                int autoWidth = textContent.Length + 4;
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
                    int maxWidth = lines.Max(line => line.Length);
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
        }

        // Widget-specific properties
        if (widget is Container container)
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

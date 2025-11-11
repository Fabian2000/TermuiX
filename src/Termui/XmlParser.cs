using System.Xml.Linq;
using Termui.Widgets;

namespace Termui;

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
            _ => throw new NotSupportedException($"Widget type '{element.Name.LocalName}' is not supported")
        };

        // Get text content first (needed for auto-sizing)
        string? textContent = null;
        if (!string.IsNullOrWhiteSpace(element.Value))
        {
            textContent = element.Value.Trim();
        }

        // Set text content for Text, Button, and Input widgets
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

        // Set properties from attributes
        foreach (var attr in element.Attributes())
        {
            SetProperty(widget, attr.Name.LocalName, attr.Value);
        }

        // Auto-size Button if Width or Height not specified
        if (widget is Button button && textContent is not null)
        {
            // Check if Width was explicitly set
            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase)))
            {
                // Auto width = text length + padding (1ch left + 1ch right) + border (2ch)
                int autoWidth = textContent.Length + 4;
                button.Width = $"{autoWidth}ch";
            }

            // Check if Height was explicitly set
            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Height", StringComparison.OrdinalIgnoreCase)))
            {
                // Auto height = 1 line + padding (1ch top + 1ch bottom) + border (2ch)
                int autoHeight = 3;
                button.Height = $"{autoHeight}ch";
            }
        }

        // Auto-size Text if Width or Height not specified
        if (widget is Text text && textContent is not null)
        {
            // Check if Width was explicitly set
            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Width", StringComparison.OrdinalIgnoreCase)))
            {
                // Auto width = longest line
                var lines = textContent.Split('\n');
                int maxWidth = lines.Max(line => line.Length);
                text.Width = $"{maxWidth}ch";
            }

            // Check if Height was explicitly set
            if (!element.Attributes().Any(a => a.Name.LocalName.Equals("Height", StringComparison.OrdinalIgnoreCase)))
            {
                // Auto height = number of lines
                int lineCount = textContent.Split('\n').Length;
                text.Height = $"{lineCount}ch";
            }
        }

        // Parse child elements
        if (widget is Container container)
        {
            foreach (var childElement in element.Elements())
            {
                var childWidget = ParseElement(childElement);
                container.Add(childWidget);
            }
        }

        return widget;
    }

    private static void SetProperty(IWidget widget, string propertyName, string value)
    {
        var prop = widget.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

        if (prop is null || !prop.CanWrite)
        {
            return; // Ignore unknown or read-only properties
        }

        object? convertedValue = null;

        if (prop.PropertyType == typeof(string))
        {
            convertedValue = value;
        }
        else if (prop.PropertyType == typeof(bool))
        {
            convertedValue = bool.Parse(value);
        }
        else if (prop.PropertyType == typeof(int))
        {
            convertedValue = int.Parse(value);
        }
        else if (prop.PropertyType == typeof(ConsoleColor))
        {
            convertedValue = Enum.Parse<ConsoleColor>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(BorderStyle))
        {
            convertedValue = Enum.Parse<BorderStyle>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(BorderStyle?))
        {
            convertedValue = Enum.Parse<BorderStyle>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(TextAlign))
        {
            convertedValue = Enum.Parse<TextAlign>(value, ignoreCase: true);
        }

        if (convertedValue is not null)
        {
            prop.SetValue(widget, convertedValue);
        }
    }
}

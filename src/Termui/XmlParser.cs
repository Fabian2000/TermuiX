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
            _ => throw new NotSupportedException($"Widget type '{element.Name.LocalName}' is not supported")
        };

        // Set properties from attributes
        foreach (var attr in element.Attributes())
        {
            SetProperty(widget, attr.Name.LocalName, attr.Value);
        }

        // Set text content for Text and Button widgets
        if (widget is Text textWidget && !string.IsNullOrWhiteSpace(element.Value))
        {
            textWidget.Content = element.Value.Trim();
        }
        else if (widget is Button buttonWidget && !string.IsNullOrWhiteSpace(element.Value))
        {
            buttonWidget.Text = element.Value.Trim();
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

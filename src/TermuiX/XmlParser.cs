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
            SetProperty(widget, attr.Name.LocalName, attr.Value);
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

        if (widget is Text text && textContent is not null)
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
            return;
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
        else if (prop.PropertyType == typeof(TextStyle))
        {
            convertedValue = Enum.Parse<TextStyle>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(ProgressBarMode))
        {
            convertedValue = Enum.Parse<ProgressBarMode>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(LineOrientation))
        {
            convertedValue = Enum.Parse<LineOrientation>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(LineType))
        {
            convertedValue = Enum.Parse<LineType>(value, ignoreCase: true);
        }
        else if (prop.PropertyType == typeof(double))
        {
            convertedValue = double.Parse(value);
        }
        else if (prop.PropertyType == typeof(char))
        {
            convertedValue = value.Length > 0 ? value[0] : ' ';
        }

        if (convertedValue is not null)
        {
            prop.SetValue(widget, convertedValue);
        }
    }
}

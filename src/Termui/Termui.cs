using Microsoft.CSharp.RuntimeBinder;

namespace Termui;

public sealed class Termui
{
    private IWidget? _widget = null;
    private readonly Renderer _renderer = new();

    private Termui() { }

    public static Termui Init()
    {
        Console.Clear();
        Console.CursorVisible = false;
        return new Termui();
    }

    public static void DeInit()
    {
        Console.CursorVisible = true;
    }

    public void AddToWindow(IWidget widget)
    {
        _widget = widget;
    }

    public void Render()
    {
        if (_widget is null)
        {
            throw new InvalidOperationException("No widget added to window. Call AddToWindow(widget) before Render().");
        }

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        // Fallback for non-interactive environments (testing)
        if (width == 0 || height == 0)
        {
            width = 80;
            height = 24;
            Console.WriteLine($"Warning: Console window size is 0. Using fallback size: {width}x{height}");
        }

        _renderer.Size(width, height);
        var output = _renderer.Render(_widget);

        try
        {
            Console.SetCursorPosition(0, 0);
        }
        catch (IOException)
        {
            // Cursor positioning not supported in this environment
        }
        catch (ArgumentOutOfRangeException)
        {
            // Console too small for cursor positioning
        }

        for (int x = 0; x < output.Length; x++)
        {
            try
            {
                Console.SetCursorPosition(0, x);
            }
            catch (IOException)
            {
                // Cursor positioning not supported
            }
            catch (ArgumentOutOfRangeException)
            {
                // Position out of range
            }
            Console.WriteLine(new string(output[x]));
        }
    }
}

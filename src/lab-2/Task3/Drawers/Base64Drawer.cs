using Microsoft.Extensions.Options;
using Spectre.Console;
using Task3.Interfaces;
using Task3.Options;

namespace Task3.Drawers;

public class Base64Drawer : IDrawer
{
    private readonly IOptionsMonitor<DisplayOptions> _options;

    public Base64Drawer(IOptionsMonitor<DisplayOptions> options)
    {
        _options = options;
    }

    public Task Draw()
    {
        string text = _options.CurrentValue.Content;
        byte[] bytes = Convert.FromBase64String(text);

        using var memoryStream = new MemoryStream(bytes);
        var image = new CanvasImage(memoryStream);

        AnsiConsole.Write(image);
        return Task.CompletedTask;
    }
}
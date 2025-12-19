using Microsoft.Extensions.Options;
using Spectre.Console;
using Task3.Interfaces;
using Task3.Options;

namespace Task3.Drawers;

public class FigletDrawer : IDrawer
{
    private readonly IOptionsMonitor<DisplayOptions> _options;

    public FigletDrawer(IOptionsMonitor<DisplayOptions> options)
    {
        _options = options;
    }

    public Task Draw()
    {
        string text = _options.CurrentValue.Content;

        var figletText = new FigletText(text);
        AnsiConsole.Write(figletText.Color(Color.Cyan2));
        return Task.CompletedTask;
    }
}
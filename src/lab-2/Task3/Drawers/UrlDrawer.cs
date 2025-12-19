using Microsoft.Extensions.Options;
using Spectre.Console;
using Task3.Interfaces;
using Task3.Options;

namespace Task3.Drawers;

public class UrlDrawer : IDrawer
{
    private readonly IOptionsMonitor<DisplayOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public UrlDrawer(IOptionsMonitor<DisplayOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Draw()
    {
        string url = _options.CurrentValue.Content;
        using HttpClient client = _httpClientFactory.CreateClient();
        byte[] bytes = await client.GetByteArrayAsync(url);

        using var memoryStream = new MemoryStream(bytes);
        var image = new CanvasImage(memoryStream);

        AnsiConsole.Write(image);
    }
}
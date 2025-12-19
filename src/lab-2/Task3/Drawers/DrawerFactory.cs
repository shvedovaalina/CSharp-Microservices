using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Task3.Interfaces;
using Task3.Options;

namespace Task3.Drawers;

public class DrawerFactory : IDrawerFactory
{
    private readonly IOptionsMonitor<DisplayOptions> _options;
    private readonly IServiceProvider _services;

    public DrawerFactory(IOptionsMonitor<DisplayOptions> options, IServiceProvider services)
    {
        _options = options;
        _services = services;
    }

    public IDrawer CreateDrawer()
    {
        string contentType = _options.CurrentValue.ContentType;
        return contentType switch
        {
            "figlet" => _services.GetRequiredService<FigletDrawer>(),
            "base64" => _services.GetRequiredService<Base64Drawer>(),
            "url" => _services.GetRequiredService<UrlDrawer>(),
            _ => throw new ArgumentException(),
        };
    }
}
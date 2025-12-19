using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Task3.Interfaces;
using Task3.Options;

namespace Task3.Services;

public class DrawingService : BackgroundService
{
    private readonly IOptionsMonitor<DisplayOptions> _options;
    private readonly IDrawerFactory _drawerFactory;

    public DrawingService(IOptionsMonitor<DisplayOptions> options, IDrawerFactory drawerFactory)
    {
        _options = options;
        _drawerFactory = drawerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _options.OnChange(async options =>
        {
            if (!string.IsNullOrEmpty(options.ContentType))
            {
                Console.Clear();
                IDrawer drawer = _drawerFactory.CreateDrawer();
                await drawer.Draw();
            }
        });

        if (!string.IsNullOrEmpty(_options.CurrentValue.ContentType))
        {
            Console.Clear();
            IDrawer drawer = _drawerFactory.CreateDrawer();
            await drawer.Draw();
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
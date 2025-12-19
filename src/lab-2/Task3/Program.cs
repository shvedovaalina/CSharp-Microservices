using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task1.Extensions;
using Task1.Options;
using Task2;
using Task2.Options;
using Task2.Provider;
using Task2.Sources;
using Task3.Drawers;
using Task3.Interfaces;
using Task3.Options;
using Task3.Services;

namespace Task3;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var cp = new CustomConfigurationProvider();
        IHostBuilder builder = Host.CreateDefaultBuilder();

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Add(new CustomConfigurationSource(cp));
        });

        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton(cp);
            services.Configure<ConfigurationClientOptions>(options =>
            {
                options.BaseUrl = "http://localhost:8080";
                options.PageSize = 10;
            });

            services.Configure<ConfigurationUpdateOptions>(options =>
            {
                options.UpdateInterval = 10;
            });

            services.Configure<DisplayOptions>(context.Configuration);
            services.AddRefitConfigurationService();
            services.AddHostedService<ConfigurationService>();
            services.AddHostedService<DrawingService>();
            services.AddSingleton<IDrawerFactory, DrawerFactory>();

            services.AddTransient<FigletDrawer>();
            services.AddTransient<Base64Drawer>();
            services.AddTransient<UrlDrawer>();
        });

        IHost host = builder.Build();
        await host.RunAsync();
    }
}
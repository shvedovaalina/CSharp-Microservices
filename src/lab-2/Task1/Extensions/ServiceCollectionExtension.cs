using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Task1.Interfaces;
using Task1.Options;
using Task1.Services;

namespace Task1.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHttpConfigurationService(this IServiceCollection services)
    {
        services.AddHttpClient<IConfigurationsRefit>()
            .ConfigureHttpClient((sp, client) =>
            {
                IOptions<ConfigurationClientOptions> options =
                    sp.GetRequiredService<IOptions<ConfigurationClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseUrl);
            });
        services.AddScoped<IConfigurationClient, HttpConfigurationClient>();
        return services;
    }

    public static IServiceCollection AddRefitConfigurationService(this IServiceCollection services)
    {
        services.AddRefitClient<IConfigurationsRefit>()
            .ConfigureHttpClient((sp, client) =>
            {
                IOptions<ConfigurationClientOptions> options =
                    sp.GetRequiredService<IOptions<ConfigurationClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseUrl);
            });

        services.AddTransient<IConfigurationClient, RefitConfigurationClient>();
        return services;
    }
}
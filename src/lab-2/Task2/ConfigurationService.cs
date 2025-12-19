using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Task1.Interfaces;
using Task1.Models;
using Task2.Options;
using Task2.Provider;

namespace Task2;

public class ConfigurationService : BackgroundService
{
    private readonly IConfigurationClient _client;
    private readonly CustomConfigurationProvider _provider;
    private readonly ConfigurationUpdateOptions _options;

    public ConfigurationService(IConfigurationClient client, CustomConfigurationProvider provider, IOptions<ConfigurationUpdateOptions> options)
    {
        _client = client;
        _provider = provider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var firstItems = new List<ConfigurationItem>();
        await foreach (ConfigurationItem configurationItem in _client.GetAllConfigurationsAsync(stoppingToken))
        {
            firstItems.Add(configurationItem);
        }

        _provider.OnConfigurationUpdated(firstItems);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.UpdateInterval));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            List<ConfigurationItem> items = await _client.GetAllConfigurationsAsync(stoppingToken)
                .ToListAsync(stoppingToken);

            _provider.OnConfigurationUpdated(items);
        }
    }
}
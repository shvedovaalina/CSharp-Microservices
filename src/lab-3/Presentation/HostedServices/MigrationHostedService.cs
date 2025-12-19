using Infrastucture.Extensions;

namespace Presentation.HostedServices;

public class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _serviceProvider.RunMigrationsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
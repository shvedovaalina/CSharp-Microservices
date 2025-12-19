using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastucture.Extensions;

public static class MigrationRunnerExtension
{
    public static async Task RunMigrationsAsync(this IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}

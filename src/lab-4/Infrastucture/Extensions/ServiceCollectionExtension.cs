using Core.Models.Enums;
using Core.Repository;
using FluentMigrator.Runner;
using Infrastucture.Options;
using Infrastucture.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastucture.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastucture(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            IOptions<DatabaseOptions> options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
            var builder = new NpgsqlDataSourceBuilder(options.Value.ConnectionString);

            builder.MapEnum<OrderState>("order_state");
            builder.MapEnum<OrderHistoryItemKind>("order_history_item_kind");
            return builder.Build();
        });
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services, string? connectionString)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                                 .AddPostgres()
                                 .WithGlobalConnectionString(connectionString)
                                 .WithMigrationsIn(typeof(ServiceCollectionExtension).Assembly));
        return services;
    }
}
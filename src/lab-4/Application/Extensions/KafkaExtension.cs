using Application.Handlers;
using Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Kafka.Contracts;

namespace Application.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaProducer<OrderCreationKey, OrderCreationValue>(configuration);
        services.AddConsumer<OrderProcessingKey, OrderProcessingValue, OrderProcessingHandler>(configuration);
        return services;
    }
}
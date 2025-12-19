using Application.Handlers;
using Kafka.Extensions;
using Orders.Kafka.Contracts;

namespace Presentation.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafkaExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaProducer<OrderCreationKey, OrderCreationValue>(configuration.GetSection("Producer"));
        services.AddConsumer<OrderProcessingKey, OrderProcessingValue, OrderProcessingHandler>(configuration.GetSection("Consumer"));
        return services;
    }
}
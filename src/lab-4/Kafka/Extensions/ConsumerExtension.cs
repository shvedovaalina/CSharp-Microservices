using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Consumer;
using Kafka.Interfaces;
using Kafka.Options;
using Kafka.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class ConsumerExtension
{
    public static IServiceCollection AddConsumer<TKey, TValue, THandler>(this IServiceCollection services, IConfiguration configuration)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
        where THandler : class, IMessageHandler<TKey, TValue>
    {
        services.Configure<ConsumerOptions>(options =>
        {
            configuration.Bind(options);
        });

        services.AddSingleton<IDeserializer<TKey>, ProtobufDeserializer<TKey>>();
        services.AddSingleton<IDeserializer<TValue>, ProtobufDeserializer<TValue>>();

        services.AddScoped<IMessageHandler<TKey, TValue>, THandler>();
        services.AddHostedService<ConsumerBackgroudService<TKey, TValue>>();

        return services;
    }
}
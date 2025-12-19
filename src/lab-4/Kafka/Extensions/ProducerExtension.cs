using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Interfaces;
using Kafka.Options;
using Kafka.Producer;
using Kafka.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class ProducerExtension
{
    public static IServiceCollection AddKafkaProducer<TKey, TValue>(this IServiceCollection services, IConfiguration configuration)
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
    {
        services.Configure<ProducerOptions>(options =>
        {
            configuration.Bind(options);
        });

        services.AddSingleton<ISerializer<TKey>, ProtobufSerializer<TKey>>();
        services.AddSingleton<ISerializer<TValue>, ProtobufSerializer<TValue>>();

        services.AddSingleton<IKafkaProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();
        return services;
    }
}
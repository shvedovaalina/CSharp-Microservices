using Confluent.Kafka;
using Kafka.Interfaces;
using Kafka.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafka.Consumer;

public class ConsumerBackgroudService<TKey, TValue> : BackgroundService
{
    private readonly IDeserializer<TKey> _keyDeserializer;
    private readonly IDeserializer<TValue> _valueDeserializer;
    private readonly ConsumerOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public ConsumerBackgroudService(IDeserializer<TKey> keyDeserializer, IDeserializer<TValue> valueDeserializer, IOptions<ConsumerOptions> options, IServiceProvider serviceProvider)
    {
        _keyDeserializer = keyDeserializer;
        _valueDeserializer = valueDeserializer;
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.Host,
            GroupId = _options.GroupId,
            EnableAutoCommit = false,
        };
        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeserializer)
            .Build();

        consumer.Subscribe(_options.Topic);

        try
        {
            while (stoppingToken.IsCancellationRequested is false)
            {
                var batch = new List<KafkaMessage<TKey, TValue>>();

                while (batch.Count < _options.BatchSize)
                {
                    ConsumeResult<TKey, TValue> result = consumer.Consume(_options.BatchTimeout);
                    if (result is not null)
                    {
                        batch.Add(new KafkaMessage<TKey, TValue>(result.Message.Key, result.Message.Value));
                    }
                    else
                    {
                        break;
                    }
                }

                if (batch.Count > 0)
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        IMessageHandler<TKey, TValue> handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TKey, TValue>>();
                        await handler.HandleAsync(batch, stoppingToken);
                    }

                    consumer.Commit();
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
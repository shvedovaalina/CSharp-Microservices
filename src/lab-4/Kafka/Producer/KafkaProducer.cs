using Confluent.Kafka;
using Kafka.Interfaces;
using Kafka.Options;
using Microsoft.Extensions.Options;

namespace Kafka.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IDisposable
{
    private readonly IProducer<TKey, TValue> _producer;

    private readonly IOptions<ProducerOptions> _options;

    public KafkaProducer(IOptions<ProducerOptions> options, ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = options.Value.Host,
        };
        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
        _options = options;
    }

    public async Task ProduceAsync(ProducerMessage<TKey, TValue> message, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<TKey, TValue>
        {
            Key = message.Key,
            Value = message.Value,
        };
        await _producer.ProduceAsync(_options.Value.Topic, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
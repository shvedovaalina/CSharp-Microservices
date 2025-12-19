using Kafka.Producer;

namespace Kafka.Interfaces;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(ProducerMessage<TKey, TValue> message, CancellationToken cancellationToken);
}

using Kafka.Consumer;

namespace Kafka.Interfaces;

public interface IMessageHandler<TKey, TValue>
{
    Task HandleAsync(IReadOnlyList<KafkaMessage<TKey, TValue>> messages, CancellationToken ct);
}
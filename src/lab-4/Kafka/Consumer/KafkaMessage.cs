namespace Kafka.Consumer;

public record KafkaMessage<TKey, TValue>(TKey Key, TValue Value);
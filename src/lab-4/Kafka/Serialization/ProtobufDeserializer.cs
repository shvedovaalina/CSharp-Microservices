using Confluent.Kafka;
using Google.Protobuf;

namespace Kafka.Serialization;

public class ProtobufDeserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new MessageParser<T>(() => new T());

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull is false)
        {
            return Parser.ParseFrom(data);
        }
        else
        {
            throw new ArgumentException("Нельзя десериализовать пустое сообщение ");
        }
    }
}
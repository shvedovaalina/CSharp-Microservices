using Lab1.Task3.Interfaces;
using Lab1.Task3.Models;
using System.Threading.Channels;

namespace Lab1.Task3.Implementations;

public class MessageProcessor(IReadOnlyList<IMessageHandler> handlers, Config config)
    : IMessageSender, IMessageProcessor
{
    private readonly Channel<Message> _channel = Channel.CreateBounded<Message>(
        new BoundedChannelOptions(config.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        IAsyncEnumerable<IReadOnlyList<Message>> chunks = _channel.Reader
            .ReadAllAsync(cancellationToken)
            .ChunkAsync(config.BatchSize, config.BatchingTimeout);

        await foreach (IReadOnlyList<Message> butch in chunks.WithCancellation(cancellationToken))
        {
            foreach (IMessageHandler handler in handlers)
            {
                await handler.HandleAsync(butch, cancellationToken);
            }
        }
    }

    public ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }
}
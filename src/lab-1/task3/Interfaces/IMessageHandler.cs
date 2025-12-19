using Lab1.Task3.Models;

namespace Lab1.Task3.Interfaces;

public interface IMessageHandler
{
    ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken);
}
using Lab1.Task3.Models;

namespace Lab1.Task3.Interfaces;

public interface IMessageSender
{
    ValueTask SendAsync(Message message, CancellationToken cancellationToken);
}
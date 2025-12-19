namespace Lab1.Task3.Interfaces;

public interface IMessageProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);

    void Complete();
}
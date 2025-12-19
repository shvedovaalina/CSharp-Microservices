using Lab1.Task3.Implementations;
using Lab1.Task3.Interfaces;
using Lab1.Task3.Models;

namespace Lab1.Task3;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var handlers = new IMessageHandler[]
        {
            new ConsoleMessageHandler(),
        };

        var config = new Config(4, 4, TimeSpan.FromMilliseconds(666));

        IMessageProcessor messageProcessor = new MessageProcessor(handlers, config);

        IMessageProcessor processor = messageProcessor;
        var sender = (IMessageSender)messageProcessor;

        var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        Task processingTask = processor.ProcessAsync(cts.Token);
        IEnumerable<int> numbers = Enumerable.Range(1, 10);

        var po = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token,
        };

        Task parallelWorker = Parallel.ForEachAsync(
            numbers,
            po,
            async (i, cancellationToken) =>
            {
                string title = i.ToString();
                string description = i.ToString();

                var message = new Message(title, description);
                await sender.SendAsync(message, cancellationToken);
            });

        await parallelWorker;

        processor.Complete();
        await processingTask;
    }
}
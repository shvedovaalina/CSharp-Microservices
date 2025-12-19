using Lab1.Task3.Interfaces;
using Lab1.Task3.Models;
using System.Text;

namespace Lab1.Task3.Implementations;

public class ConsoleMessageHandler : IMessageHandler
{
    public ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        var stringBuilder = new StringBuilder();

        foreach (Message message in messages)
        {
            stringBuilder.AppendLine(message.Title + " " + message.Text);
        }

        Console.WriteLine(stringBuilder.ToString());

        return ValueTask.CompletedTask;
    }
}
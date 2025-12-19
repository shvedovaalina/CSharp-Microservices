namespace Kafka.Options;

public class ConsumerOptions
{
    public string Topic { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int BatchSize { get; set; }

    public TimeSpan BatchTimeout { get; set; } = TimeSpan.Zero;
}
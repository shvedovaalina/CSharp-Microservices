namespace Lab1.Task3.Models;

public record Config(int ChannelCapacity, int BatchSize, TimeSpan BatchingTimeout);
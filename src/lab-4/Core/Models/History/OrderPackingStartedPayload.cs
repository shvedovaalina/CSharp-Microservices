namespace Core.Models.History;

public class OrderPackingStartedPayload : OrderHistoryPayload
{
    public string PackingBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
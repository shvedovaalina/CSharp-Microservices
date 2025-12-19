namespace Core.Models.History;

public class OrderDeliveryStartedPayload : OrderHistoryPayload
{
    public string DeliveredBy { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }
}
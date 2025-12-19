namespace Core.Models.History;

public class OrderApprovalReceivedPayload : OrderHistoryPayload
{
    public bool IsApproved { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
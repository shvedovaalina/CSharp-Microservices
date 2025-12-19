namespace Core.Models.History;

public class OrderDeliveryFinishedPayload : OrderHistoryPayload
{
    public DateTimeOffset FinishedAt { get; set; }

    public bool IsFinishedSuccessfully { get; set; }

    public string FailureReason { get; set; } = string.Empty;
}
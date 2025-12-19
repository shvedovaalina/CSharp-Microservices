namespace Gateway.History;

public class OrderCreatedPayload : OrderHistoryPayload
{
    public string CreatedBy { get; set; } = string.Empty;
}
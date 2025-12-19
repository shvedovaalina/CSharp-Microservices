namespace Gateway.History;

public class ItemRemovedPayload : OrderHistoryPayload
{
    public long ProductId { get; set; }
}
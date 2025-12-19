namespace Core.Models.History;

public class ItemRemovedPayload : OrderHistoryPayload
{
    public long ProductId { get; set; }
}
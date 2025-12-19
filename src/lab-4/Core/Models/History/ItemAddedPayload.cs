namespace Core.Models.History;

public class ItemAddedPayload : OrderHistoryPayload
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}
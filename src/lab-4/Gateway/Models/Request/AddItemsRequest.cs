namespace Gateway.Models.Request;

public class AddItemsRequest
{
    public IReadOnlyList<OrderItemDto> OrderItems { get; set; } = [];
}
namespace Gateway.Models.Request;

public class DeleteItemsRequest
{
    public long OrderId { get; set; }

    public long ProductId { get; set; }
}
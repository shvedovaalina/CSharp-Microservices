using Gateway.History;
using Gateway.Models.Enums;

namespace Gateway.Models.Response;

public class OrderHistoryItemResponse
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public OrderHistoryKind Kind { get; set; }

    public OrderHistoryPayload? Payload { get; set; }
}

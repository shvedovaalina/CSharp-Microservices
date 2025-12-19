using Core.Models.Enums;
using Core.Models.History;

namespace Core.Models;

public class OrderHistoryItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public OrderHistoryItemKind Kind { get; set; }

    public OrderHistoryPayload? Payload { get; set; }
}
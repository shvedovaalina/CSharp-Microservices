using Core.Models.Enums;

namespace Core.Models;

public class Order
{
    public long OrderId { get; set; }

    public OrderState OrderState { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
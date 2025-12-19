using Core.Models.Enums;

namespace Core.Models.History;

public class StateChangedPayload : OrderHistoryPayload
{
    public OrderState OldState { get; set; }

    public OrderState NewState { get; set; }
}
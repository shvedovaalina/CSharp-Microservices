using Gateway.Models.Enums;

namespace Gateway.History;

public class StateChangedPayload : OrderHistoryPayload
{
    public OrderState OldState { get; set; }

    public OrderState NewState { get; set; }
}
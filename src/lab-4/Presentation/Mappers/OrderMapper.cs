using Google.Protobuf.WellKnownTypes;

namespace Presentation.Mappers;

public static class OrderMapper
{
    public static Core.Models.OrderItem ToDomainOrderItem(this OrderItem protoItem, long orderId)
    {
        return new Core.Models.OrderItem
        {
            OrderId = orderId,
            ProductId = protoItem.ProductId,
            Quantity = protoItem.Quantity,
        };
    }

    public static OrderHistoryItem ToProtoHistoryItem(this Core.Models.OrderHistoryItem item)
    {
        var protoItem = new OrderHistoryItem
        {
            Id = item.Id,
            OrderId = item.OrderId,
            CreatedAt = Timestamp.FromDateTimeOffset(item.CreatedAt),
            Kind = item.Kind switch
            {
                Core.Models.Enums.OrderHistoryItemKind.Created => OrderHistoryKind.Created,
                Core.Models.Enums.OrderHistoryItemKind.ItemAdded => OrderHistoryKind.ItemAdded,
                Core.Models.Enums.OrderHistoryItemKind.ItemRemoved => OrderHistoryKind.ItemRemoved,
                Core.Models.Enums.OrderHistoryItemKind.StateChanged => OrderHistoryKind.StateChanged,
                _ => OrderHistoryKind.Unspecified,
            },
        };
        switch (item.Payload)
        {
            case Core.Models.History.OrderCreatedPayload createdPayload:
                protoItem.Created = new OrderCreatedPayload
                {
                    CreatedBy = createdPayload.CreatedBy,
                };
                break;
            case Core.Models.History.ItemAddedPayload itemAddedPayload:
                protoItem.ItemAdded = new ItemAddedPayload
                {
                    ProductId = itemAddedPayload.ProductId,
                    Quantity = itemAddedPayload.Quantity,
                };
                break;
            case Core.Models.History.ItemRemovedPayload itemRemovedPayload:
                protoItem.ItemRemoved = new ItemRemovedPayload
                {
                    ProductId = itemRemovedPayload.ProductId,
                };
                break;
            case Core.Models.History.StateChangedPayload stateChangedPayload:
                protoItem.StateChanged = new StateChangedPayload
                {
                    OldState = stateChangedPayload.OldState.ToOrderState(),
                    NewState = stateChangedPayload.NewState.ToOrderState(),
                };
                break;
        }

        return protoItem;
    }

    public static OrderState ToOrderState(this Core.Models.Enums.OrderState state)
    {
        return state switch
        {
            Core.Models.Enums.OrderState.Created => OrderState.Created,
            Core.Models.Enums.OrderState.Processing => OrderState.Processing,
            Core.Models.Enums.OrderState.Completed => OrderState.Completed,
            Core.Models.Enums.OrderState.Cancelled => OrderState.Cancelled,
            _ => OrderState.Unspecified,
        };
    }
}

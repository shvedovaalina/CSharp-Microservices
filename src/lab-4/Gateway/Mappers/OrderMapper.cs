using Gateway.Models.Request;
using Gateway.Models.Response;
using Presentation;
using AddItemsRequest = Gateway.Models.Request.AddItemsRequest;
using CreateOrderRequest = Gateway.Models.Request.CreateOrderRequest;
using ItemAddedPayload = Gateway.History.ItemAddedPayload;
using ItemRemovedPayload = Gateway.History.ItemRemovedPayload;
using OrderCreatedPayload = Gateway.History.OrderCreatedPayload;
using StateChangedPayload = Gateway.History.StateChangedPayload;

namespace Gateway.Mappers;

public static class OrderMapper
{
    public static Presentation.CreateOrderRequest ToGrpcCreateOrderRequest(CreateOrderRequest httpRequest)
    {
        return new Presentation.CreateOrderRequest
        {
            CreatedBy = httpRequest.CreatedBy,
        };
    }

    public static Presentation.AddItemsRequest ToGrpcAddItemsRequest(long orderId, AddItemsRequest httpRequest)
    {
        var grpcRequest = new Presentation.AddItemsRequest
        {
            OrderId = orderId,
        };
        foreach (OrderItemDto item in httpRequest.OrderItems)
        {
            grpcRequest.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return grpcRequest;
    }

    public static Presentation.DeleteItemsRequest ToGrpcDeleteItemRequest(long orderId, long productId)
    {
        return new Presentation.DeleteItemsRequest
        {
            OrderId = orderId,
            ProductId = productId,
        };
    }

    public static StartProcessingRequest ToGrpcStartProcessingRequest(long orderId)
    {
        return new StartProcessingRequest
        {
            OrderId = orderId,
        };
    }

    public static CompleteRequest ToGrpcCompleteRequest(long orderId)
    {
        return new CompleteRequest
        {
            OrderId = orderId,
        };
    }

    public static CancelRequest ToGrpcCancelRequest(long orderId)
    {
        return new CancelRequest
        {
            OrderId = orderId,
        };
    }

    public static GetHistoryRequest ToGrpcGetHistoryRequest(long orderId, long cursor, int pageSize)
    {
        return new GetHistoryRequest
        {
            OrderId = orderId,
            Cursor = cursor,
            PageSize = pageSize,
        };
    }

    public static Models.Response.CreateOrderResponse ToHttpCreateOrderResponse(
        Presentation.CreateOrderResponse grpcResponse,
        CancellationToken ct)
    {
        return new Models.Response.CreateOrderResponse
        {
            OrderId = grpcResponse.OrderId,
        };
    }

    public static OrderHistoryItemResponse ToHttpOrderHistoryItem(
        OrderHistoryItem grpcResponse,
        CancellationToken ct)
    {
        return new OrderHistoryItemResponse
        {
            Id = grpcResponse.Id,
            OrderId = grpcResponse.OrderId,
            CreatedAt = grpcResponse.CreatedAt.ToDateTimeOffset(),
            Kind = MapOrderHistoryKind(grpcResponse.Kind),
            Payload = MapOrderHistoryPayload(grpcResponse),
        };
    }

    public static Models.Enums.OrderHistoryKind MapOrderHistoryKind(OrderHistoryKind grpcKind)
    {
        return grpcKind switch
        {
            OrderHistoryKind.Created => Models.Enums.OrderHistoryKind.Created,
            OrderHistoryKind.ItemAdded => Models.Enums.OrderHistoryKind.ItemAdded,
            OrderHistoryKind.ItemRemoved => Models.Enums.OrderHistoryKind.ItemRemoved,
            OrderHistoryKind.StateChanged => Models.Enums.OrderHistoryKind.StateChanged,
            OrderHistoryKind.Unspecified => throw new ArgumentException(":("),
            _ => throw new ArgumentException(":("),
        };
    }

    public static History.OrderHistoryPayload? MapOrderHistoryPayload(OrderHistoryItem grpcItem)
    {
        return grpcItem.PayloadCase switch
        {
            OrderHistoryItem.PayloadOneofCase.Created => new OrderCreatedPayload
            {
                CreatedBy = grpcItem.Created.CreatedBy,
            },
            OrderHistoryItem.PayloadOneofCase.ItemAdded => new ItemAddedPayload
            {
                ProductId = grpcItem.ItemAdded.ProductId,
                Quantity = grpcItem.ItemAdded.Quantity,
            },
            OrderHistoryItem.PayloadOneofCase.ItemRemoved => new ItemRemovedPayload
            {
                ProductId = grpcItem.ItemRemoved.ProductId,
            },
            OrderHistoryItem.PayloadOneofCase.StateChanged => new StateChangedPayload
            {
                OldState = MapOrderState(grpcItem.StateChanged.OldState),
                NewState = MapOrderState(grpcItem.StateChanged.NewState),
            },
            OrderHistoryItem.PayloadOneofCase.None => null,
            _ => throw new ArgumentException(":("),
        };
    }

    public static Models.Enums.OrderState MapOrderState(OrderState grpcState)
    {
        return grpcState switch
        {
            OrderState.Created => Models.Enums.OrderState.Created,
            OrderState.Processing => Models.Enums.OrderState.Processing,
            OrderState.Cancelled => Models.Enums.OrderState.Cancelled,
            OrderState.Completed => Models.Enums.OrderState.Completed,
            OrderState.Unspecified => throw new ArgumentException(":("),
            _ => throw new ArgumentException(":("),
        };
    }
}
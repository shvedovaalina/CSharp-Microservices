using Grpc.Core;
using Presentation.Mappers;

namespace Presentation.Services;

public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly Application.Services.OrderService _orderService;

    public OrderGrpcService(Application.Services.OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        Core.Models.Order order = await _orderService.CreateOrdersTransactionAsync(request.CreatedBy, context.CancellationToken);

        return new CreateOrderResponse
        {
            OrderId = order.OrderId,
        };
    }

    public override async Task<AddItemsResponse> AddItems(AddItemsRequest request, ServerCallContext context)
    {
        var orderItems = request.Items.Select(item => item.ToDomainOrderItem(request.OrderId)).ToList();
        await _orderService.AddItemsAsync(request.OrderId, orderItems, context.CancellationToken);
        return new AddItemsResponse();
    }

    public override async Task<DeleteItemsResponse> DeleteItems(DeleteItemsRequest request, ServerCallContext context)
    {
        await _orderService.DeleteItemsAsync(request.OrderId, request.ProductId, context.CancellationToken);
        return new DeleteItemsResponse();
    }

    public override async Task<StartProcessingResponse> StartProcessing(StartProcessingRequest request, ServerCallContext context)
    {
        await _orderService.StartProcessingAsync(request.OrderId, context.CancellationToken);
        return new StartProcessingResponse();
    }

    public override async Task<CompleteResponse> Complete(CompleteRequest request, ServerCallContext context)
    {
        await _orderService.CompleteAsync(request.OrderId, context.CancellationToken);
        return new CompleteResponse();
    }

    public override async Task<CancelResponse> Cancel(CancelRequest request, ServerCallContext context)
    {
        await _orderService.CancelAsync(request.OrderId, context.CancellationToken);
        return new CancelResponse();
    }

    public override async Task GetHistory(GetHistoryRequest request, IServerStreamWriter<OrderHistoryItem> responseStream, ServerCallContext context)
    {
        await foreach (Core.Models.OrderHistoryItem historyItem in _orderService.GetHistoryAsync(
                           request.OrderId,
                           request.Cursor,
                           request.PageSize,
                           context.CancellationToken))
        {
            OrderHistoryItem protoItem = historyItem.ToProtoHistoryItem();
            await responseStream.WriteAsync(protoItem);
        }
    }
}
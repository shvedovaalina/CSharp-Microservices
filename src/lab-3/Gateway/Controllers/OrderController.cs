using Gateway.Mappers;
using Gateway.Models.Response;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Presentation;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly OrderService.OrderServiceClient _orderService;

    public OrderController(OrderService.OrderServiceClient orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Models.Response.CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Models.Response.CreateOrderResponse>> CreateOrderAsync(
        [FromBody] Models.Request.CreateOrderRequest request,
        CancellationToken ct)
    {
        CreateOrderRequest grpcRequest = OrderMapper.ToGrpcCreateOrderRequest(request);
        Presentation.CreateOrderResponse grpcResponse = await _orderService.CreateOrderAsync(grpcRequest);
        Models.Response.CreateOrderResponse httpResponse =
            OrderMapper.ToHttpCreateOrderResponse(grpcResponse, ct);
        return Ok(httpResponse);
    }

    [HttpPost("{orderId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddItemsAsync(
        [FromRoute] long orderId,
        [FromBody] Models.Request.AddItemsRequest itemsRequest,
        CancellationToken ct)
    {
        AddItemsRequest grpcRequest = OrderMapper.ToGrpcAddItemsRequest(orderId, itemsRequest);
        await _orderService.AddItemsAsync(grpcRequest);
        return Ok();
    }

    [HttpDelete("{orderId}/items/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteItemsAsync(
        [FromRoute] long orderId,
        [FromRoute] long productId,
        CancellationToken ct)
    {
        DeleteItemsRequest grpcRequest = OrderMapper.ToGrpcDeleteItemRequest(orderId, productId);
        await _orderService.DeleteItemsAsync(grpcRequest);
        return Ok();
    }

    [HttpPost("{orderId}/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> StartProcessingAsync([FromRoute] long orderId, CancellationToken ct)
    {
        StartProcessingRequest grpcRequest = OrderMapper.ToGrpcStartProcessingRequest(orderId);
        await _orderService.StartProcessingAsync(grpcRequest);
        return Ok();
    }

    [HttpPost("{orderId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CompleteAsync([FromRoute] long orderId, CancellationToken ct)
    {
        CompleteRequest grpcRequest = OrderMapper.ToGrpcCompleteRequest(orderId);
        await _orderService.CompleteAsync(grpcRequest);
        return Ok();
    }

    [HttpPost("{orderId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelAsync([FromRoute] long orderId, CancellationToken ct)
    {
        CancelRequest grpcRequest = OrderMapper.ToGrpcCancelRequest(orderId);
        await _orderService.CancelAsync(grpcRequest);
        return Ok();
    }

    [HttpGet("{orderId}/history")]
    [ProducesResponseType(typeof(OrderHistoryItemResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderHistoryItemResponse[]>> GetHistoryAsync(
        [FromRoute] long orderId,
        [FromQuery] long cursor,
        [FromQuery] int pageSize,
        CancellationToken ct)
    {
        GetHistoryRequest grpcRequest = OrderMapper.ToGrpcGetHistoryRequest(orderId, cursor, pageSize);
        AsyncServerStreamingCall<OrderHistoryItem> response = _orderService.GetHistory(grpcRequest);
        var history = new List<OrderHistoryItemResponse>();

        await foreach (OrderHistoryItem grpcItem in response.ResponseStream.ReadAllAsync(ct))
        {
            OrderHistoryItemResponse httpItem = OrderMapper.ToHttpOrderHistoryItem(grpcItem, ct);
            history.Add(httpItem);
        }

        return Ok(history.ToArray());
    }
}
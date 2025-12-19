using Microsoft.AspNetCore.Mvc;
using Orders.ProcessingService.Contracts;

namespace Gateway.Controllers;

[ApiController]
[Route("api/orderprocessing")]
[Produces("application/json")]
public class OrderProcessingController : ControllerBase
{
    private readonly OrderService.OrderServiceClient _orderService;

    public OrderProcessingController(OrderService.OrderServiceClient orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("{orderId}/approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> OrderApprovalReceived(
        [FromRoute] long orderId,
        [FromBody] ApproveOrderRequest approveOrderRequest,
        CancellationToken ct)
    {
        approveOrderRequest.OrderId = orderId;
        await _orderService.ApproveOrderAsync(approveOrderRequest);
        return Ok();
    }

    [HttpPost("{orderId}/packing/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> StartPacking(
        [FromRoute] long orderId,
        [FromBody] StartOrderPackingRequest startOrderPackingRequest,
        CancellationToken ct)
    {
        startOrderPackingRequest.OrderId = orderId;
        await _orderService.StartOrderPackingAsync(startOrderPackingRequest);
        return Ok();
    }

    [HttpPost("{orderId}/packing/finish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PackingFinished(
        [FromRoute] long orderId,
        [FromBody] FinishOrderPackingRequest finishedPackingRequest,
        CancellationToken ct)
    {
        finishedPackingRequest.OrderId = orderId;
        await _orderService.FinishOrderPackingAsync(finishedPackingRequest);
        return Ok();
    }

    [HttpPost("{orderId}/delivery/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeliveryStarted(
        [FromRoute] long orderId,
        [FromBody] StartOrderDeliveryRequest startOrderDeliveryRequest,
        CancellationToken ct)
    {
        startOrderDeliveryRequest.OrderId = orderId;
        await _orderService.StartOrderDeliveryAsync(startOrderDeliveryRequest);
        return Ok();
    }

    [HttpPost("{orderId}/delivery/finish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeliveryFinished(
        [FromRoute] long orderId,
        [FromBody] FinishOrderDeliveryRequest finishedDeliveryRequest,
        CancellationToken ct)
    {
        finishedDeliveryRequest.OrderId = orderId;
        await _orderService.FinishOrderDeliveryAsync(finishedDeliveryRequest);
        return Ok();
    }
}
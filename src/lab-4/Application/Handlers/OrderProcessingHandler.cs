using Core.Models;
using Core.Models.Enums;
using Core.Models.History;
using Core.Repository;
using Kafka.Consumer;
using Kafka.Interfaces;
using Orders.Kafka.Contracts;
using System.Transactions;

namespace Application.Handlers;

public class OrderProcessingHandler : IMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly IOrderRepository _orderRepository;

    public OrderProcessingHandler(IOrderHistoryRepository orderHistoryRepository, IOrderRepository orderRepository)
    {
        _orderHistoryRepository = orderHistoryRepository;
        _orderRepository = orderRepository;
    }

    public async Task HandleAsync(
        IReadOnlyList<KafkaMessage<OrderProcessingKey, OrderProcessingValue>> messages,
        CancellationToken ct)
    {
        foreach (KafkaMessage<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            await MainHandlerAsync(message, ct);
        }
    }

    public async Task MainHandlerAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken ct)
    {
        long orderId = message.Key.OrderId;
        switch (message.Value.EventCase)
        {
            case OrderProcessingValue.EventOneofCase.ApprovalReceived:
                await HandlerApprovalReceivedAsync(orderId, message.Value.ApprovalReceived, ct);
                break;
            case OrderProcessingValue.EventOneofCase.PackingStarted:
                await HandlerOrderPackingStartedAsync(orderId, message.Value.PackingStarted, ct);
                break;
            case OrderProcessingValue.EventOneofCase.PackingFinished:
                await HandlerOrderPackingFinishedAsync(orderId, message.Value.PackingFinished, ct);
                break;
            case OrderProcessingValue.EventOneofCase.DeliveryStarted:
                await HandlerOrderDeliveryStartedAsync(orderId, message.Value.DeliveryStarted, ct);
                break;
            case OrderProcessingValue.EventOneofCase.DeliveryFinished:
                await HandlerOrderDeliveryFinishedAsync(orderId, message.Value.DeliveryFinished, ct);
                break;
            case OrderProcessingValue.EventOneofCase.None:
                break;
        }
    }

    private async Task HandlerApprovalReceivedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderApprovalReceived approvalReceived,
        CancellationToken ct)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (!approvalReceived.IsApproved)
        {
            await _orderRepository.UpdateStateAsync(orderId, OrderState.Cancelled, ct);
        }

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = approvalReceived.CreatedAt.ToDateTimeOffset(),
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new OrderApprovalReceivedPayload()
                {
                    CreatedAt = approvalReceived.CreatedAt.ToDateTimeOffset(),
                    CreatedBy = approvalReceived.CreatedBy,
                    IsApproved = approvalReceived.IsApproved,
                },
            },
            ct);

        transaction.Complete();
    }

    private async Task HandlerOrderPackingStartedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderPackingStarted orderPackingStarted,
        CancellationToken ct)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = orderPackingStarted.StartedAt.ToDateTimeOffset(),
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new OrderPackingStartedPayload
                {
                    PackingBy = orderPackingStarted.PackingBy,
                    CreatedAt = orderPackingStarted.StartedAt.ToDateTimeOffset(),
                },
            },
            ct);

        transaction.Complete();
    }

    private async Task HandlerOrderPackingFinishedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderPackingFinished orderPackingFinished,
        CancellationToken ct)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (!orderPackingFinished.IsFinishedSuccessfully)
        {
            await _orderRepository.UpdateStateAsync(orderId, OrderState.Cancelled, ct);
        }

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = orderPackingFinished.FinishedAt.ToDateTimeOffset(),
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new OrderPackingFinishedPayload
                {
                    FinishedAt = orderPackingFinished.FinishedAt.ToDateTimeOffset(),
                    IsFinishedSuccessfully = orderPackingFinished.IsFinishedSuccessfully,
                    FailureReason = orderPackingFinished.FailureReason,
                },
            },
            ct);

        transaction.Complete();
    }

    private async Task HandlerOrderDeliveryStartedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderDeliveryStarted orderDeliveryStarted,
        CancellationToken ct)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = orderDeliveryStarted.StartedAt.ToDateTimeOffset(),
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new OrderDeliveryStartedPayload()
                {
                    DeliveredBy = orderDeliveryStarted.DeliveredBy,
                    StartedAt = orderDeliveryStarted.StartedAt.ToDateTimeOffset(),
                },
            },
            ct);

        transaction.Complete();
    }

    private async Task HandlerOrderDeliveryFinishedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderDeliveryFinished orderDeliveryFinished,
        CancellationToken ct)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (orderDeliveryFinished.IsFinishedSuccessfully)
        {
            await _orderRepository.UpdateStateAsync(orderId, OrderState.Completed, ct);
        }
        else
        {
            await _orderRepository.UpdateStateAsync(orderId, OrderState.Cancelled, ct);
        }

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = orderDeliveryFinished.FinishedAt.ToDateTimeOffset(),
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new OrderDeliveryFinishedPayload
                {
                    FinishedAt = orderDeliveryFinished.FinishedAt.ToDateTimeOffset(),
                    IsFinishedSuccessfully = orderDeliveryFinished.IsFinishedSuccessfully,
                    FailureReason = orderDeliveryFinished.FailureReason,
                },
            },
            ct);

        transaction.Complete();
    }
}
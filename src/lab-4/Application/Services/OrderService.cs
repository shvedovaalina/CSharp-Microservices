using Core.Filters;
using Core.Models;
using Core.Models.Enums;
using Core.Models.History;
using Core.Repository;
using Google.Protobuf.WellKnownTypes;
using Kafka.Interfaces;
using Kafka.Producer;
using Orders.Kafka.Contracts;
using System.Transactions;

namespace Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IKafkaProducer<OrderCreationKey, OrderCreationValue> _producer;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderHistoryRepository orderHistoryRepository,
        IOrderItemRepository orderItemRepository,
        IKafkaProducer<OrderCreationKey, OrderCreationValue> kafkaProducer)
    {
        _orderRepository = orderRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _orderItemRepository = orderItemRepository;
        _producer = kafkaProducer;
    }

    public async Task<Order> CreateOrdersTransactionAsync(
        string createdBy,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var order = new Order
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy,
            OrderState = OrderState.Created,
        };

        Order createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = createdOrder.OrderId,
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = OrderHistoryItemKind.Created,
                Payload = new OrderCreatedPayload
                {
                    CreatedBy = createdBy,
                },
            },
            cancellationToken);

        var kafkaMessage = new ProducerMessage<OrderCreationKey, OrderCreationValue>(
            new OrderCreationKey
            {
                OrderId = createdOrder.OrderId,
            },
            new OrderCreationValue
            {
                OrderCreated = new OrderCreationValue.Types.OrderCreated
                {
                    OrderId = createdOrder.OrderId,
                    CreatedAt = Timestamp.FromDateTimeOffset(createdOrder.CreatedAt),
                },
            });
        await _producer.ProduceAsync(kafkaMessage, cancellationToken);
        transaction.Complete();
        return createdOrder;
    }

    public async Task<OrderItem[]> AddItemsAsync(
        long orderId,
        IReadOnlyList<OrderItem> items,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var orderfilter = new OrderFilter(new[] { orderId }, null, null, 0, 1);
        Order? order = null;
        await foreach (Order orderNeedToSearch in _orderRepository.SearchAsync(orderfilter, cancellationToken))
        {
            order = orderNeedToSearch;
            break;
        }

        if (order == null)
        {
            throw new ArgumentException("Заказ с таким Id не найден");
        }

        if (order.OrderState != OrderState.Created)
        {
            throw new ArgumentException("Статус заказа не created");
        }

        var addedItems = new List<OrderItem>();

        await foreach (OrderItem orderItem in _orderItemRepository.AddManyAsync(items, cancellationToken))
        {
            addedItems.Add(orderItem);
            await _orderHistoryRepository.AddAsync(
                new OrderHistoryItem
                {
                    OrderId = orderId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Kind = OrderHistoryItemKind.ItemAdded,
                    Payload = new ItemAddedPayload
                    {
                        ProductId = orderItem.ProductId,
                        Quantity = orderItem.Quantity,
                    },
                },
                cancellationToken);
        }

        transaction.Complete();
        return addedItems.ToArray();
    }

    public async Task DeleteItemsAsync(long orderId, long productId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var orderFilter = new OrderFilter(new[] { orderId }, null, null, 0, 1);
        Order? order = null;
        await foreach (Order orderNeedToSearch in _orderRepository.SearchAsync(
                           orderFilter,
                           cancellationToken))
        {
            order = orderNeedToSearch;
            break;
        }

        if (order == null)
        {
            throw new ArgumentException("Заказ не найден");
        }

        if (order.OrderState != OrderState.Created)
        {
            throw new ArgumentException("Состояние не created");
        }

        await _orderItemRepository.DeleteAsync(orderId, productId, cancellationToken);

        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = OrderHistoryItemKind.ItemRemoved,
                Payload = new ItemRemovedPayload
                {
                    ProductId = productId,
                },
            },
            cancellationToken);
        transaction.Complete();
    }

    public async Task StartProcessingAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var orderFilter = new OrderFilter(new long[] { orderId }, null, null, 0, 1);
        Order? order = null;
        await foreach (Order orderNeedToSearch in _orderRepository.SearchAsync(
                           orderFilter,
                           cancellationToken))
        {
            order = orderNeedToSearch;
            break;
        }

        if (order == null)
        {
            throw new ArgumentException("Заказ не найден");
        }

        if (order.OrderState != OrderState.Created)
        {
            throw new ArgumentException("Заказ не в статусе created");
        }

        await _orderRepository.UpdateStateAsync(
            orderId,
            OrderState.Processing,
            cancellationToken);
        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new StateChangedPayload
                {
                    OldState = order.OrderState,
                    NewState = OrderState.Processing,
                },
            },
            cancellationToken);
        var kafkaMessage = new ProducerMessage<OrderCreationKey, OrderCreationValue>(
            new OrderCreationKey
            {
                OrderId = orderId,
            },
            new OrderCreationValue
            {
                OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
                {
                    OrderId = orderId,
                    StartedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
                },
            });
        await _producer.ProduceAsync(kafkaMessage, cancellationToken);

        transaction.Complete();
    }

    public async Task CompleteAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var orderFilter = new OrderFilter(new long[] { orderId }, null, null, 0, 1);
        Order? order = null;
        await foreach (Order orderNeedToSearch in _orderRepository.SearchAsync(
                           orderFilter,
                           cancellationToken))
        {
            order = orderNeedToSearch;
            break;
        }

        if (order == null)
        {
            throw new ArgumentException("Заказ не найден");
        }

        if (order.OrderState != OrderState.Processing)
        {
            throw new ArgumentException("Заказ не в статусе processing");
        }

        await _orderRepository.UpdateStateAsync(
            orderId,
            OrderState.Completed,
            cancellationToken);
        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new StateChangedPayload
                {
                    OldState = order.OrderState,
                    NewState = OrderState.Completed,
                },
            },
            cancellationToken);
        transaction.Complete();
    }

    public async Task CancelAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var orderFilter = new OrderFilter(new long[] { orderId }, null, null, 0, 1);
        Order? order = null;
        await foreach (Order orderNeedToSearch in _orderRepository.SearchAsync(
                           orderFilter,
                           cancellationToken))
        {
            order = orderNeedToSearch;
            break;
        }

        if (order == null)
        {
            throw new ArgumentException("Заказ не найден");
        }

        if (order.OrderState != OrderState.Created)
        {
            throw new ArgumentException("Отменить можно только при созданри закaза");
        }

        await _orderRepository.UpdateStateAsync(
            orderId,
            OrderState.Cancelled,
            cancellationToken);
        await _orderHistoryRepository.AddAsync(
            new OrderHistoryItem
            {
                OrderId = orderId,
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = OrderHistoryItemKind.StateChanged,
                Payload = new StateChangedPayload
                {
                    OldState = order.OrderState,
                    NewState = OrderState.Cancelled,
                },
            },
            cancellationToken);
        transaction.Complete();
    }

    public IAsyncEnumerable<OrderHistoryItem> GetHistoryAsync(
        long orderId,
        long cursor,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var historyFilter = new OrderHistoryFilter(new long[] { orderId }, null, cursor, pageSize);
        return _orderHistoryRepository.SearchAsync(historyFilter, cancellationToken);
    }
}
using Core.Filters;
using Core.Models;

namespace Core.Repository;

public interface IOrderItemRepository
{
    IAsyncEnumerable<OrderItem> AddManyAsync(IReadOnlyList<OrderItem> orderItems, CancellationToken cancellationToken);

    Task DeleteAsync(long orderId, long productId, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> SearchAsync(OrderItemFilter filter, CancellationToken cancellationToken);
}
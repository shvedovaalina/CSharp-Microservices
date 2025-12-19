using Core.Filters;
using Core.Models;
using Core.Models.Enums;

namespace Core.Repository;

public interface IOrderRepository
{
    Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken);

    Task UpdateStateAsync(
        long id,
        OrderState newState,
        CancellationToken cancellationToken);

    IAsyncEnumerable<Order> SearchAsync(
        OrderFilter filter,
        CancellationToken cancellationToken);
}
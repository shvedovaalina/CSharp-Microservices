using Core.Filters;
using Core.Models;

namespace Core.Repository;

public interface IOrderHistoryRepository
{
    Task<OrderHistoryItem> AddAsync(OrderHistoryItem historyItem, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderHistoryItem> SearchAsync(OrderHistoryFilter filter, CancellationToken cancellationToken);
}
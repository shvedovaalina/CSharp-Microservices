using Core.Filters;
using Core.Models;

namespace Core.Repository;

public interface IProductRepository
{
    IAsyncEnumerable<Product> CreateManyAsync(IReadOnlyList<Product> products, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> SearchAsync(ProductFilter filter, CancellationToken cancellationToken);
}
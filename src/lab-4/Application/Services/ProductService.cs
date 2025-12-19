using Core.Models;
using Core.Repository;
using System.Transactions;

namespace Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product[]> CreateProductsTransactionAsync(IReadOnlyList<Product> products, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var createdProducts = new List<Product>();
        await foreach (Product product in _productRepository.CreateManyAsync(products, cancellationToken))
        {
            createdProducts.Add(product);
        }

        transaction.Complete();
        return createdProducts.ToArray();
    }
}
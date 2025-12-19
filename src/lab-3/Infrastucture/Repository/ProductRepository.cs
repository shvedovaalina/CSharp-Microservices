using Core.Filters;
using Core.Models;
using Core.Repository;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Infrastucture.Repository;

public class ProductRepository : IProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<Product> CreateManyAsync(IReadOnlyList<Product> products, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           insert into products (product_name, product_price)
                           select name, price from unnest(:names, :prices) as source(name, price)
                           returning product_id, product_name, product_price;
                           """;
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("names", products.Select(product => product.ProductName).ToArray()),
                new NpgsqlParameter("prices", products.Select(product => product.Price).ToArray()),
            },
        };
        var manyProducts = new List<Product>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Product
            {
                ProductId = reader.GetInt64(0),
                ProductName = reader.GetString(1),
                Price = reader.GetDecimal(2),
            };
        }
    }

    public async IAsyncEnumerable<Product> SearchAsync(ProductFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        const string sql = """
                           select product_id, product_name, product_price
                           from products
                           where
                           (product_id > :cursor)
                           and (cardinality(:ids) = 0 or product_id = any (:ids))
                           and (:name_pattern is null or product_name like :name_pattern)
                           and (:min_price is null or product_price > :min_price::money)
                           and (:max_price is null or product_price < :max_price::money)
                           order by product_id
                           limit :page_size;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("ids", filter.ProductIds),
                new NpgsqlParameter("name_pattern", filter.NameSubstring),
                new NpgsqlParameter("min_price", filter.MinPrice),
                new NpgsqlParameter("max_price", filter.MaxPrice),
                new NpgsqlParameter("cursor", filter.Cursor),
                new NpgsqlParameter("page_size", filter.PageSize),
            },
        };
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Product()
            {
                ProductId = reader.GetInt64(0),
                ProductName = reader.GetString(1),
                Price = reader.GetDecimal(2),
            };
        }
    }
}
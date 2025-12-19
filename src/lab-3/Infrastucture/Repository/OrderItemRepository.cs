using Core.Filters;
using Core.Models;
using Core.Repository;
using Npgsql;
using NpgsqlTypes;
using System.Runtime.CompilerServices;

namespace Infrastucture.Repository;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<OrderItem> AddManyAsync(
        IReadOnlyList<OrderItem> orderItems,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
                           select order_id, product_id, quantity, false
                           from unnest(:order_ids, :product_ids, :quantities) as source(order_id, product_id, quantity)
                           returning order_item_id, order_id, product_id, order_item_quantity, order_item_deleted;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_ids", orderItems.Select(orderItem => orderItem.OrderId).ToArray()),
                new NpgsqlParameter("product_ids", orderItems.Select(orderItem => orderItem.ProductId).ToArray()),
                new NpgsqlParameter("quantities", orderItems.Select(orderItem => orderItem.Quantity).ToArray()),
            },
        };
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderItem
            {
                OrderItemId = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                Quantity = reader.GetInt32(3),
                Deleted = reader.GetBoolean(4),
            };
        }
    }

    public async Task DeleteAsync(long orderId, long productId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           update order_items 
                           set order_item_deleted = true 
                           where order_id = :order_id and product_id = :product_id;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_id", orderId),
                new NpgsqlParameter("product_id", productId),
            },
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<OrderItem> SearchAsync(
        OrderItemFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                           from order_items
                           where
                           (order_item_id > :cursor)
                           and (cardinality(:order_ids) = 0 or order_item_id = any (:order_ids))
                           and (cardinality(:product_ids) = 0 or product_id = any (:product_ids))
                           and (:deleted is null or order_item_deleted = :deleted)
                           order by order_item_id
                           limit :page_size;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_ids", filter.OrderIds),
                new NpgsqlParameter("product_ids", filter.ProductIds),
                new NpgsqlParameter("deleted", NpgsqlDbType.Boolean)
                {
                    Value = filter.IsDeleted ?? (object)DBNull.Value,
                },
                new NpgsqlParameter("cursor", filter.Cursor),
                new NpgsqlParameter("page_size", filter.PageSize),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderItem()
            {
                OrderItemId = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                Quantity = reader.GetInt32(3),
                Deleted = reader.GetBoolean(4),
            };
        }
    }
}
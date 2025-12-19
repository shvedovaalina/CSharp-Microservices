using Core.Filters;
using Core.Models;
using Core.Models.Enums;
using Core.Repository;
using Npgsql;
using NpgsqlTypes;
using System.Runtime.CompilerServices;

namespace Infrastucture.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into orders (order_state, order_created_at, order_created_by)
                           values(:state, :created_at, :created_by) 
                           returning order_id, order_state, order_created_at, order_created_by;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("state", order.OrderState),
                new NpgsqlParameter("created_at", order.CreatedAt),
                new NpgsqlParameter("created_by", order.CreatedBy),
            },
        };
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Order
        {
            OrderId = reader.GetInt64(0),
            OrderState = reader.GetFieldValue<OrderState>(1),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(2),
            CreatedBy = reader.GetFieldValue<string>(3),
        };
    }

    public async Task UpdateStateAsync(
        long id,
        OrderState newState,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           update orders
                           set order_state = :newstate
                           where order_id = :order_id;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_id", id),
                new NpgsqlParameter("newstate", newState),
            },
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Order> SearchAsync(
        OrderFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           select order_id, order_state, order_created_at, order_created_by
                           from orders
                           where 
                               (order_id > :cursor)
                               and (cardinality(:ids) = 0 or order_id = any (:ids))
                               and (:state is null or order_state = :state::order_state)
                               and (:created_by is null or order_created_by = :created_by)
                           order by order_id
                           limit :page_size;
                           """;
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("cursor", filter.Cursor),
                new NpgsqlParameter("ids", filter.OrderIds),
                new NpgsqlParameter("state", NpgsqlDbType.Text)
                {
                    Value = filter.OrderState?.ToString() ?? (object)DBNull.Value,
                },
                new NpgsqlParameter("created_by", NpgsqlDbType.Text)
                {
                    Value = filter.CreatedBy ?? (object)DBNull.Value,
                },
                new NpgsqlParameter("page_size", filter.PageSize),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Order()
            {
                OrderId = reader.GetInt64(0),
                OrderState = reader.GetFieldValue<OrderState>(1),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(2),
                CreatedBy = reader.GetString(3),
            };
        }
    }
}
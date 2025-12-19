using Core.Filters;
using Core.Models;
using Core.Models.Enums;
using Core.Models.History;
using Core.Repository;
using Npgsql;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Infrastucture.Repository;

public class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<OrderHistoryItem> AddAsync(OrderHistoryItem historyItem, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        const string sql = """
                           insert into order_history(order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                           values (:order_id, :created_at, :item_kind, :payload::jsonb)
                           returning order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload;
                           """;

        string payloadSerialized = JsonSerializer.Serialize(historyItem.Payload);

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_id", historyItem.OrderId),
                new NpgsqlParameter("created_at", historyItem.CreatedAt),
                new NpgsqlParameter("item_kind", historyItem.Kind),
                new NpgsqlParameter("payload", payloadSerialized),
            },
        };
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        string serializedPayload = reader.GetString(4);
        OrderHistoryPayload? deserializedPayload = JsonSerializer.Deserialize<OrderHistoryPayload>(serializedPayload);
        return new OrderHistoryItem
        {
            Id = reader.GetInt64(0),
            OrderId = reader.GetInt64(1),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(2),
            Kind = reader.GetFieldValue<OrderHistoryItemKind>(3),
            Payload = deserializedPayload,
        };
    }

    public async IAsyncEnumerable<OrderHistoryItem> SearchAsync(
        OrderHistoryFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        const string sql = """
                           select order_history_item_id , order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                           from order_history
                           where
                           (order_history_item_id > :cursor)
                               and (cardinality(:order_ids) = 0 or order_id = any (:order_ids))
                               and (:kind is null or order_history_item_kind = :kind::order_history_item_kind)
                           order by order_history_item_id
                           limit :page_size;
                           """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("order_ids", filter.OrderIds),
                new NpgsqlParameter("kind", NpgsqlTypes.NpgsqlDbType.Text)
                {
                    Value = filter.Kind?.ToString() ?? (object)DBNull.Value,
                },
                new NpgsqlParameter("cursor", filter.Cursor),
                new NpgsqlParameter("page_size", filter.PageSize),
            },
        };
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            string serializedpayload = reader.GetString(4);
            OrderHistoryPayload? deserializedpayload =
                JsonSerializer.Deserialize<OrderHistoryPayload>(serializedpayload);
            yield return new OrderHistoryItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(2),
                Kind = reader.GetFieldValue<OrderHistoryItemKind>(3),
                Payload = deserializedpayload,
            };
        }
    }
}
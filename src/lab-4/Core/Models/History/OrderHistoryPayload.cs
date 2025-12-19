using System.Text.Json.Serialization;

namespace Core.Models.History;

[JsonDerivedType(typeof(OrderCreatedPayload), typeDiscriminator: nameof(OrderCreatedPayload))]
[JsonDerivedType(typeof(ItemAddedPayload), typeDiscriminator: nameof(ItemAddedPayload))]
[JsonDerivedType(typeof(ItemRemovedPayload), typeDiscriminator: nameof(ItemRemovedPayload))]
[JsonDerivedType(typeof(StateChangedPayload), typeDiscriminator: nameof(StateChangedPayload))]
[JsonDerivedType(typeof(OrderApprovalReceivedPayload), typeDiscriminator: nameof(OrderApprovalReceivedPayload))]
[JsonDerivedType(typeof(OrderDeliveryFinishedPayload), typeDiscriminator: nameof(OrderDeliveryFinishedPayload))]
[JsonDerivedType(typeof(OrderDeliveryStartedPayload), typeDiscriminator: nameof(OrderDeliveryStartedPayload))]
[JsonDerivedType(typeof(OrderPackingFinishedPayload), typeDiscriminator: nameof(OrderPackingFinishedPayload))]
[JsonDerivedType(typeof(OrderPackingStartedPayload), typeDiscriminator: nameof(OrderPackingStartedPayload))]
public abstract class OrderHistoryPayload
{
}
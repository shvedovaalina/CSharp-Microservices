using System.Text.Json.Serialization;

namespace Gateway.History;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(OrderCreatedPayload), typeDiscriminator: nameof(OrderCreatedPayload))]
[JsonDerivedType(typeof(ItemAddedPayload), typeDiscriminator: nameof(ItemAddedPayload))]
[JsonDerivedType(typeof(ItemRemovedPayload), typeDiscriminator: nameof(ItemRemovedPayload))]
[JsonDerivedType(typeof(StateChangedPayload), typeDiscriminator: nameof(StateChangedPayload))]
public abstract class OrderHistoryPayload
{
}
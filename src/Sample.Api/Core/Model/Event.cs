using System.Text.Json.Serialization;

namespace Sample.Api.Core.Model;

[JsonPolymorphic]
[JsonDerivedType(typeof(OrderSaved))]
public abstract class OutBoxEvent
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Name { get; init; } = string.Empty;
}


public sealed class OrderSaved : OutBoxEvent
{
    public Guid OrderId { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; }
    
    public OrderSaved(Order order, TimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(order);
        Id = order.Id;
        CreatedAt = provider.GetUtcNow();
        Name = "order.saved";
        OrderId = order.Id;
        Items = order.Items;
    }
}

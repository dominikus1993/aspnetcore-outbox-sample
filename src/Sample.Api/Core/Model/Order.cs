namespace Sample.Api.Core.Model;

public sealed class Order
{
    public Guid Id { get; set; }
    public IReadOnlyList<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public sealed class OrderItem
{
    public ProductId ItemId { get; set; }
    public int Quantity { get; set; }
}

namespace Sample.Api.Core.Model;

public enum OrderState
{
    New = 0,
    Cancelled = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4
}

public sealed class Order
{
    public OrderId Id { get; set; }
    public OrderNumber OrderNumber { get; set; }
    public OrderState State { get; set; }
    public IReadOnlyList<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    public Order()
    {
        
    }
    
    public Order(OrderId id, OrderNumber orderNumber, OrderState state, IReadOnlyList<OrderItem> items)
    {
        Id = id;
        OrderNumber = orderNumber;
        State = state;
        Items = items;
    }
    
    public static Order New(OrderId id, OrderNumber orderNumber, IReadOnlyList<OrderItem> items)
    {
        return new Order(id, orderNumber, OrderState.New, items);
    }
    
    public void SetAsNew()
    {
        State = OrderState.New;
    }
}

public sealed class OrderItem
{
    public ProductId ItemId { get; set; }
    public int Quantity { get; set; }
    
}

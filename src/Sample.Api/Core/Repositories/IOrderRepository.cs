using Sample.Api.Core.Model;

namespace Sample.Api.Core.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderById(OrderId orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByNumber(OrderNumber orderNumber, CancellationToken cancellationToken = default);
    Task<Result<Unit>> SaveOrder(Order order, CancellationToken cancellationToken = default);
}

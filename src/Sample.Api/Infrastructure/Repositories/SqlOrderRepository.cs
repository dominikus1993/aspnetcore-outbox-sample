using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories;

public sealed class SqlOrderRepository : IOrderRepository
{
    private readonly ProductsDbContext _productsDbContext;
    private readonly OutBoxEventCreator _outBoxEventCreator;
    private readonly TimeProvider _timeProvider;
    public SqlOrderRepository(ProductsDbContext productsDbContext, OutBoxEventCreator outBoxEventCreator, TimeProvider timeProvider)
    {
        _productsDbContext = productsDbContext;
        _outBoxEventCreator = outBoxEventCreator;
        _timeProvider = timeProvider;
    }
    
    public async Task<Order?> GetOrderById(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _productsDbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetOrderByNumber(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        return await _productsDbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Result<Unit>> SaveOrder(Order order, CancellationToken cancellationToken = default)
    {
        await using var tran = await _productsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _productsDbContext.Orders.AddAsync(order, cancellationToken);
            await _productsDbContext.OutBox.AddAsync(_outBoxEventCreator.Create(new OrderSaved(order, _timeProvider)), cancellationToken);
            await _productsDbContext.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return Result.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            await tran.RollbackAsync(cancellationToken);
            return Result.Failure<Unit>(e);
        }
    }
    
    public async Task<Result<Unit>> CancelOrder(Order order, CancellationToken cancellationToken = default)
    {
        await using var tran = await _productsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _productsDbContext.Orders.AddAsync(order, cancellationToken);
            await _productsDbContext.OutBox.AddAsync(_outBoxEventCreator.Create(new OrderSaved(order, _timeProvider)), cancellationToken);
            await _productsDbContext.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return Result.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            await tran.RollbackAsync(cancellationToken);
            return Result.Failure<Unit>(e);
        }
    }
}

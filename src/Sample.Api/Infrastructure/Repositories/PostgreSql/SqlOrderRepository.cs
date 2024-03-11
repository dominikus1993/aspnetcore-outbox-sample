using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.PostgreSql;

public sealed class SqlOrderRepository : IOrderRepository
{
    private readonly IDbContextFactory<ProductsDbContext> _productsDbContext;
    private readonly OutBoxEventCreator _outBoxEventCreator;
    private readonly TimeProvider _timeProvider;
    public SqlOrderRepository( IDbContextFactory<ProductsDbContext> productsDbContext, OutBoxEventCreator outBoxEventCreator, TimeProvider timeProvider)
    {
        _productsDbContext = productsDbContext;
        _outBoxEventCreator = outBoxEventCreator;
        _timeProvider = timeProvider;
    }
    
    public async Task<Order?> GetOrderById(OrderId orderId, CancellationToken cancellationToken = default)
    {
        await using var context = await _productsDbContext.CreateDbContextAsync(cancellationToken);
        return await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetOrderByNumber(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        await using var context = await _productsDbContext.CreateDbContextAsync(cancellationToken);
        return await context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Result<Unit>> SaveOrder(Order order, CancellationToken cancellationToken = default)
    {
        await using var context = await _productsDbContext.CreateDbContextAsync(cancellationToken);
        await using var tran = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            context.Orders.Add(order);
            context.OutBox.Add(_outBoxEventCreator.Create(new OrderSaved(order, _timeProvider)));
            await context.SaveChangesAsync(cancellationToken);
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
        await using var context = await _productsDbContext.CreateDbContextAsync(cancellationToken);
        await using var tran = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.Orders.AddAsync(order, cancellationToken);
            await context.OutBox.AddAsync(_outBoxEventCreator.Create(new OrderSaved(order, _timeProvider)), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
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

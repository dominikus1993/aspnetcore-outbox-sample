using AutoFixture.Xunit2;
using Microsoft.Extensions.Time.Testing;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
using Sample.Api.Infrastructure.Repositories.PostgreSql;
using Sample.Api.Tests.Fixtures;

namespace Sample.Api.Tests.Infrastructure.Repositories;

public class SqlOutBoxRepositoryTests: IClassFixture<PostgresFixture>, IDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsDbContext _productsDbContext;
    private readonly IOutBoxRepository _outBoxRepository;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    public SqlOutBoxRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _orderRepository = new SqlOrderRepository(_productsDbContext, new OutBoxEventCreator(new FakeTimeProvider(_now)),
            new FakeTimeProvider(_now));
        _outBoxRepository = new SqlOutBoxRepository(_productsDbContext, new FakeTimeProvider(_now));
    }

    [Theory]
    [AutoData]
    public async Task TestSaveNewOrder(Guid orderId, long orderNumber, OrderItem[] items)
    {
        var order = Order.New(orderId, orderNumber, items);
        var res = await _orderRepository.SaveOrder(order);
        
        Assert.True(res.IsSuccess);

        var events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
        
        var result =
            await _outBoxRepository.ProcessOldestNotProcessedEvent((_, _) =>
                Task.FromResult<Result<Unit>>(Result.UnitResult));
        
        Assert.True(result.IsSuccess);

        events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.Null(events);
    }
    
    [Theory]
    [AutoData]
    public async Task TestSaveNewOrderWhenProcessIsFailed(Guid orderId, long orderNumber, OrderItem[] items)
    {
        var order = Order.New(orderId, orderNumber, items);
        var res = await _orderRepository.SaveOrder(order);
        
        Assert.True(res.IsSuccess);

        var events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
        var error = new InvalidOperationException("Error");
        var result =
            await _outBoxRepository.ProcessOldestNotProcessedEvent((_, _) =>
                Task.FromResult<Result<Unit>>(Result.Failure<Unit>(error)));
        
        Assert.False(result.IsSuccess);

        events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
    }
    
    public void Dispose()
    {
        _productsDbContext.CleanAllTables();
        _productsDbContext.Dispose();
    }
}

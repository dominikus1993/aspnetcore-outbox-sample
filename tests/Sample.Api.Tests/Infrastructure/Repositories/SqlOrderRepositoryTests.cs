using AutoFixture.Xunit2;
using Microsoft.Extensions.Time.Testing;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
using Sample.Api.Tests.Fixtures;

namespace Sample.Api.Tests.Infrastructure.Repositories;

public sealed class SqlOrderRepositoryTests : IClassFixture<PostgresFixture>, IDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsDbContext _productsDbContext;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    public SqlOrderRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _orderRepository = new SqlOrderRepository(_productsDbContext, new OutBoxEventCreator(new FakeTimeProvider(_now)),
            new FakeTimeProvider(_now));
    }

    [Theory]
    [AutoData]
    public async Task TestCorrectOrder(Order order)
    {
        var res = await _orderRepository.SaveOrder(order);
        
        Assert.True(res.IsSuccess);
        
        var orderFromDb = await _orderRepository.GetOrderById(order.Id);

        Assert.NotNull(orderFromDb);
    }
    
    public void Dispose()
    {
        _productsDbContext.CleanAllTables();
        _productsDbContext.Dispose();
    }
}

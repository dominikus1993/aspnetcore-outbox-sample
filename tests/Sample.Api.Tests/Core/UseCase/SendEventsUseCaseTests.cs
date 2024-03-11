using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Sample.Api.Core.Events;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Core.UseCases;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
using Sample.Api.Infrastructure.Repositories.PostgreSql;
using Sample.Api.Tests.Fixtures;

namespace Sample.Api.Tests.Core.UseCase;

public sealed class FakeMessagePublisher : IEventPublisher
{
    private readonly List<OutBox> _outBoxes = [];
    
    public IReadOnlyList<OutBox> OutBoxes => _outBoxes;
    
    public Task<Result<Unit>> Publish(OutBox evt, CancellationToken cancellationToken = default)
    {
        _outBoxes.Add(evt);
        return Task.FromResult(Result.UnitResult);
    }
}

public sealed class ErrorMessagePublisher : IEventPublisher
{
    public Task<Result<Unit>> Publish(OutBox evt, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<Unit>(new InvalidOperationException("test")));
    }
}

public sealed class SendEventsUseCaseTests : IClassFixture<PostgresFixture>, IDisposable
{
    private readonly PostgresFixture _fixture;
    private readonly IOutBoxRepository _outBoxRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsDbContext _productsDbContext;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public SendEventsUseCaseTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _productsDbContext = _fixture.DbContextFactory.CreateDbContext();
        _orderRepository = new SqlOrderRepository(_fixture.DbContextFactory, new OutBoxEventCreator(new FakeTimeProvider(_now)),
            new FakeTimeProvider(_now));
        _outBoxRepository = new SqlOutBoxRepository(_productsDbContext, new FakeTimeProvider(_now));
    }
    
    [Theory]
    [AutoData]
    public async Task TestSaveNewOrder(Guid orderId, long orderNumber, OrderItem[] items)
    {
        var publisher = new FakeMessagePublisher();
        var useCase = new SendEventsUseCase(_outBoxRepository, publisher, NullLogger<SendEventsUseCase>.Instance);
        var order = Order.New(orderId, orderNumber, items);
        var res = await _orderRepository.SaveOrder(order);
        
        Assert.True(res.IsSuccess);

        var events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
        
        await useCase.Run();

        Assert.Single(publisher.OutBoxes);
        
        events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.Null(events);
    }
    
    [Theory]
    [AutoData]
    public async Task TestSaveNewOrders(Guid orderId, Guid orderId2, long orderNumber, long orderNumber2, OrderItem[] items)
    {
        var publisher = new FakeMessagePublisher();
        var useCase = new SendEventsUseCase(_outBoxRepository, publisher, NullLogger<SendEventsUseCase>.Instance);
        var order = Order.New(orderId, orderNumber, items);
        var order2 = Order.New(orderId2, orderNumber2, items);
        var res = await _orderRepository.SaveOrder(order);
        var res2 = await _orderRepository.SaveOrder(order2);
        
        Assert.True(res.IsSuccess);
        Assert.True(res2.IsSuccess);

        var events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
        
        await useCase.Run();

        Assert.NotEmpty(publisher.OutBoxes);
        
        events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.Null(events);
    }
    
    [Theory]
    [AutoData]
    public async Task TestSaveNewOrderWhenProcessIsFailed(Guid orderId, long orderNumber, OrderItem[] items)
    {
        var useCase = new SendEventsUseCase(_outBoxRepository, new ErrorMessagePublisher(), NullLogger<SendEventsUseCase>.Instance);
        var order = Order.New(orderId, orderNumber, items);
        var res = await _orderRepository.SaveOrder(order);
        
        Assert.True(res.IsSuccess);

        var events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await useCase.Run());
        
        events = await _outBoxRepository.GetOldestNotProcessedEvents();
        
        Assert.NotNull(events);
    }
    
    public void Dispose()
    {
        _productsDbContext.CleanAllTables();
        _productsDbContext.Dispose();
    }
}

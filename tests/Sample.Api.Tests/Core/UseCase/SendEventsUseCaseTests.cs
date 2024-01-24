using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Sample.Api.Core.Events;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Core.UseCases;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
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

public class SendEventsUseCaseTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;
    private readonly SendEventsUseCase _sut;
    private readonly IOutBoxRepository _outBoxRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsDbContext _productsDbContext;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    private readonly IEventPublisher _messagePublisher;

    public SendEventsUseCaseTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _productsDbContext = _fixture.DbContextFactory.CreateDbContext();
        _orderRepository = new SqlOrderRepository(_productsDbContext, new OutBoxEventCreator(new FakeTimeProvider(_now)),
            new FakeTimeProvider(_now));
        _outBoxRepository = new SqlOutBoxRepository(_productsDbContext, new FakeTimeProvider(_now));
        _messagePublisher = new FakeMessagePublisher();
        _sut = new SendEventsUseCase(_outBoxRepository, _messagePublisher, NullLogger<SendEventsUseCase>.Instance);
    }
}

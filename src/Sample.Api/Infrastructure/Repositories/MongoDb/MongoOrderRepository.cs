using MongoDB.Driver;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;

namespace Sample.Api.Infrastructure.Repositories.MongoDb;

public sealed class MongoOrderRepository : IOrderRepository
{
    private IMongoDatabase _mongoDatabase;
    private IMongoClient _mongoClient;
    private readonly OutBoxEventCreator _outBoxEventCreator;
    private readonly TimeProvider _timeProvider;
    public MongoOrderRepository(IMongoDatabase mongoDatabase, IMongoClient mongoClient, OutBoxEventCreator outBoxEventCreator, TimeProvider timeProvider)
    {
        _mongoDatabase = mongoDatabase;
        _mongoClient = mongoClient;
        _outBoxEventCreator = outBoxEventCreator;
        _timeProvider = timeProvider;
    }

    public async Task<Order?> GetOrderById(OrderId orderId, CancellationToken cancellationToken = default)
    {
        var filters = Builders<Order>.Filter.Eq(x => x.Id, orderId);
        var result = await _mongoDatabase.Orders().Find(filters).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return result;
    }

    public async Task<Order?> GetOrderByNumber(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        var filters = Builders<Order>.Filter.Eq(x => x.OrderNumber, orderNumber);
        var result = await _mongoDatabase.Orders().Find(filters).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return result;
    }

    public async Task<Result<Unit>> SaveOrder(Order order, CancellationToken cancellationToken = default)
    {
        using var session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        try
        {
            await _mongoDatabase.Orders().InsertOneAsync(session, order, cancellationToken: cancellationToken);
            await _mongoDatabase.OutBox().InsertOneAsync(session, _outBoxEventCreator.Create(new OrderSaved(order, _timeProvider)), cancellationToken: cancellationToken);
            await session.CommitTransactionAsync(cancellationToken);
            return Result.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync(cancellationToken);
            return Result.Failure<Unit>(e);
        }
    }
}

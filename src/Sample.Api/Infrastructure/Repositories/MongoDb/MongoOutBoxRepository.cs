using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.MongoDb;

public sealed class MongoOutBoxRepository : IOutBoxRepository
{
    private IMongoDatabase _mongoDatabase;
    private IMongoClient _mongoClient;
    private readonly TimeProvider _timeProvider;

    public MongoOutBoxRepository(IMongoDatabase mongoDatabase, IMongoClient mongoClient, TimeProvider timeProvider)
    {
        _mongoClient = mongoClient;
        _mongoDatabase = mongoDatabase;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Unit>> ProcessOldestNotProcessedEvent(
        Func<OutBox, CancellationToken, Task<Result<Unit>>> f, CancellationToken cancellationToken = default)
    {
        using var session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        try
        {
            var record = await _mongoDatabase.OutBox().Find(session, o => !o.ProcessedAtTimestamp.HasValue)
                .SortBy(o => o.CreatedAtTimestamp)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (record is null)
            {
                return Result.UnitResult;
            }

            var result = await f(record, cancellationToken);
            if (!result.IsSuccess)
            {
                await session.AbortTransactionAsync(cancellationToken);
                return result;
            }

            record.MarkAsProcessed(_timeProvider);
            var res = await _mongoDatabase.OutBox().ReplaceOneAsync(session, o => o.Id == record.Id, record,
                cancellationToken: cancellationToken);
            if (res.IsAcknowledged)
            {
                await session.CommitTransactionAsync(cancellationToken);
                return Result.UnitResult;
            }

            await session.AbortTransactionAsync(cancellationToken);
            return Result.Failure<Unit>(new InvalidOperationException("Failed to mark event as processed"));
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync(cancellationToken);
            return Result.Failure<Unit>(e);
        }
    }

    public async Task<OutBox?> GetOldestNotProcessedEvents(CancellationToken cancellationToken = default)
    {
        return await _mongoDatabase.OutBox().AsQueryable().OrderBy(o => o.CreatedAtTimestamp)
            .FirstOrDefaultAsync(o => !o.ProcessedAtTimestamp.HasValue, cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> MarkAsProcessed(OutBox outBox, CancellationToken cancellationToken = default)
    {
        try
        {
            outBox.MarkAsProcessed(_timeProvider);
            return Result.UnitResult;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(e);
        }
    }
}

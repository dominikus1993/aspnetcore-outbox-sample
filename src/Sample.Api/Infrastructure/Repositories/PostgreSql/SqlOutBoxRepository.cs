using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.PostgreSql;

public sealed class SqlOutBoxRepository : IOutBoxRepository
{
    private readonly ProductsDbContext _productsDbContext;
    private readonly TimeProvider _timeProvider;

    public SqlOutBoxRepository(ProductsDbContext productsDbContext, TimeProvider timeProvider)
    {
        _productsDbContext = productsDbContext;
        _timeProvider = timeProvider;
    }

    public async Task<OutBox?> GetOldestNotProcessedEvents(CancellationToken cancellationToken = default)
    {
        return await _productsDbContext.OutBox.OrderBy(o => o.CreatedAtTimestamp)
            .FirstOrDefaultAsync(o => !o.ProcessedAtTimestamp.HasValue, cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> MarkAsProcessed(OutBox outBox, CancellationToken cancellationToken = default)
    {
        try
        {
            outBox.MarkAsProcessed(_timeProvider);
            _productsDbContext.OutBox.Update(outBox);
            await _productsDbContext.SaveChangesAsync(cancellationToken);
            return Result.UnitResult;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(e);
        }
    }

    public async Task<Result<Unit>> ProcessOldestNotProcessedEvent(Func<OutBox, CancellationToken, Task<Result<Unit>>> f, CancellationToken cancellationToken = default)
    {
        await using var session = await _productsDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var record = await _productsDbContext.OutBox.OrderBy(o => o.CreatedAtTimestamp)
                .FirstOrDefaultAsync(o => !o.ProcessedAtTimestamp.HasValue, cancellationToken: cancellationToken);
            if (record is null)
            {
                return Result.UnitResult;
            }

            var result = await f(record, cancellationToken);
            if (!result.IsSuccess)
            {
                await session.RollbackAsync(cancellationToken);
                return result;
            }
            record.MarkAsProcessed(_timeProvider);
            
            _productsDbContext.OutBox.Update(record);
            await _productsDbContext.SaveChangesAsync(cancellationToken);
            await session.CommitAsync(cancellationToken);
            return Result.UnitResult;
        }
        catch (Exception e)
        {
            await session.RollbackAsync(cancellationToken);
            return Result.Failure<Unit>(e);
        }
    }
}

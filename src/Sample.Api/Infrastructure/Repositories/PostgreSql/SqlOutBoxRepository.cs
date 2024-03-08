using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.PostgreSql;

public class SqlOutBoxRepository : IOutBoxRepository
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
}

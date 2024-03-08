using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Core.Repositories;

public interface IOutBoxRepository
{
    Task<OutBox?> GetOldestNotProcessedEvents(CancellationToken cancellationToken = default);
    Task<Result<Unit>> MarkAsProcessed(OutBox outBox, CancellationToken cancellationToken = default);
    Task<Result<Unit>> ProcessOldestNotProcessedEvent(
        Func<OutBox, CancellationToken, Task<Result<Unit>>> f, CancellationToken cancellationToken = default);
}

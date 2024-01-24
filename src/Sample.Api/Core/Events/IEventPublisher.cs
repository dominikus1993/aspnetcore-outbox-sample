using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Core.Events;

public interface IEventPublisher
{
    Task<Result<Unit>> Publish(OutBox evt, CancellationToken cancellationToken = default);
}

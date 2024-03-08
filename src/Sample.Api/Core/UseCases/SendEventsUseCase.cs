using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.Services;

namespace Sample.Api.Core.UseCases;

public sealed class SendEventsUseCase
{
    private readonly IOutBoxRepository _outBoxRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SendEventsUseCase> _logger;
    
    public SendEventsUseCase(IOutBoxRepository outBoxRepository, IEventPublisher eventPublisher, ILogger<SendEventsUseCase> logger)
    {
        _outBoxRepository = outBoxRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var res = await _outBoxRepository.ProcessOldestNotProcessedEvent(async (outbox, ct) => await _eventPublisher.Publish(outbox, ct), cancellationToken);
        if (!res.IsSuccess)
        {
            var err = res.ErrorValue;
            _logger.LogError(err, "Failed to process oldest not processed event");
            throw new InvalidOperationException("Failed to process oldest not processed event", err);
        }
    }
}

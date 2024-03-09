using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;
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
        try
        {
            OutBox? outBox = await _outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
            while (outBox is not null)
            {
                var result = await _eventPublisher.Publish(outBox, cancellationToken);
                if (result.IsSuccess)
                {
                    await _outBoxRepository.MarkAsProcessed(outBox, cancellationToken);
                    outBox = await _outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException("Failed to process event", result.ErrorValue);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process event");
            throw;
        }
    }
}

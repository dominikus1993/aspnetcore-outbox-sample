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
    public async Task Run(CancellationToken cancellationToken)
    {

        var outBox = await _outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
        while (outBox is not null)
        {
            var result = await _eventPublisher.Publish(outBox, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError(result.ErrorValue, "Error while publishing event {@Event}", outBox);
                continue;
            }

            await _outBoxRepository.MarkAsProcessed(outBox, cancellationToken);

            _logger.LogInformation("Event {@Event} published", outBox);
            
            outBox = await _outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
        }
    }
}

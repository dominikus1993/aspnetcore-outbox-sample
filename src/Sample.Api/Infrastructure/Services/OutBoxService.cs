using Coravel.Invocable;
using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;
using Sample.Api.Core.UseCases;

namespace Sample.Api.Infrastructure.Services;

public sealed class OutBoxService : IInvocable
{
    private readonly ILogger<OutBoxService> _logger;
    private readonly SendEventsUseCase _useCase;

    public OutBoxService(ILogger<OutBoxService> logger, SendEventsUseCase useCase)
    {
        _logger = logger;
        _useCase = useCase;
    }

    public async Task Invoke()
    {
        try
        {
            await _useCase.Run();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing SendEventsUseCase");
        }
    }
}

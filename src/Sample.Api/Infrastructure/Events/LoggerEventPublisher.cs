using System.Text.Json;
using Sample.Api.Core.Events;
using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Serialization;

namespace Sample.Api.Infrastructure.Events;

public sealed class LoggerEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggerEventPublisher> _logger;

    public LoggerEventPublisher(ILogger<LoggerEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task<Result<Unit>> Publish(OutBox evt, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Deserialize<OutBoxEvent>(evt.Data, OutBoxSerializationConfig.Default.OutBoxEvent);
        switch (json)
        {
            case OrderSaved saved:
                _logger.LogInformation("Order saved: {OrderId}", saved.OrderId);
                return Task.FromResult(Result.UnitResult);
        }
        
        return Task.FromResult<Result<Unit>>(Result.Failure<Unit>(new InvalidOperationException("Unknown event type")));
    }
}

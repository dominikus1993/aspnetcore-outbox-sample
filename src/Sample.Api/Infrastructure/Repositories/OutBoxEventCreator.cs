using System.Text.Json;
using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Serialization;

namespace Sample.Api.Infrastructure.Repositories;

public sealed class OutBoxEventCreator
{
    private readonly TimeProvider _timeProvider;
    
    public OutBoxEventCreator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public OutBox Create(OutBoxEvent boxEvent)
    {
        ArgumentNullException.ThrowIfNull(boxEvent);
        return new OutBox()
        {
            Type = boxEvent.GetType().FullName ?? string.Empty,
            Name = boxEvent.Name,
            Data = JsonSerializer.Serialize(boxEvent, OutBoxSerializationConfig.Default.OutBoxEvent),
            CreatedAtTimestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds(),
            Id = boxEvent.Id
        };
    }
}

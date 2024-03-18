using Microsoft.EntityFrameworkCore.Diagnostics;
using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.Repositories;

namespace Sample.Api.Infrastructure.EfCore;

public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly OutBoxEventCreator _creator;
    private readonly TimeProvider _timeProvider;

    public OutboxInterceptor(OutBoxEventCreator creator, TimeProvider timeProvider)
    {
        _creator = creator;
        _timeProvider = timeProvider;
    }


    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        var order = eventData.Context.ChangeTracker.Entries<Order>().Select(e => e.Entity).ToArray();
        if (order is null or {Length: 0 })
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    
        var outBoxs = Create(order);
        
        eventData.Context.AddRange(outBoxs);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private IEnumerable<OutBox> Create(IEnumerable<Order> orders)
    {
        foreach (var order in orders)
        {
            yield return _creator.Create(new OrderSaved(order, _timeProvider));
        }
    }
}

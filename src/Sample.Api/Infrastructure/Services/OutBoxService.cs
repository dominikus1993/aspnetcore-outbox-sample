using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;

namespace Sample.Api.Infrastructure.Services;

public sealed class OutBoxService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutBoxService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            await Run(stoppingToken);
        }
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var outBoxRepository = scope.ServiceProvider.GetRequiredService<IOutBoxRepository>();

        var outBox = await outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
        while (outBox is not null)
        {
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OutBoxService>>();

            var result = await publisher.Publish(outBox, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError(result.ErrorValue, "Error while publishing event {@Event}", outBox);
            }

            await outBoxRepository.MarkAsProcessed(outBox, cancellationToken);

            logger.LogInformation("Event {@Event} published", outBox);
            
            outBox = await outBoxRepository.GetOldestNotProcessedEvents(cancellationToken);
        }
    }
}

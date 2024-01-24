using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;
using Sample.Api.Core.UseCases;

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
            await using var scope = _serviceProvider.CreateAsyncScope();
            try
            {
                var usecase = scope.ServiceProvider.GetRequiredService<SendEventsUseCase>();
                await usecase.Run(stoppingToken);
            }
            catch (Exception e)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<OutBoxService>>();
                logger.LogError(e, "Error while executing SendEventsUseCase");
            }
        }
    }
}

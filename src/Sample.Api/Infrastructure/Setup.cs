using Sample.Api.Core.Events;
using Sample.Api.Core.Repositories;
using Sample.Api.Core.UseCases;
using Sample.Api.Infrastructure.Events;
using Sample.Api.Infrastructure.Repositories.PostgreSql;
using Sample.Api.Infrastructure.Services;

namespace Sample.Api.Infrastructure;

public static class Setup
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<OutBoxService>();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddTransient<SendEventsUseCase>();
        builder.Services.AddTransient<IOrderRepository, SqlOrderRepository>();
        builder.Services.AddTransient<IOutBoxRepository, SqlOutBoxRepository>();
        builder.Services.AddTransient<IEventPublisher, LoggerEventPublisher>();
        return builder;
    }
}

using Agenda.Application.Abstractions.Messaging;
using Agenda.Application.Abstractions.Persistence;
using Agenda.Infrastructure.Messaging;
using Agenda.Infrastructure.Persistence;
using Agenda.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agenda.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AgendaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IContactRepository, ContactRepository>();

        AddMessaging(services, configuration);

        return services;
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection("RabbitMq");
        if (rabbitSection.Exists())
        {
            services.Configure<RabbitMqOptions>(opts =>
            {
                opts.Host = rabbitSection["Host"] ?? "localhost";
                opts.Port = int.TryParse(rabbitSection["Port"], out var p) ? p : 5672;
                opts.Username = rabbitSection["Username"] ?? "guest";
                opts.Password = rabbitSection["Password"] ?? "guest";
            });
            services.AddSingleton<RabbitMqConnection>();
            services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        }
        else
        {
            services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
        }
    }
}

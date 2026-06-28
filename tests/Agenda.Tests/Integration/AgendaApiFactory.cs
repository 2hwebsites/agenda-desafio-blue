using Agenda.Application.Abstractions.Messaging;
using Agenda.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Agenda.Tests.Integration;

public sealed class AgendaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUsername = "testadmin";
    public const string TestPassword = "testadmin_pass_for_integration_tests";

    private const string TestJwtKey = "test-jwt-secret-key-for-integration-tests-min32chars!!";
    private const string TestJwtIssuer = "agenda-tests";
    private const string TestJwtAudience = "agenda-tests";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Issuer", TestJwtIssuer);
        builder.UseSetting("Jwt:Audience", TestJwtAudience);
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:ExpiresMinutes", "60");
        builder.UseSetting("AuthSeed:Username", TestUsername);
        builder.UseSetting("AuthSeed:Password", TestPassword);

        builder.ConfigureTestServices(services =>
        {
            var existing = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AgendaDbContext>))
                .ToList();
            foreach (var d in existing)
                services.Remove(d);

            services.AddDbContext<AgendaDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Replace RabbitMQ publisher with no-op so tests run without a broker
            var publisherDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IIntegrationEventPublisher));
            if (publisherDescriptor is not null)
                services.Remove(publisherDescriptor);
            services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await db.Contacts.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

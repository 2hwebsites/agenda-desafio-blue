using Agenda.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

#pragma warning disable CS0618
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();
#pragma warning restore CS0618

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Issuer", TestJwtIssuer);
        builder.UseSetting("Jwt:Audience", TestJwtAudience);
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:ExpiresMinutes", "60");
        builder.UseSetting("AuthSeed:Username", TestUsername);
        builder.UseSetting("AuthSeed:Password", TestPassword);

        builder.ConfigureServices(services =>
        {
            var existing = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AgendaDbContext>))
                .ToList();
            foreach (var d in existing)
                services.Remove(d);

            services.AddDbContext<AgendaDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
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

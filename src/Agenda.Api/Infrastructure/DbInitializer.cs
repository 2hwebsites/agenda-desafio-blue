using Agenda.Domain.Entities;
using Agenda.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Api.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Contacts.AnyAsync())
        {
            db.Contacts.AddRange(
                Contact.Create("Ana Silva", "ana.silva@example.com", "(11) 91234-5678"),
                Contact.Create("Bruno Costa", "bruno.costa@example.com", "(21) 98765-4321"),
                Contact.Create("Carla Mendes", "carla.mendes@example.com"));
            await db.SaveChangesAsync();
        }
    }
}

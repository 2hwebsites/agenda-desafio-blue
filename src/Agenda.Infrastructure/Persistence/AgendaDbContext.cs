using Agenda.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Infrastructure.Persistence;

public sealed class AgendaDbContext : DbContext
{
    public AgendaDbContext(DbContextOptions<AgendaDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

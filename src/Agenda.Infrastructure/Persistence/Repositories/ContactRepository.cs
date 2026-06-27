using Agenda.Application.Abstractions.Persistence;
using Agenda.Domain.Entities;
using Agenda.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Agenda.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(AgendaDbContext db) : IContactRepository
{
    public Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => db.Contacts.AnyAsync(
            c => c.Email == email.Trim().ToLowerInvariant(),
            cancellationToken);

    public Task<bool> ExistsByEmailExcludingIdAsync(string email, Guid excludedId, CancellationToken cancellationToken = default)
        => db.Contacts.AnyAsync(
            c => c.Email == email.Trim().ToLowerInvariant() && c.Id != excludedId,
            cancellationToken);

    public async Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = db.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        db.Contacts.Add(contact);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505", ConstraintName: "ix_contacts_email" })
        {
            throw new DuplicateEmailException();
        }
    }
}

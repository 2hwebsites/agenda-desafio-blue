namespace Agenda.Application.Contacts;

public sealed record ContactDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    DateTime CreatedAt);

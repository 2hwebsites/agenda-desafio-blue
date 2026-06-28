namespace Agenda.Contracts;

public sealed record ContactCreatedIntegrationEvent(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt);

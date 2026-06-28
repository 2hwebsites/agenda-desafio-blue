using MediatR;

namespace Agenda.Application.Contacts.Events;

public sealed record ContactCreatedDomainEvent(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt) : INotification;

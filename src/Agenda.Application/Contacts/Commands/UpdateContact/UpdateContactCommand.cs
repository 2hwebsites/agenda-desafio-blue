using MediatR;

namespace Agenda.Application.Contacts.Commands.UpdateContact;

public sealed record UpdateContactCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone) : IRequest<ContactDto>;

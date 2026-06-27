using MediatR;

namespace Agenda.Application.Contacts.Commands.CreateContact;

public sealed record CreateContactCommand(
    string Name,
    string Email,
    string? Phone) : IRequest<ContactDto>;

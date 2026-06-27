using MediatR;

namespace Agenda.Application.Contacts.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid Id) : IRequest<Unit>;

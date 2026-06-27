using MediatR;

namespace Agenda.Application.Contacts.Queries.GetContactById;

public sealed record GetContactByIdQuery(Guid Id) : IRequest<ContactDto>;

using Agenda.Application.Common.Models;
using MediatR;

namespace Agenda.Application.Contacts.Queries.GetContacts;

public sealed record GetContactsQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ContactDto>>;

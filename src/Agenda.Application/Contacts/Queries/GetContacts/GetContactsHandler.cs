using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Agenda.Application.Contacts.Queries.GetContacts;

public sealed class GetContactsHandler(IContactRepository repository, IMapper mapper)
    : IRequestHandler<GetContactsQuery, PagedResult<ContactDto>>
{
    public async Task<PagedResult<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, total) = await repository.GetPagedAsync(request.Search, page, pageSize, cancellationToken);
        var dtos = mapper.Map<IReadOnlyList<ContactDto>>(items);

        return new PagedResult<ContactDto>(dtos, page, pageSize, total);
    }
}

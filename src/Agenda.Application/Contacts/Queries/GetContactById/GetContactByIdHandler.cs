using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Domain.Entities;
using AutoMapper;
using MediatR;

namespace Agenda.Application.Contacts.Queries.GetContactById;

public sealed class GetContactByIdHandler(IContactRepository repository, IMapper mapper)
    : IRequestHandler<GetContactByIdQuery, ContactDto>
{
    public async Task<ContactDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), request.Id);

        return mapper.Map<ContactDto>(contact);
    }
}

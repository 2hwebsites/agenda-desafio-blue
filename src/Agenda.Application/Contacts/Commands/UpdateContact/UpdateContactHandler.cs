using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Domain.Entities;
using Agenda.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace Agenda.Application.Contacts.Commands.UpdateContact;

public sealed class UpdateContactHandler(IContactRepository repository, IMapper mapper)
    : IRequestHandler<UpdateContactCommand, ContactDto>
{
    public async Task<ContactDto> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), request.Id);

        var emailChanged = !string.Equals(contact.Email, request.Email.Trim().ToLowerInvariant(),
            StringComparison.Ordinal);

        if (emailChanged && await repository.ExistsByEmailExcludingIdAsync(request.Email, request.Id, cancellationToken))
            throw new DuplicateEmailException(request.Email);

        contact.Update(request.Name, request.Email, request.Phone);
        await repository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ContactDto>(contact);
    }
}

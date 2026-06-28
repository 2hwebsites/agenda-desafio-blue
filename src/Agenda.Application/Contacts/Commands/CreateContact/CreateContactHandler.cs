using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Contacts.Events;
using Agenda.Domain.Entities;
using Agenda.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace Agenda.Application.Contacts.Commands.CreateContact;

public sealed class CreateContactHandler(
    IContactRepository repository,
    IMapper mapper,
    IPublisher publisher) : IRequestHandler<CreateContactCommand, ContactDto>
{
    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        if (await repository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new DuplicateEmailException(request.Email);

        var contact = Contact.Create(request.Name, request.Email, request.Phone);
        await repository.AddAsync(contact, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await publisher.Publish(
            new ContactCreatedDomainEvent(contact.Id, contact.Name, contact.Email, contact.CreatedAt),
            cancellationToken);

        return mapper.Map<ContactDto>(contact);
    }
}

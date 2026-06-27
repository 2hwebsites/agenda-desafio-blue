using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Domain.Entities;
using MediatR;

namespace Agenda.Application.Contacts.Commands.DeleteContact;

public sealed class DeleteContactHandler(IContactRepository repository)
    : IRequestHandler<DeleteContactCommand, Unit>
{
    public async Task<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), request.Id);

        contact.MarkAsDeleted();
        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Application.Contacts.Commands.DeleteContact;
using Agenda.Domain.Entities;
using NSubstitute;
using Shouldly;

namespace Agenda.Tests.Unit.Handlers;

public class DeleteContactHandlerTests
{
    private readonly IContactRepository _repository = Substitute.For<IContactRepository>();
    private readonly DeleteContactHandler _handler;

    public DeleteContactHandlerTests()
    {
        _handler = new DeleteContactHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingContact_MarksAsDeletedAndSaves()
    {
        var contact = Contact.Create("Test", "test@example.com");
        var command = new DeleteContactCommand(contact.Id);
        _repository.GetByIdAsync(contact.Id, default).Returns(contact);

        await _handler.Handle(command, default);

        contact.IsDeleted.ShouldBeTrue();
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ContactNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var command = new DeleteContactCommand(id);
        _repository.GetByIdAsync(id, default).Returns((Contact?)null);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, default));
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }
}

using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Application.Contacts;
using Agenda.Application.Contacts.Commands.UpdateContact;
using Agenda.Domain.Entities;
using Agenda.Domain.Exceptions;
using AutoMapper;
using NSubstitute;
using Shouldly;

namespace Agenda.Tests.Unit.Handlers;

public class UpdateContactHandlerTests
{
    private readonly IContactRepository _repository = Substitute.For<IContactRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly UpdateContactHandler _handler;

    public UpdateContactHandlerTests()
    {
        _handler = new UpdateContactHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingContact_SameEmail_UpdatesAndReturnsDto()
    {
        var contact = Contact.Create("Old Name", "test@example.com");
        var command = new UpdateContactCommand(contact.Id, "New Name", "test@example.com", null);
        _repository.GetByIdAsync(contact.Id, default).Returns(contact);
        var expectedDto = new ContactDto(contact.Id, "New Name", "test@example.com", null, contact.CreatedAt);
        _mapper.Map<ContactDto>(contact).Returns(expectedDto);

        var result = await _handler.Handle(command, default);

        result.ShouldBe(expectedDto);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ExistingContact_NewEmailNotTaken_UpdatesEmail()
    {
        var contact = Contact.Create("Name", "old@example.com");
        var command = new UpdateContactCommand(contact.Id, "Name", "new@example.com", null);
        _repository.GetByIdAsync(contact.Id, default).Returns(contact);
        _repository.ExistsByEmailExcludingIdAsync("new@example.com", contact.Id, default).Returns(false);
        _mapper.Map<ContactDto>(contact).Returns(
            new ContactDto(contact.Id, "Name", "new@example.com", null, contact.CreatedAt));

        await _handler.Handle(command, default);

        await _repository.Received(1).ExistsByEmailExcludingIdAsync("new@example.com", contact.Id, default);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ExistingContact_NewEmailAlreadyTaken_ThrowsDuplicateEmailException()
    {
        var contact = Contact.Create("Name", "old@example.com");
        var command = new UpdateContactCommand(contact.Id, "Name", "taken@example.com", null);
        _repository.GetByIdAsync(contact.Id, default).Returns(contact);
        _repository.ExistsByEmailExcludingIdAsync("taken@example.com", contact.Id, default).Returns(true);

        await Should.ThrowAsync<DuplicateEmailException>(() => _handler.Handle(command, default));
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ContactNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var command = new UpdateContactCommand(id, "Name", "test@example.com", null);
        _repository.GetByIdAsync(id, default).Returns((Contact?)null);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, default));
    }
}

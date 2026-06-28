using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Contacts;
using Agenda.Application.Contacts.Commands.CreateContact;
using Agenda.Domain.Exceptions;
using AutoMapper;
using NSubstitute;
using Shouldly;

namespace Agenda.Tests.Unit.Handlers;

public class CreateContactHandlerTests
{
    private readonly IContactRepository _repository = Substitute.For<IContactRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly CreateContactHandler _handler;

    public CreateContactHandlerTests()
    {
        _handler = new CreateContactHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesContactAndReturnsDto()
    {
        var command = new CreateContactCommand("Test User", "test@example.com", null);
        _repository.ExistsByEmailAsync(command.Email, default).Returns(false);
        var expectedDto = new ContactDto(Guid.NewGuid(), command.Name, command.Email, null, DateTime.UtcNow);
        _mapper.Map<ContactDto>(Arg.Any<object>()).Returns(expectedDto);

        var result = await _handler.Handle(command, default);

        result.ShouldBe(expectedDto);
        await _repository.Received(1).AddAsync(Arg.Any<Agenda.Domain.Entities.Contact>(), default);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var command = new CreateContactCommand("Test", "existing@example.com", null);
        _repository.ExistsByEmailAsync(command.Email, default).Returns(true);

        await Should.ThrowAsync<DuplicateEmailException>(
            () => _handler.Handle(command, default));

        await _repository.DidNotReceive().AddAsync(Arg.Any<Agenda.Domain.Entities.Contact>(), default);
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsContactBeforeSaving()
    {
        var command = new CreateContactCommand("Test", "test@example.com", "(11) 91234-5678");
        _repository.ExistsByEmailAsync(command.Email, default).Returns(false);
        _mapper.Map<ContactDto>(Arg.Any<object>()).Returns(
            new ContactDto(Guid.NewGuid(), command.Name, command.Email, command.Phone, DateTime.UtcNow));

        await _handler.Handle(command, default);

        Received.InOrder(() =>
        {
            _repository.AddAsync(Arg.Any<Agenda.Domain.Entities.Contact>(), default);
            _repository.SaveChangesAsync(default);
        });
    }
}

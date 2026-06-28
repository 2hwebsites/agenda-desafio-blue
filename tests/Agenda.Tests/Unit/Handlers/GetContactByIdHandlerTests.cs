using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Common.Exceptions;
using Agenda.Application.Contacts;
using Agenda.Application.Contacts.Queries.GetContactById;
using Agenda.Domain.Entities;
using AutoMapper;
using NSubstitute;
using Shouldly;

namespace Agenda.Tests.Unit.Handlers;

public class GetContactByIdHandlerTests
{
    private readonly IContactRepository _repository = Substitute.For<IContactRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetContactByIdHandler _handler;

    public GetContactByIdHandlerTests()
    {
        _handler = new GetContactByIdHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingContact_ReturnsDto()
    {
        var contact = Contact.Create("Test", "test@example.com");
        var query = new GetContactByIdQuery(contact.Id);
        _repository.GetByIdAsync(contact.Id, default).Returns(contact);
        var expectedDto = new ContactDto(contact.Id, "Test", "test@example.com", null, contact.CreatedAt);
        _mapper.Map<ContactDto>(contact).Returns(expectedDto);

        var result = await _handler.Handle(query, default);

        result.ShouldBe(expectedDto);
    }

    [Fact]
    public async Task Handle_ContactNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var query = new GetContactByIdQuery(id);
        _repository.GetByIdAsync(id, default).Returns((Contact?)null);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(query, default));
    }
}

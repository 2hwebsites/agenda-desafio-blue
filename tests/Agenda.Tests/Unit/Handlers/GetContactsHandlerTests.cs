using Agenda.Application.Abstractions.Persistence;
using Agenda.Application.Contacts;
using Agenda.Application.Contacts.Queries.GetContacts;
using Agenda.Domain.Entities;
using AutoMapper;
using NSubstitute;
using Shouldly;

namespace Agenda.Tests.Unit.Handlers;

public class GetContactsHandlerTests
{
    private readonly IContactRepository _repository = Substitute.For<IContactRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetContactsHandler _handler;

    public GetContactsHandlerTests()
    {
        _handler = new GetContactsHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WithCorrectMetadata()
    {
        var contacts = new List<Contact>
        {
            Contact.Create("Alice", "alice@test.com"),
            Contact.Create("Bob", "bob@test.com"),
        };
        var query = new GetContactsQuery(null, 1, 20);
        _repository.GetPagedAsync(null, 1, 20, default).Returns((contacts, 2));
        var dtos = contacts.Select(c => new ContactDto(c.Id, c.Name, c.Email, null, c.CreatedAt)).ToList();
        _mapper.Map<IReadOnlyList<ContactDto>>(contacts).Returns(dtos);

        var result = await _handler.Handle(query, default);

        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(20);
        result.TotalPages.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_PageBelowOne_ClampsToOne()
    {
        var query = new GetContactsQuery(null, -5, 20);
        _repository.GetPagedAsync(null, 1, 20, default).Returns((new List<Contact>(), 0));
        _mapper.Map<IReadOnlyList<ContactDto>>(Arg.Any<object>()).Returns(Array.Empty<ContactDto>());

        var result = await _handler.Handle(query, default);

        result.Page.ShouldBe(1);
        await _repository.Received(1).GetPagedAsync(null, 1, 20, default);
    }

    [Fact]
    public async Task Handle_PageSizeAbove100_ClampedTo100()
    {
        var query = new GetContactsQuery(null, 1, 999);
        _repository.GetPagedAsync(null, 1, 100, default).Returns((new List<Contact>(), 0));
        _mapper.Map<IReadOnlyList<ContactDto>>(Arg.Any<object>()).Returns(Array.Empty<ContactDto>());

        var result = await _handler.Handle(query, default);

        result.PageSize.ShouldBe(100);
        await _repository.Received(1).GetPagedAsync(null, 1, 100, default);
    }

    [Fact]
    public async Task Handle_TotalPagesCalculation_RoundsUp()
    {
        var contacts = Enumerable.Range(1, 5)
            .Select(i => Contact.Create($"User {i}", $"user{i}@test.com"))
            .ToList();
        var query = new GetContactsQuery(null, 1, 3);
        _repository.GetPagedAsync(null, 1, 3, default).Returns(((IReadOnlyList<Contact>)contacts.Take(3).ToList(), 7));
        _mapper.Map<IReadOnlyList<ContactDto>>(Arg.Any<object>()).Returns(Array.Empty<ContactDto>());

        var result = await _handler.Handle(query, default);

        result.TotalPages.ShouldBe(3);
    }
}

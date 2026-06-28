using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Agenda.Application.Common.Models;
using Agenda.Application.Contacts;
using Shouldly;

namespace Agenda.Tests.Integration;

[Trait("Category", "Integration")]
public class ContactsEndpointsTests : IClassFixture<AgendaApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AgendaApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ContactsEndpointsTests(AgendaApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<ContactDto> CreateContactAsync(string name, string email, string? phone = null)
    {
        var response = await _client.PostAsJsonAsync("/api/contacts", new { name, email, phone });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ContactDto>(JsonOpts))!;
    }

    [Fact]
    public async Task GetContacts_EmptyDb_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/contacts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ContactDto>>(JsonOpts);
        result!.TotalCount.ShouldBe(0);
        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetContacts_WithContacts_ReturnsPaginatedResult()
    {
        await CreateContactAsync("Alice", "alice@test.com");
        await CreateContactAsync("Bob", "bob@test.com");
        await CreateContactAsync("Carol", "carol@test.com");

        var response = await _client.GetAsync("/api/contacts?pageSize=2&page=1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ContactDto>>(JsonOpts);
        result!.TotalCount.ShouldBe(3);
        result.Items.Count.ShouldBe(2);
        result.TotalPages.ShouldBe(2);
    }

    [Fact]
    public async Task GetContacts_Search_ReturnsMatchingContacts()
    {
        await CreateContactAsync("Alice Smith", "alice@test.com");
        await CreateContactAsync("Bob Jones", "bob@test.com");

        var response = await _client.GetAsync("/api/contacts?search=alice");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ContactDto>>(JsonOpts);
        result!.TotalCount.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Alice Smith");
    }

    [Fact]
    public async Task CreateContact_ValidData_Returns201WithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/contacts",
            new { name = "Test User", email = "test@example.com", phone = "(11) 91234-5678" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var dto = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOpts);
        dto!.Name.ShouldBe("Test User");
        dto.Email.ShouldBe("test@example.com");
        dto.Phone.ShouldBe("(11) 91234-5678");
        dto.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateContact_DuplicateEmail_Returns409WithProblemJson()
    {
        await CreateContactAsync("First", "dup@test.com");

        var response = await _client.PostAsJsonAsync("/api/contacts",
            new { name = "Second", email = "dup@test.com", phone = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task CreateContact_InvalidData_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/contacts",
            new { name = "", email = "not-an-email", phone = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("errors", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task GetContactById_ExistingId_Returns200()
    {
        var created = await CreateContactAsync("Detail Test", "detail@test.com", "(11) 91234-5678");

        var response = await _client.GetAsync($"/api/contacts/{created.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOpts);
        dto!.Id.ShouldBe(created.Id);
        dto.Name.ShouldBe("Detail Test");
        dto.Phone.ShouldBe("(11) 91234-5678");
    }

    [Fact]
    public async Task GetContactById_NonExistentId_Returns404WithProblemJson()
    {
        var response = await _client.GetAsync($"/api/contacts/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Contato não encontrado");
    }

    [Fact]
    public async Task UpdateContact_ExistingContact_Returns200WithUpdatedData()
    {
        var created = await CreateContactAsync("Original Name", "original@test.com");

        var response = await _client.PutAsJsonAsync($"/api/contacts/{created.Id}",
            new { name = "Updated Name", email = "updated@test.com", phone = "(21) 98765-4321" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOpts);
        dto!.Name.ShouldBe("Updated Name");
        dto.Email.ShouldBe("updated@test.com");
        dto.Phone.ShouldBe("(21) 98765-4321");
    }

    [Fact]
    public async Task UpdateContact_NonExistentId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/contacts/{Guid.NewGuid()}",
            new { name = "Test", email = "test@test.com", phone = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContact_EmailTakenByAnotherContact_Returns409()
    {
        var first = await CreateContactAsync("First", "first@test.com");
        var second = await CreateContactAsync("Second", "second@test.com");

        var response = await _client.PutAsJsonAsync($"/api/contacts/{second.Id}",
            new { name = "Second", email = "first@test.com", phone = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateContact_SameEmail_Returns200()
    {
        var created = await CreateContactAsync("Name", "same@test.com");

        var response = await _client.PutAsJsonAsync($"/api/contacts/{created.Id}",
            new { name = "Name Updated", email = "same@test.com", phone = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteContact_ExistingContact_Returns204()
    {
        var created = await CreateContactAsync("To Delete", "delete@test.com");

        var response = await _client.DeleteAsync($"/api/contacts/{created.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteContact_NonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/contacts/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteContact_DeletedContactNotReturnedInGetById()
    {
        var created = await CreateContactAsync("Soft Delete", "softdel@test.com");
        await _client.DeleteAsync($"/api/contacts/{created.Id}");

        var response = await _client.GetAsync($"/api/contacts/{created.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteContact_DeletedContactNotReturnedInList()
    {
        var a = await CreateContactAsync("Keep", "keep@test.com");
        var b = await CreateContactAsync("Remove", "remove@test.com");
        await _client.DeleteAsync($"/api/contacts/{b.Id}");

        var response = await _client.GetAsync("/api/contacts");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ContactDto>>(JsonOpts);

        result!.TotalCount.ShouldBe(1);
        result.Items[0].Id.ShouldBe(a.Id);
    }
}

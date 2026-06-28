using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Agenda.Api.Auth;
using Shouldly;

namespace Agenda.Tests.Integration;

[Trait("Category", "Integration")]
public class AuthEndpointsTests : IClassFixture<AgendaApiFactory>
{
    private readonly HttpClient _client;
    private readonly AgendaApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AuthEndpointsTests(AgendaApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = AgendaApiFactory.TestUsername, password = AgendaApiFactory.TestPassword });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts);
        body!.Token.ShouldNotBeNullOrEmpty();
        body.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = AgendaApiFactory.TestUsername, password = "wrongpassword" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WrongUsername_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "unknownuser", password = AgendaApiFactory.TestPassword });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ResponseContainsPtBrDetail()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "wrong", password = "wrong" });

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Usuário ou senha inválidos");
    }

    [Fact]
    public async Task GetContacts_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/contacts");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContacts_WithInvalidToken_Returns401()
    {
        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await authClient.GetAsync("/api/contacts");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContacts_WithValidToken_Returns200()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = AgendaApiFactory.TestUsername, password = AgendaApiFactory.TestPassword });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts);

        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var response = await authClient.GetAsync("/api/contacts");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

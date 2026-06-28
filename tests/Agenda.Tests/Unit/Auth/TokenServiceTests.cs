using System.IdentityModel.Tokens.Jwt;
using Agenda.Api.Auth;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Agenda.Tests.Unit.Auth;

public class TokenServiceTests
{
    private readonly TokenService _service;

    public TokenServiceTests()
    {
        _service = new TokenService(Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "test-secret-key-that-is-at-least-32-characters-long",
            ExpiresMinutes = 30,
        }));
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var (token, _) = _service.GenerateToken("admin", "admin");
        token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_TokenContainsSubClaim()
    {
        var (token, _) = _service.GenerateToken("admin", "admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Subject.ShouldBe("admin");
    }

    [Fact]
    public void GenerateToken_TokenContainsNonEmptyJti()
    {
        var (token, _) = _service.GenerateToken("admin", "admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Id.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ExpiresAtMatchesConfiguredMinutes()
    {
        var before = DateTime.UtcNow;
        var (_, expiresAt) = _service.GenerateToken("admin", "admin");
        expiresAt.ShouldBeGreaterThanOrEqualTo(before.AddMinutes(30));
        expiresAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow.AddMinutes(30).AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_TwoCalls_ProduceDifferentJti()
    {
        var (token1, _) = _service.GenerateToken("admin", "admin");
        var (token2, _) = _service.GenerateToken("admin", "admin");
        var handler = new JwtSecurityTokenHandler();
        handler.ReadJwtToken(token1).Id.ShouldNotBe(handler.ReadJwtToken(token2).Id);
    }

    [Fact]
    public void GenerateToken_TokenHasCorrectIssuerAndAudience()
    {
        var (token, _) = _service.GenerateToken("admin", "admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.ShouldBe("test-issuer");
        jwt.Audiences.ShouldContain("test-audience");
    }
}

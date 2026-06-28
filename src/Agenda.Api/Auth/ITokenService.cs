namespace Agenda.Api.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(string username, string role);
}

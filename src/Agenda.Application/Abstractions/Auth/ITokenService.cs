using Agenda.Application.Authentication;

namespace Agenda.Application.Abstractions.Auth;

public interface ITokenService
{
    TokenResult GenerateToken(string username, string role);
}

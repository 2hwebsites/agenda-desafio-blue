using Agenda.Application.Abstractions.Auth;
using Agenda.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;

namespace Agenda.Application.Authentication.Login;

public sealed class LoginHandler(
    IOptions<AuthSeedOptions> seedOptions,
    ITokenService tokenService) : IRequestHandler<LoginCommand, TokenResult>
{
    public Task<TokenResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var seed = seedOptions.Value;

        var usernameMatch = string.Equals(request.Username, seed.Username, StringComparison.Ordinal);
        var passwordMatch = string.Equals(request.Password, seed.Password, StringComparison.Ordinal);

        if (!usernameMatch || !passwordMatch)
            throw new InvalidCredentialsException();

        var result = tokenService.GenerateToken(request.Username, "admin");
        return Task.FromResult(result);
    }
}

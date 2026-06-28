using Agenda.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Agenda.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous]
public sealed class AuthController(
    ITokenService tokenService,
    IOptions<AuthSeedOptions> seedOptions) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var seed = seedOptions.Value;

        var usernameMatch = string.Equals(request.Username, seed.Username, StringComparison.Ordinal);
        var passwordMatch = string.Equals(request.Password, seed.Password, StringComparison.Ordinal);

        if (!usernameMatch || !passwordMatch)
        {
            return Problem(
                detail: "Usuário ou senha inválidos.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");
        }

        var (token, expiresAt) = tokenService.GenerateToken(request.Username, "admin");
        return Ok(new LoginResponse(token, expiresAt));
    }
}

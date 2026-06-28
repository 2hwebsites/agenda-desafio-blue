using MediatR;

namespace Agenda.Application.Authentication.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<TokenResult>;

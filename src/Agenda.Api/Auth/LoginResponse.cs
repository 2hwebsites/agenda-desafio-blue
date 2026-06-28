namespace Agenda.Api.Auth;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);

namespace Agenda.Application.Authentication;

public sealed record TokenResult(string Token, DateTime ExpiresAt);

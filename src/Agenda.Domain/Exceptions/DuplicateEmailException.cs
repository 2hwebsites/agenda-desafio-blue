namespace Agenda.Domain.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"A contact with e-mail '{email}' already exists.") { }
}

namespace Agenda.Domain.Entities;

public sealed class Contato
{
    private Contato() { }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Excluido { get; private set; }

    public static Contato Criar(string nome, string email, string? telefone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Contato
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Telefone = telefone?.Trim(),
            CriadoEm = DateTime.UtcNow,
            Excluido = false,
        };
    }

    public void Atualizar(string nome, string email, string? telefone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone?.Trim();
    }

    public void MarcarExcluido() => Excluido = true;
}

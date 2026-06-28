using FluentValidation;

namespace Agenda.Application.Contacts;

public static class ContactRuleExtensions
{
    private const string PhonePattern = @"^(\(?\d{2}\)?\s?)?(\d{4,5}[-\s]?\d{4})$";

    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.");

    public static IRuleBuilderOptions<T, string?> ValidPhone<T>(this IRuleBuilder<T, string?> rule)
        => rule
            .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.")
            .Matches(PhonePattern).WithMessage("O telefone deve estar no formato (DDD) XXXXX-XXXX.");
}

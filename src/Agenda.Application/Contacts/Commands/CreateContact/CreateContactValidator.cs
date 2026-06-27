using FluentValidation;

namespace Agenda.Application.Contacts.Commands.CreateContact;

public sealed class CreateContactValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.")
            .Matches(@"^(\(?\d{2}\)?\s?)?(\d{4,5}[-\s]?\d{4})$")
            .WithMessage("O telefone deve estar no formato (DDD) XXXXX-XXXX.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

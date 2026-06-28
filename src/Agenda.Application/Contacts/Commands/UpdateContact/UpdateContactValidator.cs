using Agenda.Application.Contacts;
using FluentValidation;

namespace Agenda.Application.Contacts.Commands.UpdateContact;

public sealed class UpdateContactValidator : AbstractValidator<UpdateContactCommand>
{
    public UpdateContactValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O identificador do contato é obrigatório.");
        RuleFor(x => x.Name).ValidName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Phone).ValidPhone().When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

using Agenda.Application.Contacts;
using FluentValidation;

namespace Agenda.Application.Contacts.Commands.CreateContact;

public sealed class CreateContactValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactValidator()
    {
        RuleFor(x => x.Name).ValidName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Phone).ValidPhone().When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

using FluentValidation;

namespace FinanceiroApi.Application.Commands.Customers.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TaxId)
            .NotEmpty()
            .Matches(@"^\d{11}$|^\d{14}$")
            .WithMessage("TaxId deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ), sem pontuação.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.PersonType)
            .IsInEnum();

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone is not null);

        RuleFor(x => x.ContactName)
            .MaximumLength(100)
            .When(x => x.ContactName is not null);
    }
}
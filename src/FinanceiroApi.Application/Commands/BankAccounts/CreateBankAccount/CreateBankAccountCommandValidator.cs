using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankAccounts.CreateBankAccount;

public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.BankName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BankCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Agency)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.AccountType)
            .IsInEnum();

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PixKey)
            .MaximumLength(100)
            .When(x => x.PixKey is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);
    }
}
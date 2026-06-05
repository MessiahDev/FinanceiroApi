using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankAccounts.TransferBetweenAccounts;

public class TransferBetweenAccountsCommandValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);

        RuleFor(x => x.DestinationAccountId)
            .NotEqual(x => x.SourceAccountId)
            .WithMessage("Conta de origem e destino não podem ser iguais.");
    }
}
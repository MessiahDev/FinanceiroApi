using FluentValidation;

namespace FinanceiroApi.Application.Commands.Transactions.ConfirmTransaction;

public class ConfirmTransactionCommandValidator : AbstractValidator<ConfirmTransactionCommand>
{
    public ConfirmTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountsReceivable.ReceivePayment;

public class ReceivePaymentCommandValidator : AbstractValidator<ReceivePaymentCommand>
{
    public ReceivePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.ReceiptDate)
            .NotEmpty();

        RuleFor(x => x.BankAccountId)
            .NotEmpty();
    }
}
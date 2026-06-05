using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountsPayable.PayAccountPayable;

public class PayAccountPayableCommandValidator : AbstractValidator<PayAccountPayableCommand>
{
    public PayAccountPayableCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentDate).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
    }
}
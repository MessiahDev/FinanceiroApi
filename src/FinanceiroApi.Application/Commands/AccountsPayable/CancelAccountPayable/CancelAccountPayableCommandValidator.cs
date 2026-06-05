using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountsPayable.CancelAccountPayable;

public class CancelAccountPayableCommandValidator : AbstractValidator<CancelAccountPayableCommand>
{
    public CancelAccountPayableCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
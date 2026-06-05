using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountsPayable.CreateAccountPayable;

public class CreateAccountPayableCommandValidator : AbstractValidator<CreateAccountPayableCommand>
{
    public CreateAccountPayableCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0);

        RuleFor(x => x.DueDate)
            .NotEmpty();

        RuleFor(x => x.InvoiceNumber)
            .MaximumLength(50)
            .When(x => x.InvoiceNumber is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);
    }
}
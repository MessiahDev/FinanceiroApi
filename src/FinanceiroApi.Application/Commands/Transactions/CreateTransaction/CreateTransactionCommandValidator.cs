using FluentValidation;

namespace FinanceiroApi.Application.Commands.Transactions.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type).NotEmpty()
            .Must(t => t is "Debit" or "Credit")
            .WithMessage("Tipo deve ser 'Debit' ou 'Credit'.");
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}
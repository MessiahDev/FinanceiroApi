using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankReconciliations.AddReconciliationItem;

public class AddReconciliationItemCommandValidator : AbstractValidator<AddReconciliationItemCommand>
{
    public AddReconciliationItemCommandValidator()
    {
        RuleFor(x => x.ReconciliationId).NotEmpty().WithMessage("Id da conciliação é obrigatório.");
        RuleFor(x => x.BankStatementEntryId).NotEmpty().WithMessage("Id do lançamento do extrato é obrigatório.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
    }
}

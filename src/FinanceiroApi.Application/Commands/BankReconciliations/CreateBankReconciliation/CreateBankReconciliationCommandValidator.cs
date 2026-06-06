using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankReconciliations.CreateBankReconciliation;

public class CreateBankReconciliationCommandValidator : AbstractValidator<CreateBankReconciliationCommand>
{
    public CreateBankReconciliationCommandValidator()
    {
        RuleFor(x => x.BankAccountId).NotEmpty().WithMessage("Conta bancária é obrigatória.");
        RuleFor(x => x.BankStatementId).NotEmpty().WithMessage("Extrato bancário é obrigatório.");
    }
}

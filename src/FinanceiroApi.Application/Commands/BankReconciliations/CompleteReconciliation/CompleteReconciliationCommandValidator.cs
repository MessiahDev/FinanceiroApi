using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankReconciliations.CompleteReconciliation;

public class CompleteReconciliationCommandValidator : AbstractValidator<CompleteReconciliationCommand>
{
    public CompleteReconciliationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");
        RuleFor(x => x.CompletedBy).NotEmpty().WithMessage("Usuário responsável é obrigatório.");
    }
}

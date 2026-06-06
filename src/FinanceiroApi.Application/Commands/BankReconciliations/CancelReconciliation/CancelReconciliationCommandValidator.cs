using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankReconciliations.CancelReconciliation;

public class CancelReconciliationCommandValidator : AbstractValidator<CancelReconciliationCommand>
{
    public CancelReconciliationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Motivo do cancelamento é obrigatório.")
            .MaximumLength(500).WithMessage("Motivo não pode exceder 500 caracteres.");
    }
}

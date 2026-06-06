using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankStatements.CancelBankStatement;

public class CancelBankStatementCommandValidator : AbstractValidator<CancelBankStatementCommand>
{
    public CancelBankStatementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Motivo do cancelamento é obrigatório.")
            .MaximumLength(500).WithMessage("Motivo não pode exceder 500 caracteres.");
    }
}

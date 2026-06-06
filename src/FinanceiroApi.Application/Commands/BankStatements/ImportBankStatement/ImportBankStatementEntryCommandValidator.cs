using FluentValidation;

namespace FinanceiroApi.Application.Commands.BankStatements.ImportBankStatement;

public class ImportBankStatementCommandValidator : AbstractValidator<ImportBankStatementCommand>
{
    public ImportBankStatementCommandValidator()
    {
        RuleFor(x => x.BankAccountId).NotEmpty().WithMessage("Conta bancária é obrigatória.");
        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("Data fim não pode ser anterior à data início.");
        RuleFor(x => x.Entries)
            .NotEmpty().WithMessage("O extrato deve conter ao menos um lançamento.");
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.Description).NotEmpty().WithMessage("Descrição do lançamento é obrigatória.");
            entry.RuleFor(e => e.Amount).GreaterThan(0).WithMessage("Valor do lançamento deve ser maior que zero.");
        });
    }
}

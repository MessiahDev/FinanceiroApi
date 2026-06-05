using FluentValidation;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição do lançamento é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("A data do lançamento é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("A data do lançamento não pode ser futura.");

        RuleFor(x => x.EntryType)
            .IsInEnum().WithMessage("Tipo de lançamento inválido.");

        RuleFor(x => x.AccountingPeriodId)
            .NotEmpty().WithMessage("O período contábil é obrigatório.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("O usuário responsável é obrigatório.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("O lançamento deve ter ao menos uma linha.")
            .Must(lines => lines.Count >= 2)
            .WithMessage("O lançamento deve ter ao menos duas linhas (débito e crédito).");

        RuleFor(x => x.Lines)
            .Must(lines =>
            {
                var debits = lines.Where(l => l.DebitCredit == DebitCredit.Debit).Sum(l => l.Amount);
                var credits = lines.Where(l => l.DebitCredit == DebitCredit.Credit).Sum(l => l.Amount);
                return debits == credits;
            })
            .WithMessage("O lançamento está desequilibrado. A soma dos débitos deve ser igual à soma dos créditos.")
            .When(x => x.Lines?.Count >= 2);

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ChartOfAccountId)
                .NotEmpty().WithMessage("A conta contábil da linha é obrigatória.");

            line.RuleFor(l => l.Amount)
                .GreaterThan(0).WithMessage("O valor da linha deve ser maior que zero.");

            line.RuleFor(l => l.DebitCredit)
                .IsInEnum().WithMessage("Débito/Crédito da linha inválido.");
        });
    }
}

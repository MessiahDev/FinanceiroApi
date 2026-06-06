using FluentValidation;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descriÃ§Ã£o do lanÃ§amento Ã© obrigatÃ³ria.")
            .MaximumLength(500).WithMessage("A descriÃ§Ã£o deve ter no mÃ¡ximo 500 caracteres.");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("A data do lanÃ§amento Ã© obrigatÃ³ria.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("A data do lanÃ§amento nÃ£o pode ser futura.");

        RuleFor(x => x.EntryType)
            .IsInEnum().WithMessage("Tipo de lanÃ§amento invÃ¡lido.");

        RuleFor(x => x.AccountingPeriodId)
            .NotEmpty().WithMessage("O perÃ­odo contÃ¡bil Ã© obrigatÃ³rio.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("O lanÃ§amento deve ter ao menos uma linha.")
            .Must(lines => lines.Count >= 2)
            .WithMessage("O lanÃ§amento deve ter ao menos duas linhas (dÃ©bito e crÃ©dito).");

        RuleFor(x => x.Lines)
            .Must(lines =>
            {
                var debits = lines.Where(l => l.DebitCredit == DebitCredit.Debit).Sum(l => l.Amount);
                var credits = lines.Where(l => l.DebitCredit == DebitCredit.Credit).Sum(l => l.Amount);
                return debits == credits;
            })
            .WithMessage("O lanÃ§amento estÃ¡ desequilibrado. A soma dos dÃ©bitos deve ser igual Ã  soma dos crÃ©ditos.")
            .When(x => x.Lines?.Count >= 2);

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ChartOfAccountId)
                .NotEmpty().WithMessage("A conta contÃ¡bil da linha Ã© obrigatÃ³ria.");

            line.RuleFor(l => l.Amount)
                .GreaterThan(0).WithMessage("O valor da linha deve ser maior que zero.");

            line.RuleFor(l => l.DebitCredit)
                .IsInEnum().WithMessage("DÃ©bito/CrÃ©dito da linha invÃ¡lido.");
        });
    }
}



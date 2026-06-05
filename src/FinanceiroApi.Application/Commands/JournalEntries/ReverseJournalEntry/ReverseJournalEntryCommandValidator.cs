using FluentValidation;

namespace FinanceiroApi.Application.Commands.JournalEntries.ReverseJournalEntry;

public class ReverseJournalEntryCommandValidator : AbstractValidator<ReverseJournalEntryCommand>
{
    public ReverseJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O id do lançamento é obrigatório.");
        RuleFor(x => x.ReversalDescription)
            .NotEmpty().WithMessage("A descrição do estorno é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
        RuleFor(x => x.ReversedByUserId).NotEmpty().WithMessage("O usuário responsável é obrigatório.");
    }
}

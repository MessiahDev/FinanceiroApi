using FluentValidation;

namespace FinanceiroApi.Application.Commands.JournalEntries.PostJournalEntry;

public class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
	public PostJournalEntryCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("O id do lançamento é obrigatório.");
	}
}
using MediatR;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;

public record CreateJournalEntryLineRequest(
    Guid ChartOfAccountId,
    DebitCredit DebitCredit,
    decimal Amount,
    string? Description
);

public record CreateJournalEntryCommand(
    string Description,
    DateTime EntryDate,
    JournalEntryType EntryType,
    Guid AccountingPeriodId,
    Guid CreatedByUserId,
    string? ReferenceDocument,
    string? ReferenceDocumentType,
    Guid? ReferenceDocumentId,
    IList<CreateJournalEntryLineRequest> Lines
) : IRequest<Guid>;

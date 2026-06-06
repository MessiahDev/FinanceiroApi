using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Entities;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public JournalEntry? JournalEntry { get; private set; }

    public Guid ChartOfAccountId { get; private set; }
    public ChartOfAccount? ChartOfAccount { get; private set; }

    public DebitCredit DebitCredit { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public int LineOrder { get; private set; }

    protected JournalEntryLine() { }

    private JournalEntryLine(
        Guid journalEntryId,
        Guid chartOfAccountId,
        DebitCredit debitCredit,
        decimal amount,
        string? description)
    {
        JournalEntryId = journalEntryId;
        ChartOfAccountId = chartOfAccountId;
        DebitCredit = debitCredit;
        Amount = amount;
        Description = description;
    }

    internal static JournalEntryLine Create(
        Guid journalEntryId,
        Guid chartOfAccountId,
        DebitCredit debitCredit,
        decimal amount,
        string? description)
    {
        if (journalEntryId == Guid.Empty)
            throw new DomainException("O lançamento é obrigatório.");

        if (chartOfAccountId == Guid.Empty)
            throw new DomainException("A conta contábil é obrigatória.");

        if (amount <= 0)
            throw new DomainException("O valor da linha deve ser maior que zero.");

        return new JournalEntryLine(journalEntryId, chartOfAccountId, debitCredit, amount, description);
    }

    internal void SetLineOrder(int order) => LineOrder = order;
}

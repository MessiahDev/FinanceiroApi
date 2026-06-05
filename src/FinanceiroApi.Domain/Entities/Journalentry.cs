using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class JournalEntry : AggregateRoot
{
    public string EntryNumber { get; private set; } = string.Empty;   // Número sequencial do lançamento
    public string Description { get; private set; } = string.Empty;
    public DateTime EntryDate { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public JournalEntryType EntryType { get; private set; }
    public string? ReferenceDocument { get; private set; }            // Ex: NF-001, REC-002
    public string? ReferenceDocumentType { get; private set; }        // Ex: "AccountPayable", "Payroll"
    public Guid? ReferenceDocumentId { get; private set; }            // FK para a entidade de origem

    public Guid AccountingPeriodId { get; private set; }
    public AccountingPeriod? AccountingPeriod { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();
    private readonly List<JournalEntryLine> _lines = new();

    protected JournalEntry() { }

    private JournalEntry(
        string entryNumber,
        string description,
        DateTime entryDate,
        JournalEntryType entryType,
        Guid accountingPeriodId,
        Guid createdByUserId,
        string? referenceDocument,
        string? referenceDocumentType,
        Guid? referenceDocumentId)
    {
        EntryNumber = entryNumber;
        Description = description;
        EntryDate = entryDate;
        EntryType = entryType;
        AccountingPeriodId = accountingPeriodId;
        CreatedByUserId = createdByUserId;
        ReferenceDocument = referenceDocument;
        ReferenceDocumentType = referenceDocumentType;
        ReferenceDocumentId = referenceDocumentId;
        Status = JournalEntryStatus.Draft;
    }

    public static JournalEntry Create(
        string entryNumber,
        string description,
        DateTime entryDate,
        JournalEntryType entryType,
        Guid accountingPeriodId,
        Guid createdByUserId,
        string? referenceDocument = null,
        string? referenceDocumentType = null,
        Guid? referenceDocumentId = null)
    {
        if (string.IsNullOrWhiteSpace(entryNumber))
            throw new DomainException("O número do lançamento é obrigatório.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("A descrição do lançamento é obrigatória.");

        if (entryDate == default)
            throw new DomainException("A data do lançamento é obrigatória.");

        if (accountingPeriodId == Guid.Empty)
            throw new DomainException("O período contábil é obrigatório.");

        return new JournalEntry(entryNumber, description, entryDate, entryType,
            accountingPeriodId, createdByUserId, referenceDocument, referenceDocumentType, referenceDocumentId);
    }

    public void AddLine(Guid chartOfAccountId, DebitCredit debitCredit, decimal amount, string? lineDescription = null)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new DomainException("Apenas lançamentos em rascunho permitem adição de linhas.");

        if (amount <= 0)
            throw new DomainException("O valor da linha deve ser maior que zero.");

        var line = JournalEntryLine.Create(Id, chartOfAccountId, debitCredit, amount, lineDescription);
        _lines.Add(line);
    }

    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new DomainException("Apenas lançamentos em rascunho podem ser contabilizados.");

        if (!_lines.Any())
            throw new DomainException("O lançamento deve ter ao menos uma linha.");

        ValidateDoubleEntry();

        Status = JournalEntryStatus.Posted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JournalEntryPostedEvent(Id, EntryNumber, EntryDate, TotalDebits()));
    }

    public void Reverse(string reversalDescription, Guid createdByUserId)
    {
        if (Status != JournalEntryStatus.Posted)
            throw new DomainException("Apenas lançamentos contabilizados podem ser estornados.");

        Status = JournalEntryStatus.Reversed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JournalEntryReversedEvent(Id, EntryNumber, reversalDescription, createdByUserId));
    }

    /// <summary>Valida a regra das partidas dobradas: ∑Débitos == ∑Créditos</summary>
    private void ValidateDoubleEntry()
    {
        var totalDebits = _lines.Where(l => l.DebitCredit == DebitCredit.Debit).Sum(l => l.Amount);
        var totalCredits = _lines.Where(l => l.DebitCredit == DebitCredit.Credit).Sum(l => l.Amount);

        if (totalDebits != totalCredits)
            throw new DomainException(
                $"Lançamento desequilibrado. Débitos: {totalDebits:C} | Créditos: {totalCredits:C}");
    }

    public decimal TotalDebits() => _lines.Where(l => l.DebitCredit == DebitCredit.Debit).Sum(l => l.Amount);
    public decimal TotalCredits() => _lines.Where(l => l.DebitCredit == DebitCredit.Credit).Sum(l => l.Amount);
    public bool IsBalanced() => TotalDebits() == TotalCredits();
}
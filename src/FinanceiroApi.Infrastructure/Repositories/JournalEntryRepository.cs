using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;

namespace FinanceiroApi.Infrastructure.Repositories;

public class JournalEntryRepository : RepositoryBase<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(AppDbContext context) : base(context) { }

    public async Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Lines).ThenInclude(l => l.ChartOfAccount)
            .FirstOrDefaultAsync(e => e.EntryNumber == entryNumber, cancellationToken);

    public async Task<JournalEntry?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Lines).ThenInclude(l => l.ChartOfAccount)
            .Include(e => e.AccountingPeriod)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IEnumerable<JournalEntry>> GetByPeriodAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Lines)
            .Include(e => e.AccountingPeriod)
            .Where(e => e.AccountingPeriodId == accountingPeriodId)
            .OrderByDescending(e => e.EntryDate)
            .ThenBy(e => e.EntryNumber)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JournalEntry>> GetByAccountAsync(Guid chartOfAccountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Lines).ThenInclude(l => l.ChartOfAccount)
            .Where(e => e.Status == JournalEntryStatus.Posted
                     && e.EntryDate >= from
                     && e.EntryDate <= to
                     && e.Lines.Any(l => l.ChartOfAccountId == chartOfAccountId))
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.EntryNumber)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JournalEntry>> GetByReferenceDocumentAsync(
        string referenceDocumentType, Guid referenceDocumentId, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(e => e.ReferenceDocumentType == referenceDocumentType
                     && e.ReferenceDocumentId == referenceDocumentId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JournalEntry>> GetPostedEntriesAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Lines)
            .Where(e => e.AccountingPeriodId == accountingPeriodId
                     && e.Status == JournalEntryStatus.Posted)
            .ToListAsync(cancellationToken);

    public async Task<string> GetNextEntryNumberAsync(int year, CancellationToken cancellationToken = default)
    {
        var prefix = $"LC{year}-";
        var lastEntry = await DbSet
            .Where(e => e.EntryNumber.StartsWith(prefix))
            .OrderByDescending(e => e.EntryNumber)
            .Select(e => e.EntryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEntry is null)
            return $"{prefix}00001";

        var lastNumber = int.Parse(lastEntry.Replace(prefix, ""));
        return $"{prefix}{(lastNumber + 1):D5}";
    }
}

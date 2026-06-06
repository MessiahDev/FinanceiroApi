using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class TaxEntryRepository : RepositoryBase<TaxEntry>, ITaxEntryRepository
{
    public TaxEntryRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TaxEntry>> GetByTaxTypeAsync(TaxType taxType, CancellationToken ct = default)
        => await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Where(e => e.TaxType == taxType && !e.IsDeleted)
            .OrderByDescending(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxEntry>> GetByStatusAsync(TaxEntryStatus status, CancellationToken ct = default)
        => await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Where(e => e.Status == status && !e.IsDeleted)
            .OrderByDescending(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxEntry>> GetByCompetenceAsync(int year, int month, CancellationToken ct = default)
        => await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Where(e => e.Competence.Year == year && e.Competence.Month == month && !e.IsDeleted)
            .OrderBy(e => e.TaxType)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxEntry>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Where(e => e.DueDate >= from && e.DueDate <= to && !e.IsDeleted)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxEntry>> GetOverdueAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Where(e => e.DueDate < today
                && (e.Status == TaxEntryStatus.Pending || e.Status == TaxEntryStatus.Calculated)
                && !e.IsDeleted)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);
    }

    public async Task<TaxEntry?> GetWithPaymentsAsync(Guid id, CancellationToken ct = default)
        => await Context.TaxEntries
            .Include(e => e.CostCenter)
            .Include(e => e.Payments)
                .ThenInclude(p => p.BankAccount)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}

using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;

namespace FinanceiroApi.Infrastructure.Repositories;

public class AccountingPeriodRepository : RepositoryBase<AccountingPeriod>, IAccountingPeriodRepository
{
    public AccountingPeriodRepository(AppDbContext context) : base(context) { }

    public async Task<AccountingPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);

    public async Task<AccountingPeriod?> GetCurrentOpenPeriodAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(p => p.Status == AccountingPeriodStatus.Open)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccountingPeriod>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(p => p.Year == year)
            .OrderBy(p => p.Month)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<AccountingPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .Where(p => p.Status == AccountingPeriodStatus.Open)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByYearMonthAsync(int year, int month, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await DbSet
            .AnyAsync(p => p.Year == year && p.Month == month && (excludeId == null || p.Id != excludeId), cancellationToken);
}

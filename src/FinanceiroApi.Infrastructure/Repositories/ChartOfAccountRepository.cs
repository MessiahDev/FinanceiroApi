using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;

namespace FinanceiroApi.Infrastructure.Repositories;

public class ChartOfAccountRepository : RepositoryBase<ChartOfAccount>, IChartOfAccountRepository
{
    public ChartOfAccountRepository(AppDbContext context) : base(context) { }

    public async Task<ChartOfAccount?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

    public async Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(a => a.AccountType == accountType && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ChartOfAccount>> GetRootAccountsAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .Include(a => a.ChildAccounts)
            .Where(a => a.ParentAccountId == null)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ChartOfAccount>> GetChildAccountsAsync(Guid parentId, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(a => a.ParentAccountId == parentId)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ChartOfAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ChartOfAccount>> GetAccountsAcceptingEntriesAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .Where(a => a.IsActive && a.AcceptsEntries)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await DbSet
            .AnyAsync(a => a.Code == code && (excludeId == null || a.Id != excludeId), cancellationToken);
}

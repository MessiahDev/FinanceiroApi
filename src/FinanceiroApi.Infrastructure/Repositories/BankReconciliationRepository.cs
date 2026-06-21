using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class BankReconciliationRepository : RepositoryBase<BankReconciliation>, IBankReconciliationRepository
{
    public BankReconciliationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<BankReconciliation>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default)
        => await Context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.BankStatement)
            .Include(r => r.Items)
            .Where(r => r.BankAccountId == bankAccountId && !r.IsDeleted)
            .OrderByDescending(r => r.PeriodStart)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BankReconciliation>> GetAllDetailedAsync(CancellationToken ct = default)
        => await Context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.BankStatement)
            .Include(r => r.Items)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.PeriodStart)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BankReconciliation>> GetByStatusAsync(ReconciliationStatus status, CancellationToken ct = default)
        => await Context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.BankStatement)
            .Include(r => r.Items)
            .Where(r => r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.PeriodStart)
            .ToListAsync(ct);

    public async Task<BankReconciliation?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
        => await Context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.BankStatement)
                .ThenInclude(s => s!.Entries)
            .Include(r => r.Items)
                .ThenInclude(i => i.BankStatementEntry)
            .Include(r => r.Items)
                .ThenInclude(i => i.Transaction)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> ExistsForStatementAsync(Guid bankStatementId, CancellationToken ct = default)
        => await Context.BankReconciliations
            .AnyAsync(r => r.BankStatementId == bankStatementId
                && r.Status != ReconciliationStatus.Cancelled
                && !r.IsDeleted, ct);
}

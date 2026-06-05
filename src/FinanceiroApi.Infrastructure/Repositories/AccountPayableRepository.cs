using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class AccountPayableRepository : RepositoryBase<AccountPayable>, IAccountPayableRepository
{
    public AccountPayableRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AccountPayable>> GetBySupplierAsync(Guid supplierId, CancellationToken ct = default)
        => await Context.AccountsPayable
            .Include(a => a.Supplier)
            .Include(a => a.CostCenter)
            .Where(a => a.SupplierId == supplierId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountPayable>> GetByStatusAsync(AccountPayableStatus status, CancellationToken ct = default)
        => await Context.AccountsPayable
            .Include(a => a.Supplier)
            .Include(a => a.CostCenter)
            .Where(a => a.Status == status)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountPayable>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await Context.AccountsPayable
            .Include(a => a.Supplier)
            .Where(a => a.DueDate >= from && a.DueDate <= to)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountPayable>> GetOverdueAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await Context.AccountsPayable
            .Include(a => a.Supplier)
            .Where(a => a.Status == AccountPayableStatus.Overdue ||
                        (a.DueDate < today &&
                         (a.Status == AccountPayableStatus.Pending || a.Status == AccountPayableStatus.PartiallyPaid)))
            .ToListAsync(ct);
    }

    public async Task<AccountPayable?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await Context.AccountsPayable
            .Include(a => a.Supplier)
            .Include(a => a.CostCenter)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
}

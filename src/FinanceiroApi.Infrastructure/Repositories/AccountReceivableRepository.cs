using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class AccountReceivableRepository : RepositoryBase<AccountReceivable>, IAccountReceivableRepository
{
    public AccountReceivableRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AccountReceivable>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => await Context.AccountsReceivable
            .Include(a => a.Customer)
            .Include(a => a.CostCenter)
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetByStatusAsync(AccountReceivableStatus status, CancellationToken ct = default)
        => await Context.AccountsReceivable
            .Include(a => a.Customer)
            .Include(a => a.CostCenter)
            .Where(a => a.Status == status)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await Context.AccountsReceivable
            .Include(a => a.Customer)
            .Where(a => a.DueDate >= from && a.DueDate <= to)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetOpenAsync(CancellationToken ct = default)
        => await Context.AccountsReceivable
            .Include(a => a.Customer)
            .Include(a => a.CostCenter)
            .Where(a => a.Status == AccountReceivableStatus.Pending ||
                        a.Status == AccountReceivableStatus.PartiallyReceived ||
                        a.Status == AccountReceivableStatus.Overdue)
            .ToListAsync(ct);

    public async Task<AccountReceivable?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await Context.AccountsReceivable
            .Include(a => a.Customer)
            .Include(a => a.CostCenter)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
}

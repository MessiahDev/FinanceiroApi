using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class BudgetRepository : RepositoryBase<Budget>, IBudgetRepository
{
    public BudgetRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Budget>> GetByYearAsync(int year, CancellationToken ct = default)
        => await Context.Budgets
            .Where(b => b.Year == year)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Budget>> GetByStatusAsync(BudgetStatus status, CancellationToken ct = default)
        => await Context.Budgets
            .Where(b => b.Status == status)
            .ToListAsync(ct);

    public async Task<Budget?> GetWithItemsAsync(Guid id, CancellationToken ct = default, bool tracking = true)
        => await Context.Budgets
            .AsTracking(tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
            .Include(b => b.Items)
            .ThenInclude(i => i.CostCenter)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
}

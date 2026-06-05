using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class CostCenterRepository : RepositoryBase<CostCenter>, ICostCenterRepository
{
    public CostCenterRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        => await Context.CostCenters.AnyAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<CostCenter?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Context.CostCenters.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<CostCenter>> GetActiveAsync(CancellationToken ct = default)
        => await Context.CostCenters
            .Include(c => c.Manager)
            .Include(c => c.Parent)
            .Where(c => c.Status == CostCenterStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CostCenter>> GetRootsAsync(CancellationToken ct = default)
        => await Context.CostCenters
            .Include(c => c.Children)
            .Where(c => c.ParentId == null && c.Status == CostCenterStatus.Active)
            .ToListAsync(ct);

    public async Task<CostCenter?> GetWithChildrenAsync(Guid id, CancellationToken ct = default)
        => await Context.CostCenters
            .Include(c => c.Children)
            .Include(c => c.Manager)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}

using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class SupplierRepository : RepositoryBase<Supplier>, ISupplierRepository
{
    public SupplierRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        var normalized = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        return await Context.Suppliers.AnyAsync(s => s.TaxId == normalized, ct);
    }

    public async Task<Supplier?> GetByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        var normalized = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        return await Context.Suppliers.FirstOrDefaultAsync(s => s.TaxId == normalized, ct);
    }

    public async Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken ct = default)
        => await Context.Suppliers
            .Where(s => s.Status == SupplierStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Supplier>> GetByStatusAsync(SupplierStatus status, CancellationToken ct = default)
        => await Context.Suppliers
            .Where(s => s.Status == status)
            .ToListAsync(ct);
}

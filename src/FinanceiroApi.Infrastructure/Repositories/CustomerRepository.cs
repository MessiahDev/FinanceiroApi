using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        var normalized = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        return await Context.Customers.AnyAsync(c => c.TaxId == normalized, ct);
    }

    public async Task<Customer?> GetByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        var normalized = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        return await Context.Customers.FirstOrDefaultAsync(c => c.TaxId == normalized, ct);
    }

    public async Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken ct = default)
        => await Context.Customers
            .Where(c => c.Status == CustomerStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken ct = default)
        => await Context.Customers
            .Where(c => c.Status == status)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetActivePagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = Context.Customers.Where(c => c.Status == CustomerStatus.Active);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}

using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class DepartmentRepository : RepositoryBase<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await Context.Departments
            .AnyAsync(d => d.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<IReadOnlyList<Department>> GetActiveAsync(CancellationToken ct = default)
    {
        return await Context.Departments
            .Include(d => d.Employees)
            .Where(d => d.IsActive)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Department> Items, int TotalCount)> GetActivePagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = Context.Departments
            .Include(d => d.Employees)
            .Where(d => d.IsActive);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}

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
}
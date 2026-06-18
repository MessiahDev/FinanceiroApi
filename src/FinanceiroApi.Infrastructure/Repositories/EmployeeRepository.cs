using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public sealed class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Email.Value == email, ct);

    public async Task<Employee?> GetByCpfAsync(string cpf, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Cpf.Value == cpf, ct);

    public async Task<bool> ExistsByCpfAsync(string cpf, CancellationToken ct = default)
        => await DbSet.AnyAsync(e => e.Cpf.Value == cpf, ct);

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(e => e.Status == EmployeeStatus.Active, ct);

    public async Task<IReadOnlyList<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(e => e.Status == status)
            .OrderBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(e => e.DepartmentId == departmentId && e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<PagedResult<Employee>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? departmentId = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Include(e => e.Department).AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (isActive.HasValue)
            query = query.Where(e => e.Status == (isActive.Value ? EmployeeStatus.Active : EmployeeStatus.Inactive));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                e.FirstName.Contains(search) ||
                e.LastName.Contains(search) ||
                e.Email.Value.Contains(search));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Employee>(items, total, page, pageSize);
    }

    public async Task<bool> HasActivePayrollAsync(Guid employeeId, CancellationToken ct = default)
        => await Context.Set<Payroll>()
            .AnyAsync(p => p.Items.Any(i => i.EmployeeId == employeeId)
                        && p.Status != PayrollStatus.Cancelled, ct);
}
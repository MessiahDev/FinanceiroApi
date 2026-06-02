using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public sealed class PayrollRepository : RepositoryBase<Payroll>, IPayrollRepository
{
    public PayrollRepository(AppDbContext context) : base(context) { }

    public async Task<Payroll?> GetByPeriodAsync(int year, int month, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Period.Start.Year == year
                                   && p.Period.Start.Month == month, ct);

    public async Task<bool> ExistsForPeriodAsync(int year, int month, CancellationToken ct = default)
        => await DbSet.AnyAsync(p => p.Period.Start.Year == year
                                  && p.Period.Start.Month == month
                                  && p.Status != PayrollStatus.Cancelled, ct);

    public async Task<Payroll?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payroll?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await GetWithItemsAsync(id, ct);

    public async Task<(IReadOnlyList<Payroll> Items, int Total)> GetHistoryPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Period.Start)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Payroll>> GetProcessedByPeriodAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(p => p.Status == PayrollStatus.Paid
                     && p.Period.Start >= from
                     && p.Period.End <= to)
            .ToListAsync(ct);

    public async Task<bool> ExistsByEmployeeAndMonthAsync(
        Guid employeeId,
        DateOnly referenceMonth,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            p => p.Items.Any(i => i.EmployeeId == employeeId)
              && p.Period.Start.Month == referenceMonth.Month
              && p.Period.Start.Year == referenceMonth.Year
              && p.Status != PayrollStatus.Cancelled, ct);

    public async Task<PagedResult<Payroll>> GetHistoryAsync(
        Guid? employeeId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        PayrollStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(p => p.Items.Any(i => i.EmployeeId == employeeId.Value));
        if (from.HasValue)
            query = query.Where(p => p.Period.Start >= from.Value);
        if (to.HasValue)
            query = query.Where(p => p.Period.End <= to.Value);
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Period.Start)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Payroll>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<Payroll>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(p => p.Items.Any(i => i.EmployeeId == employeeId))
            .OrderByDescending(p => p.Period.Start)
            .ToListAsync(ct);
}

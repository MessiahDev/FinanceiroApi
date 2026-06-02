using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public sealed class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Transaction>> GetByPeriodAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Transaction>> GetByPayrollAsync(
        Guid payrollId,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(t => t.PayrollId == payrollId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalByTypeAsync(
        TransactionType type,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(t => t.Type == type
                     && t.TransactionDate >= from
                     && t.TransactionDate <= to)
            .SumAsync(t => t.Amount.Amount, ct);

    public async Task<IReadOnlyList<Transaction>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
}

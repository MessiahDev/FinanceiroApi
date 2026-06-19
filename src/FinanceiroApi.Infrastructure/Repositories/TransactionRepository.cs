using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public sealed class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(
    Guid? employeeId,
    Guid? bankAccountId,
    TransactionType? type,
    TransactionStatus? status,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeId == employeeId.Value);

        if (bankAccountId.HasValue)
            query = query.Where(x => x.BankAccountId == bankAccountId.Value);

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.TransactionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Transaction>> GetByBankAccountAsync(
        Guid bankAccountId,
        CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(t => t.BankAccountId == bankAccountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);

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
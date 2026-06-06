using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class BankStatementRepository : RepositoryBase<BankStatement>, IBankStatementRepository
{
    public BankStatementRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<BankStatement>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default)
        => await Context.BankStatements
            .Include(s => s.BankAccount)
            .Where(s => s.BankAccountId == bankAccountId && !s.IsDeleted)
            .OrderByDescending(s => s.StatementDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BankStatement>> GetByPeriodAsync(
        Guid bankAccountId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await Context.BankStatements
            .Include(s => s.BankAccount)
            .Where(s => s.BankAccountId == bankAccountId
                && s.PeriodStart >= from
                && s.PeriodEnd <= to
                && !s.IsDeleted)
            .OrderByDescending(s => s.PeriodStart)
            .ToListAsync(ct);

    public async Task<BankStatement?> GetWithEntriesAsync(Guid id, CancellationToken ct = default)
        => await Context.BankStatements
            .Include(s => s.BankAccount)
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> ExistsForPeriodAsync(
        Guid bankAccountId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken ct = default)
        => await Context.BankStatements
            .AnyAsync(s => s.BankAccountId == bankAccountId
                && s.PeriodStart == periodStart
                && s.PeriodEnd == periodEnd
                && !s.IsDeleted, ct);
}

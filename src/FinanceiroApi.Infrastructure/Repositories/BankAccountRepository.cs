using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class BankAccountRepository : RepositoryBase<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<BankAccount>> GetActiveAsync(CancellationToken ct = default)
        => await Context.BankAccounts
            .Where(b => b.IsActive)
            .ToListAsync(ct);

    public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
        => await Context.BankAccounts
            .FirstOrDefaultAsync(b => b.AccountNumber == accountNumber, ct);
}

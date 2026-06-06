using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public class TaxPaymentRepository : RepositoryBase<TaxPayment>, ITaxPaymentRepository
{
    public TaxPaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TaxPayment>> GetByTaxEntryAsync(Guid taxEntryId, CancellationToken ct = default)
        => await Context.TaxPayments
            .Include(p => p.BankAccount)
            .Where(p => p.TaxEntryId == taxEntryId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxPayment>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default)
        => await Context.TaxPayments
            .Include(p => p.TaxEntry)
            .Where(p => p.BankAccountId == bankAccountId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaxPayment>> GetByPaymentDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await Context.TaxPayments
            .Include(p => p.TaxEntry)
            .Include(p => p.BankAccount)
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);

    public async Task<TaxPayment?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await Context.TaxPayments
            .Include(p => p.TaxEntry)
                .ThenInclude(e => e!.CostCenter)
            .Include(p => p.BankAccount)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}

using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Reports.GetFinancialSummary;

public record GetFinancialSummaryQuery(DateOnly PeriodStart, DateOnly PeriodEnd) : IRequest<FinancialSummaryResponse>;

public class GetFinancialSummaryQueryHandler : IRequestHandler<GetFinancialSummaryQuery, FinancialSummaryResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAccountPayableRepository _payableRepository;
    private readonly IAccountReceivableRepository _receivableRepository;
    private readonly ITaxPaymentRepository _taxPaymentRepository;
    private readonly ICacheService _cache;

    public GetFinancialSummaryQueryHandler(
        ITransactionRepository transactionRepository,
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IAccountPayableRepository payableRepository,
        IAccountReceivableRepository receivableRepository,
        ITaxPaymentRepository taxPaymentRepository,
        ICacheService cache)
    {
        _transactionRepository = transactionRepository;
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _payableRepository = payableRepository;
        _receivableRepository = receivableRepository;
        _taxPaymentRepository = taxPaymentRepository;
        _cache = cache;
    }

    public async Task<FinancialSummaryResponse> Handle(
        GetFinancialSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"financial-summary:{request.PeriodStart:yyyyMMdd}:{request.PeriodEnd:yyyyMMdd}";
        var cached = await _cache.GetAsync<FinancialSummaryResponse>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var transactions = await _transactionRepository.GetByPeriodAsync(
            request.PeriodStart, request.PeriodEnd, cancellationToken);

        var employeeCount = await _employeeRepository.CountActiveAsync(cancellationToken);

        var allPayables = await _payableRepository.GetAllWithDetailsAsync(cancellationToken);
        var allReceivables = await _receivableRepository.GetOpenAsync(cancellationToken);
        var allReceivablesReceived = await _receivableRepository.GetByStatusAsync(AccountReceivableStatus.Received, cancellationToken);
        var allReceivablesPartial = await _receivableRepository.GetByStatusAsync(AccountReceivableStatus.PartiallyReceived, cancellationToken);

        var allPayrolls = (await _payrollRepository.GetHistoryPagedAsync(1, int.MaxValue, cancellationToken)).Items;

        var taxPayments = await _taxPaymentRepository.GetByPaymentDateRangeAsync(
            request.PeriodStart, request.PeriodEnd, cancellationToken);

        var totalCredits = transactions
            .Where(t => t.Type == TransactionType.Credit)
            .Sum(t => t.Amount.Amount);

        var totalDebits = transactions
            .Where(t => t.Type == TransactionType.Debit)
            .Sum(t => t.Amount.Amount);

        var payrollsInPeriod = allPayrolls
            .Where(p => p.Status == PayrollStatus.Paid
                     && p.PaidAt.HasValue
                     && DateOnly.FromDateTime(p.PaidAt.Value) >= request.PeriodStart
                     && DateOnly.FromDateTime(p.PaidAt.Value) <= request.PeriodEnd)
            .ToList();

        var totalPayroll = payrollsInPeriod.Sum(p => p.TotalNet.Amount);

        var totalPaid = allPayables
            .Where(p => (p.Status == AccountPayableStatus.Paid || p.Status == AccountPayableStatus.PartiallyPaid)
                     && p.PaymentDate.HasValue
                     && p.PaymentDate.Value >= request.PeriodStart
                     && p.PaymentDate.Value <= request.PeriodEnd)
            .Sum(p => p.PaidAmount.Amount);

        var receivedAccounts = allReceivablesReceived.Concat(allReceivablesPartial);
        var totalReceived = receivedAccounts
            .Where(r => r.ReceiptDate.HasValue
                     && r.ReceiptDate.Value >= request.PeriodStart
                     && r.ReceiptDate.Value <= request.PeriodEnd)
            .Sum(r => r.ReceivedAmount.Amount);

        var totalTaxesPaid = taxPayments.Sum(t => t.TotalPaid.Amount);

        var pendingPayables = allPayables
            .Where(p => p.Status == AccountPayableStatus.Pending || p.Status == AccountPayableStatus.PartiallyPaid || p.Status == AccountPayableStatus.Overdue)
            .Sum(p => p.RemainingAmount.Amount);

        var pendingReceivables = allReceivables
            .Where(r => r.Status == AccountReceivableStatus.Pending || r.Status == AccountReceivableStatus.PartiallyReceived || r.Status == AccountReceivableStatus.Overdue)
            .Sum(r => r.RemainingAmount.Amount);

        var breakdown = transactions
            .GroupBy(t => new { Category = t.Category.ToString(), Type = t.Type.ToString() })
            .Select(g => new CategoryBreakdown(
                g.Key.Category,
                g.Key.Type,
                g.Sum(t => t.Amount.Amount),
                g.Count()))
            .OrderByDescending(b => b.Total)
            .ToList();

        var monthlyTrend = transactions
            .GroupBy(t => $"{t.TransactionDate.Year:D4}-{t.TransactionDate.Month:D2}")
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyTrend(
                g.Key,
                g.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Amount.Amount),
                g.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Amount.Amount)))
            .ToList();

        var summary = new FinancialSummaryResponse(
            From: request.PeriodStart,
            To: request.PeriodEnd,
            TotalCredits: totalCredits,
            TotalDebits: totalDebits,
            NetBalance: totalCredits - totalDebits,
            PayrollsProcessed: payrollsInPeriod.Count,
            TotalPayroll: totalPayroll,
            ActiveEmployees: employeeCount,
            TotalPaid: totalPaid,
            TotalReceived: totalReceived,
            TotalTaxesPaid: totalTaxesPaid,
            PendingPayables: pendingPayables,
            PendingReceivables: pendingReceivables,
            Breakdown: breakdown,
            MonthlyTrend: monthlyTrend);

        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(10), cancellationToken);
        return summary;
    }
}
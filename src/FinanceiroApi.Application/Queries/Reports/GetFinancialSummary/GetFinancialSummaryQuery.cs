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
    private readonly ICacheService _cache;

    public GetFinancialSummaryQueryHandler(
        ITransactionRepository transactionRepository,
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        ICacheService cache)
    {
        _transactionRepository = transactionRepository;
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _cache = cache;
    }

    public async Task<FinancialSummaryResponse> Handle(
        GetFinancialSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"financial-summary:{request.PeriodStart:yyyyMMdd}:{request.PeriodEnd:yyyyMMdd}";
        var cached = await _cache.GetAsync<FinancialSummaryResponse>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var transactions = await _transactionRepository.GetByPeriodAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);
        var payrolls = await _payrollRepository.GetProcessedByPeriodAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);
        var employeeCount = await _employeeRepository.CountActiveAsync(cancellationToken);

        var totalCredits = transactions.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Amount.Amount);
        var totalDebits = transactions.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Amount.Amount);
        var totalPayroll = payrolls.Sum(p => p.TotalNet.Amount);

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
            PayrollsProcessed: payrolls.Count,
            TotalPayroll: totalPayroll,
            ActiveEmployees: employeeCount,
            Breakdown: breakdown,
            MonthlyTrend: monthlyTrend);

        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(10), cancellationToken);
        return summary;
    }
}

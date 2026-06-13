using System.Data;
using Dapper;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Application.DTOs.Response;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceiroApi.Infrastructure.Reports;

public sealed class FinancialReportService : IFinancialReportService
{
    private readonly string _connectionString;
    private readonly ILogger<FinancialReportService> _logger;

    public FinancialReportService(IConfiguration configuration, ILogger<FinancialReportService> logger)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                SUM(CASE WHEN t.Type = 'Credit' THEN t.Amount ELSE 0 END)  AS TotalCredits,
                SUM(CASE WHEN t.Type = 'Debit'  THEN t.Amount ELSE 0 END)  AS TotalDebits,
                COUNT(DISTINCT p.Id)                                         AS PayrollsProcessed,
                SUM(p.NetSalaryAmount)                                       AS TotalPayroll,
                COUNT(DISTINCT e.Id)                                         AS ActiveEmployees
            FROM Transactions t
            LEFT JOIN Payrolls p ON p.Id = t.PayrollId
                AND p.Status = 'Processed'
                AND p.ReferenceMonth BETWEEN @From AND @To
            LEFT JOIN Employees e ON e.IsActive = 1
            WHERE CAST(t.OccurredAt AS DATE) BETWEEN @From AND @To
            """;

        const string breakdownSql = """
            SELECT
                t.Category,
                t.Type,
                SUM(t.Amount) AS Total,
                COUNT(*)      AS Count
            FROM Transactions t
            WHERE CAST(t.OccurredAt AS DATE) BETWEEN @From AND @To
            GROUP BY t.Category, t.Type
            ORDER BY Total DESC
            """;

        const string monthlyTrendSql = """
            SELECT
                FORMAT(t.OccurredAt, 'yyyy-MM')                              AS Month,
                SUM(CASE WHEN t.Type = 'Credit' THEN t.Amount ELSE 0 END)   AS Credits,
                SUM(CASE WHEN t.Type = 'Debit'  THEN t.Amount ELSE 0 END)   AS Debits
            FROM Transactions t
            WHERE CAST(t.OccurredAt AS DATE) BETWEEN @From AND @To
            GROUP BY FORMAT(t.OccurredAt, 'yyyy-MM')
            ORDER BY Month
            """;

        var parameters = new { From = from.ToString("yyyy-MM-dd"), To = to.ToString("yyyy-MM-dd") };

        using var connection = CreateConnection();

        var summary = await connection.QueryFirstOrDefaultAsync<FinancialSummaryRow>(sql, parameters);
        var breakdown = (await connection.QueryAsync<CategoryBreakdownRow>(breakdownSql, parameters)).ToList();
        var trend = (await connection.QueryAsync<MonthlyTrendRow>(monthlyTrendSql, parameters)).ToList();

        _logger.LogInformation("Financial summary generated for period {From} to {To}", from, to);

        return new FinancialSummaryResponse(
            From: from,
            To: to,
            TotalCredits: summary?.TotalCredits ?? 0,
            TotalDebits: summary?.TotalDebits ?? 0,
            NetBalance: (summary?.TotalCredits ?? 0) - (summary?.TotalDebits ?? 0),
            PayrollsProcessed: summary?.PayrollsProcessed ?? 0,
            TotalPayroll: summary?.TotalPayroll ?? 0,
            ActiveEmployees: summary?.ActiveEmployees ?? 0,
            TotalPaid: 0m,
            TotalReceived: 0m,
            TotalTaxesPaid: 0m,
            PendingPayables: 0m,
            PendingReceivables: 0m,
            Breakdown: breakdown.Select(b => new CategoryBreakdown(b.Category, b.Type, b.Total, b.Count)).ToList(),
            MonthlyTrend: trend.Select(t => new MonthlyTrend(t.Month, t.Credits, t.Debits)).ToList());
    }

    private record FinancialSummaryRow(
        decimal TotalCredits,
        decimal TotalDebits,
        int PayrollsProcessed,
        decimal TotalPayroll,
        int ActiveEmployees);

    private record CategoryBreakdownRow(string Category, string Type, decimal Total, int Count);
    private record MonthlyTrendRow(string Month, decimal Credits, decimal Debits);
}

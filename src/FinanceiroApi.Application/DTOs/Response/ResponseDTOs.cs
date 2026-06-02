namespace FinanceiroApi.Application.DTOs.Response;

public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Cpf,
    string? Position,
    Guid DepartmentId,
    string DepartmentName,
    decimal Salary,
    string Currency,
    string Status,
    string ContractType,
    DateOnly HireDate,
    DateOnly? TerminationDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record EmployeeSummaryResponse(
    Guid Id,
    string FullName,
    string? Position,
    Guid DepartmentId,
    string DepartmentName,
    decimal Salary,
    string Status);

public record DepartmentResponse(
    Guid Id,
    string Name,
    string? Description,
    string CostCenter,
    bool IsActive,
    int EmployeeCount);

public record PayrollResponse(
    Guid Id,
    int Month,
    int Year,
    string Period,
    string Status,
    decimal TotalGross,
    decimal TotalDiscounts,
    decimal TotalNet,
    int EmployeeCount,
    DateTime? ProcessedAt,
    DateTime? PaidAt,
    DateTime CreatedAt);

public record PayrollDetailResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    decimal GrossSalary,
    decimal InssDiscount,
    decimal IrpfDiscount,
    decimal OtherDiscounts,
    decimal NetSalary);

public record TransactionResponse(
    Guid Id,
    string Description,
    decimal Amount,
    string Currency,
    string Type,
    string Category,
    string Status,
    DateOnly TransactionDate,
    Guid? EmployeeId,
    Guid? PayrollId,
    string? ReferenceNumber,
    DateTime CreatedAt);

public record FinancialSummaryResponse(
    DateOnly From,
    DateOnly To,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal NetBalance,
    int PayrollsProcessed,
    decimal TotalPayroll,
    int ActiveEmployees,
    IReadOnlyList<CategoryBreakdown> Breakdown,
    IReadOnlyList<MonthlyTrend> MonthlyTrend);

public record CategoryBreakdown(
    string Category,
    string Type,
    decimal Total,
    int Count);

public record MonthlyTrend(
    string Month,
    decimal Credits,
    decimal Debits)
{
    public decimal NetBalance => Credits - Debits;
}

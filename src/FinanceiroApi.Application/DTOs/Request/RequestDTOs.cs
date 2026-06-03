using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.DTOs.Request;

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Cpf,
    Position? Position,
    Guid DepartmentId,
    decimal Salary,
    ContractType ContractType,
    DateOnly? HireDate = null);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    Position? Position,
    Guid DepartmentId);

public record UpdateSalaryRequest(
    decimal NewSalary,
    string Reason);

public record ProcessPayrollRequest(
    int Month,
    int Year,
    List<Guid> EmployeeIds);

public record CreateTransactionRequest(
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateOnly? TransactionDate = null,
    Guid? EmployeeId = null,
    Guid? PayrollId = null,
    string? ReferenceNumber = null);

public record PagedRequest(
    int Page = 1,
    int PageSize = 20);

public record GetEmployeesByDepartmentRequest(
    Guid DepartmentId,
    int Page = 1,
    int PageSize = 20) : PagedRequest(Page, PageSize);

public record GetFinancialSummaryRequest(
    DateOnly From,
    DateOnly To);

public record CreateDepartmentRequest(
    string Name,
    string CostCenter,
    string? Description = null);

public record UpdateDepartmentRequest(
    string Name,
    string CostCenter,
    string? Description = null);
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

public record CreateCustomerRequest(
    string Name,
    string TaxId,
    PersonType PersonType,
    string Email,
    string? Phone,
    string? ContactName,
    decimal CreditLimit = 0);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string? Phone,
    string? ContactName);

public record UpdateCreditLimitRequest(decimal CreditLimit);

public record CreateSupplierRequest(
    string Name,
    string TaxId,
    PersonType PersonType,
    string Email,
    string? Phone,
    string? ContactName);

public record UpdateSupplierRequest(
    string Name,
    string Email,
    string? Phone,
    string? ContactName);

public record UpdateSupplierBankingRequest(
    string BankName,
    string BankAgency,
    string BankAccount,
    string? PixKey);

public record CreateAccountPayableRequest(
    Guid SupplierId,
    string Description,
    decimal TotalAmount,
    DateOnly DueDate,
    Guid? CostCenterId,
    string? InvoiceNumber,
    string? Notes);

public record PayAccountPayableRequest(
    decimal Amount,
    DateOnly PaymentDate,
    Guid BankAccountId);

public record CreateAccountReceivableRequest(
    Guid CustomerId,
    string Description,
    decimal TotalAmount,
    DateOnly DueDate,
    Guid? CostCenterId,
    string? InvoiceNumber,
    string? Notes);

public record ReceivePaymentRequest(
    decimal Amount,
    DateOnly ReceiptDate,
    Guid BankAccountId);

public record CreateBankAccountRequest(
    string BankName,
    string BankCode,
    string Agency,
    string AccountNumber,
    BankAccountType AccountType,
    decimal InitialBalance = 0,
    string? PixKey = null,
    string? Description = null);

public record TransferBetweenAccountsRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description);

public record CreateCostCenterRequest(
    string Code,
    string Name,
    decimal AnnualBudget,
    Guid? ParentId,
    Guid? ManagerId,
    string? Description);

public record UpdateCostCenterRequest(
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId);

public record CreateBudgetRequest(
    int Year,
    string Name,
    string? Description);

public record UpdateBudgetRequest(
    string Name,
    string? Description);

public record AddBudgetItemRequest(
    Guid CostCenterId,
    string Category,
    decimal PlannedAmount);

public record BlockSupplierRequest(
    string Reason);

public record ApproveBudgetRequest(
    Guid ApprovedBy);

public record CancelTransactionRequest(
    string Reason);
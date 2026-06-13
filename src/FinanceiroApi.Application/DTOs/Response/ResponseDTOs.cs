using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.DTOs.Response;

public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Cpf,
    Position? Position,
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
    Position? Position,
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

public record PayrollItemResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    decimal GrossSalary,
    decimal InssDiscount,
    decimal IrpfDiscount,
    decimal OtherDiscounts,
    decimal NetSalary);

public record PayrollDetailResponse(
    Guid Id,
    int Month,
    int Year,
    string Period,
    string Status,
    decimal TotalGross,
    decimal TotalDiscounts,
    decimal TotalNet,
    string? Notes,
    DateTime? ProcessedAt,
    DateTime? PaidAt,
    DateTime CreatedAt,
    IReadOnlyList<PayrollItemResponse> Items);

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
    decimal TotalPaid,
    decimal TotalReceived,
    decimal TotalTaxesPaid,
    decimal PendingPayables,
    decimal PendingReceivables,
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

public record CustomerResponse(
    Guid Id,
    string Name,
    string TaxId,
    string PersonType,
    string Email,
    string? Phone,
    string? ContactName,
    string Status,
    decimal CreditLimit,
    string Currency,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CustomerSummaryResponse(
    Guid Id,
    string Name,
    string TaxId,
    string PersonType,
    string Status,
    decimal CreditLimit);

public record SupplierResponse(
    Guid Id,
    string Name,
    string TaxId,
    string PersonType,
    string Email,
    string? Phone,
    string? ContactName,
    string Status,
    string? BankName,
    string? BankAgency,
    string? BankAccount,
    string? PixKey,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SupplierSummaryResponse(
    Guid Id,
    string Name,
    string TaxId,
    string PersonType,
    string Status);

public record AccountPayableResponse(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    Guid? CostCenterId,
    string? CostCenterName,
    string Description,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    string Currency,
    DateOnly DueDate,
    DateOnly? PaymentDate,
    string Status,
    string? InvoiceNumber,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AccountReceivableResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? CostCenterId,
    string? CostCenterName,
    string Description,
    decimal TotalAmount,
    decimal ReceivedAmount,
    decimal RemainingAmount,
    string Currency,
    DateOnly DueDate,
    DateOnly? ReceiptDate,
    string Status,
    string? InvoiceNumber,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BankAccountResponse(
    Guid Id,
    string BankName,
    string BankCode,
    string Agency,
    string AccountNumber,
    string AccountType,
    string? PixKey,
    decimal Balance,
    string Currency,
    bool IsActive,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CostCenterResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentId,
    string? ParentName,
    decimal AnnualBudget,
    string Currency,
    string Status,
    Guid? ManagerId,
    string? ManagerName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BudgetResponse(
    Guid Id,
    int Year,
    string Name,
    string? Description,
    string Status,
    decimal TotalPlanned,
    decimal TotalRealized,
    decimal Variance,
    string Currency,
    DateTime? ApprovedAt,
    Guid? ApprovedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<BudgetItemResponse> Items);

public record BudgetSummaryResponse(
    Guid Id,
    int Year,
    string Name,
    string Status,
    decimal TotalPlanned,
    decimal TotalRealized,
    decimal Variance,
    DateTime? ApprovedAt);

public record BudgetItemResponse(
    Guid Id,
    Guid CostCenterId,
    string CostCenterName,
    string Category,
    decimal PlannedAmount,
    decimal RealizedAmount,
    decimal Variance,
    bool IsOverBudget);

public record ChartOfAccountResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    AccountType AccountType,
    string AccountTypeName,
    AccountNature AccountNature,
    string AccountNatureName,
    bool AcceptsEntries,
    bool IsActive,
    Guid? ParentAccountId,
    string? ParentAccountCode,
    string? ParentAccountName,
    IEnumerable<ChartOfAccountResponse>? ChildAccounts,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ChartOfAccountSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    AccountType AccountType,
    AccountNature AccountNature,
    bool AcceptsEntries,
    bool IsActive
);

public record JournalEntryResponse(
    Guid Id,
    string EntryNumber,
    string Description,
    DateTime EntryDate,
    JournalEntryStatus Status,
    string StatusName,
    JournalEntryType EntryType,
    string EntryTypeName,
    string? ReferenceDocument,
    string? ReferenceDocumentType,
    Guid? ReferenceDocumentId,
    Guid AccountingPeriodId,
    string AccountingPeriodName,
    decimal TotalDebits,
    decimal TotalCredits,
    bool IsBalanced,
    IEnumerable<JournalEntryLineResponse> Lines,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record JournalEntryLineResponse(
    Guid Id,
    Guid ChartOfAccountId,
    string AccountCode,
    string AccountName,
    DebitCredit DebitCredit,
    string DebitCreditName,
    decimal Amount,
    string? Description,
    int LineOrder
);

public record JournalEntrySummaryResponse(
    Guid Id,
    string EntryNumber,
    string Description,
    DateTime EntryDate,
    JournalEntryStatus Status,
    string StatusName,
    JournalEntryType EntryType,
    decimal TotalAmount
);

public record AccountingPeriodResponse(
    Guid Id,
    string Name,
    int Year,
    int Month,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    AccountingPeriodStatus Status,
    string StatusName,
    int TotalEntries,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record LedgerEntryResponse(
    DateTime EntryDate,
    string EntryNumber,
    string Description,
    decimal? Debit,
    decimal? Credit,
    decimal Balance
);

public record AccountLedgerResponse(
    Guid ChartOfAccountId,
    string AccountCode,
    string AccountName,
    AccountNature AccountNature,
    DateTime From,
    DateTime To,
    decimal OpeningBalance,
    decimal TotalDebits,
    decimal TotalCredits,
    decimal ClosingBalance,
    IEnumerable<LedgerEntryResponse> Entries
);

public record TrialBalanceLineResponse(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    decimal TotalDebits,
    decimal TotalCredits,
    decimal Balance,
    AccountNature BalanceNature
);

public record TrialBalanceResponse(
    Guid AccountingPeriodId,
    string PeriodName,
    DateTime GeneratedAt,
    decimal TotalDebits,
    decimal TotalCredits,
    IEnumerable<TrialBalanceLineResponse> Lines
);
public record TaxEntryResponse(
    Guid Id,
    string TaxType,
    string Description,
    decimal BaseAmount,
    decimal Rate,
    decimal TaxAmount,
    string Currency,
    DateOnly Competence,
    DateOnly DueDate,
    string Status,
    string? ReferenceDocument,
    Guid? ReferenceDocumentId,
    Guid? CostCenterId,
    string? CostCenterName,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TaxPaymentResponse> Payments);

public record TaxEntrySummaryResponse(
    Guid Id,
    string TaxType,
    string Description,
    decimal TaxAmount,
    string Currency,
    DateOnly Competence,
    DateOnly DueDate,
    string Status);

public record TaxPaymentResponse(
    Guid Id,
    Guid TaxEntryId,
    string TaxType,
    Guid BankAccountId,
    string BankAccountName,
    decimal Amount,
    decimal Fine,
    decimal Interest,
    decimal TotalPaid,
    string Currency,
    DateOnly PaymentDate,
    string? DarfNumber,
    string? ReceiptCode,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BankStatementResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountName,
    DateOnly StatementDate,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal OpeningBalance,
    decimal ClosingBalance,
    string Currency,
    string Status,
    int TotalEntries,
    decimal TotalCredits,
    decimal TotalDebits,
    string? FileName,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<BankStatementEntryResponse> Entries);

public record BankStatementSummaryResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal OpeningBalance,
    decimal ClosingBalance,
    string Status,
    int TotalEntries);

public record BankStatementEntryResponse(
    Guid Id,
    DateOnly Date,
    string Description,
    decimal Amount,
    string Currency,
    string EntryType,
    string? DocumentNumber,
    bool IsReconciled);

public record BankReconciliationResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountName,
    Guid BankStatementId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal StatementOpeningBalance,
    decimal StatementClosingBalance,
    decimal SystemBalance,
    decimal Difference,
    bool IsBalanced,
    string Status,
    int TotalItems,
    int MatchedItems,
    int UnmatchedItems,
    DateTime? CompletedAt,
    Guid? CompletedBy,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<BankReconciliationItemResponse> Items);

public record BankReconciliationSummaryResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal Difference,
    bool IsBalanced,
    string Status,
    int TotalItems,
    int MatchedItems);

public record BankReconciliationItemResponse(
    Guid Id,
    Guid BankStatementEntryId,
    string EntryDescription,
    DateOnly EntryDate,
    decimal Amount,
    string Currency,
    string EntryType,
    Guid? TransactionId,
    string Status,
    string? Notes);

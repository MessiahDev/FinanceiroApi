using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Domain.Interfaces.Repositories;

public interface IEmployeeRepository : IRepositoryBase<Employee>
{
    Task<Employee?> GetByCpfAsync(string cpf, CancellationToken ct = default);
    Task<bool> ExistsByCpfAsync(string cpf, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task<bool> HasActivePayrollAsync(Guid employeeId, CancellationToken ct = default);

    Task<PagedResult<Employee>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? departmentId = null,
        bool? isActive = null,
        CancellationToken ct = default);
}

public interface IPayrollRepository : IRepositoryBase<Payroll>
{
    Task<Payroll?> GetByPeriodAsync(int year, int month, CancellationToken ct = default);
    Task<bool> ExistsForPeriodAsync(int year, int month, CancellationToken ct = default);

    Task<Payroll?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<Payroll?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Payroll>> GetProcessedByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<(IReadOnlyList<Payroll> Items, int Total)> GetHistoryPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

public interface ITransactionRepository : IRepositoryBase<Transaction>
{
    Task<IReadOnlyList<Transaction>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByPayrollAsync(Guid payrollId, CancellationToken ct = default);
    Task<decimal> GetTotalByTypeAsync(TransactionType type, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface IDepartmentRepository : IRepositoryBase<Department>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Department>> GetActiveAsync(CancellationToken ct = default);
}

public interface ICustomerRepository : IRepositoryBase<Customer>
{
    Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<Customer?> GetByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken ct = default);
}

public interface ISupplierRepository : IRepositoryBase<Supplier>
{
    Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<Supplier?> GetByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetByStatusAsync(SupplierStatus status, CancellationToken ct = default);
}

public interface IAccountPayableRepository : IRepositoryBase<AccountPayable>
{
    Task<IReadOnlyList<AccountPayable>> GetBySupplierAsync(Guid supplierId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountPayable>> GetByStatusAsync(AccountPayableStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<AccountPayable>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<AccountPayable>> GetOverdueAsync(CancellationToken ct = default);
    Task<AccountPayable?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IAccountReceivableRepository : IRepositoryBase<AccountReceivable>
{
    Task<IReadOnlyList<AccountReceivable>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetByStatusAsync(AccountReceivableStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetOpenAsync(CancellationToken ct = default);
    Task<AccountReceivable?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IBankAccountRepository : IRepositoryBase<BankAccount>
{
    Task<IReadOnlyList<BankAccount>> GetActiveAsync(CancellationToken ct = default);
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
}

public interface ICostCenterRepository : IRepositoryBase<CostCenter>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<CostCenter?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<CostCenter>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CostCenter>> GetRootsAsync(CancellationToken ct = default);
    Task<CostCenter?> GetWithChildrenAsync(Guid id, CancellationToken ct = default);
}

public interface IBudgetRepository : IRepositoryBase<Budget>
{
    Task<IReadOnlyList<Budget>> GetByYearAsync(int year, CancellationToken ct = default);
    Task<IReadOnlyList<Budget>> GetByStatusAsync(BudgetStatus status, CancellationToken ct = default);
    Task<Budget?> GetWithItemsAsync(Guid id, CancellationToken ct = default, bool tracking = true);
}

public interface IChartOfAccountRepository : IRepositoryBase<ChartOfAccount>
{
    Task<ChartOfAccount?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccount>> GetRootAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccount>> GetChildAccountsAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccount>> GetAccountsAcceptingEntriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public interface IJournalEntryRepository : IRepositoryBase<JournalEntry>
{
    Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByPeriodAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByAccountAsync(Guid chartOfAccountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByReferenceDocumentAsync(string referenceDocumentType, Guid referenceDocumentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetPostedEntriesAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default);
    Task<string> GetNextEntryNumberAsync(int year, CancellationToken cancellationToken = default);
}

public interface IAccountingPeriodRepository : IRepositoryBase<AccountingPeriod>
{
    Task<AccountingPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<AccountingPeriod?> GetCurrentOpenPeriodAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountingPeriod>> GetByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountingPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByYearMonthAsync(int year, int month, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public interface ITaxEntryRepository : IRepositoryBase<TaxEntry>
{
    Task<IReadOnlyList<TaxEntry>> GetByTaxTypeAsync(TaxType taxType, CancellationToken ct = default);
    Task<IReadOnlyList<TaxEntry>> GetByStatusAsync(TaxEntryStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<TaxEntry>> GetByCompetenceAsync(int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<TaxEntry>> GetByDueDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<TaxEntry>> GetOverdueAsync(CancellationToken ct = default);
    Task<TaxEntry?> GetWithPaymentsAsync(Guid id, CancellationToken ct = default);
}

public interface ITaxPaymentRepository : IRepositoryBase<TaxPayment>
{
    Task<IReadOnlyList<TaxPayment>> GetByTaxEntryAsync(Guid taxEntryId, CancellationToken ct = default);
    Task<IReadOnlyList<TaxPayment>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<TaxPayment>> GetByPaymentDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<TaxPayment?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IBankStatementRepository : IRepositoryBase<BankStatement>
{
    Task<IReadOnlyList<BankStatement>> GetAsync(Guid? bankAccountId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<IReadOnlyList<BankStatement>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<BankStatement>> GetByPeriodAsync(Guid bankAccountId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<BankStatement?> GetWithEntriesAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsForPeriodAsync(Guid bankAccountId, DateOnly periodStart, DateOnly periodEnd, CancellationToken ct = default);
}

public interface IBankReconciliationRepository : IRepositoryBase<BankReconciliation>
{
    Task<IReadOnlyList<BankReconciliation>> GetByBankAccountAsync(Guid bankAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<BankReconciliation>> GetAllDetailedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BankReconciliation>> GetByStatusAsync(ReconciliationStatus status, CancellationToken ct = default);
    Task<BankReconciliation?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsForStatementAsync(Guid bankStatementId, CancellationToken ct = default);
}

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

using System.Linq.Expressions;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Specifications.Base;

namespace FinanceiroApi.Domain.Specifications;

public sealed class ActiveEmployeeSpecification : Specification<Employee>
{
    public override Expression<Func<Employee, bool>> ToExpression() =>
        e => e.Status == EmployeeStatus.Active;
}

public sealed class EmployeeByDepartmentSpecification(Guid departmentId) : Specification<Employee>
{
    public override Expression<Func<Employee, bool>> ToExpression() =>
        e => e.DepartmentId == departmentId;
}

public sealed class PaidPayrollSpecification : Specification<Payroll>
{
    public override Expression<Func<Payroll, bool>> ToExpression() =>
        p => p.Status == PayrollStatus.Paid;
}

public sealed class PayrollByYearSpecification(int year) : Specification<Payroll>
{
    public override Expression<Func<Payroll, bool>> ToExpression() =>
        p => p.Period.Start.Year == year;
}

public sealed class PendingTransactionSpecification : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression() =>
        t => t.Status == TransactionStatus.Pending;
}

public sealed class TransactionByDateRangeSpecification(DateOnly from, DateOnly to) : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression() =>
        t => t.TransactionDate >= from && t.TransactionDate <= to;
}

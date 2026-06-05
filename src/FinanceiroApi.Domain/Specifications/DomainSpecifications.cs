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

public class ActiveChartOfAccountSpecification : Specification<ChartOfAccount>
{
    public override System.Linq.Expressions.Expression<Func<ChartOfAccount, bool>> ToExpression()
        => account => account.IsActive;
}

public class AccountAcceptsEntriesSpecification : Specification<ChartOfAccount>
{
    public override System.Linq.Expressions.Expression<Func<ChartOfAccount, bool>> ToExpression()
        => account => account.IsActive && account.AcceptsEntries;
}

public class ChartOfAccountByTypeSpecification : Specification<ChartOfAccount>
{
    private readonly AccountType _accountType;

    public ChartOfAccountByTypeSpecification(AccountType accountType)
        => _accountType = accountType;

    public override System.Linq.Expressions.Expression<Func<ChartOfAccount, bool>> ToExpression()
        => account => account.AccountType == _accountType;
}

public class PostedJournalEntrySpecification : Specification<JournalEntry>
{
    public override System.Linq.Expressions.Expression<Func<JournalEntry, bool>> ToExpression()
        => entry => entry.Status == JournalEntryStatus.Posted;
}

public class JournalEntryByPeriodSpecification : Specification<JournalEntry>
{
    private readonly Guid _accountingPeriodId;

    public JournalEntryByPeriodSpecification(Guid accountingPeriodId)
        => _accountingPeriodId = accountingPeriodId;

    public override System.Linq.Expressions.Expression<Func<JournalEntry, bool>> ToExpression()
        => entry => entry.AccountingPeriodId == _accountingPeriodId;
}

public class JournalEntryByDateRangeSpecification : Specification<JournalEntry>
{
    private readonly DateTime _from;
    private readonly DateTime _to;

    public JournalEntryByDateRangeSpecification(DateTime from, DateTime to)
    {
        _from = from;
        _to = to;
    }

    public override System.Linq.Expressions.Expression<Func<JournalEntry, bool>> ToExpression()
        => entry => entry.EntryDate >= _from && entry.EntryDate <= _to;
}

public class OpenAccountingPeriodSpecification : Specification<AccountingPeriod>
{
    public override System.Linq.Expressions.Expression<Func<AccountingPeriod, bool>> ToExpression()
        => period => period.Status == AccountingPeriodStatus.Open;
}

public class AccountingPeriodByYearSpecification : Specification<AccountingPeriod>
{
    private readonly int _year;

    public AccountingPeriodByYearSpecification(int year) => _year = year;

    public override System.Linq.Expressions.Expression<Func<AccountingPeriod, bool>> ToExpression()
        => period => period.Year == _year;
}

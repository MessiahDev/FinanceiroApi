using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Specifications;
using FinanceiroApi.Domain.Validators;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Tests.Validators;

public class EmployeeValidatorTests
{
    private static Employee CreateEmployee(decimal salary = 5000m) =>
        Employee.Create("João", "Silva", "529.982.247-25", "joao@empresa.com",
            salary, ContractType.CLT, Guid.NewGuid());

    [Fact]
    public void ValidateSalaryUpdate_ValidIncrease_ShouldNotThrow()
    {
        var emp = CreateEmployee(3000m);

        var ex = Record.Exception(() => EmployeeValidator.ValidateSalaryUpdate(emp, 6000m));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void ValidateSalaryUpdate_ZeroOrNegative_ShouldThrow(decimal salary)
    {
        var emp = CreateEmployee(3000m);

        Assert.Throws<BusinessRuleException>(() => EmployeeValidator.ValidateSalaryUpdate(emp, salary));
    }

    [Fact]
    public void ValidateSalaryUpdate_MoreThan200PercentIncrease_ShouldThrow()
    {
        var emp = CreateEmployee(1000m);

        Assert.Throws<BusinessRuleException>(() => EmployeeValidator.ValidateSalaryUpdate(emp, 3001m));
    }

    [Fact]
    public void ValidateTermination_ValidDate_ShouldNotThrow()
    {
        var emp = Employee.Create("Ana", "Costa", "529.982.247-25", "ana@emp.com",
            2000m, ContractType.CLT, Guid.NewGuid(),
            hireDate: new DateOnly(2020, 1, 1));

        var ex = Record.Exception(() =>
            EmployeeValidator.ValidateTermination(emp, new DateOnly(2024, 6, 1)));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateTermination_BeforeHireDate_ShouldThrow()
    {
        var emp = Employee.Create("Ana", "Costa", "529.982.247-25", "ana@emp.com",
            2000m, ContractType.CLT, Guid.NewGuid(),
            hireDate: new DateOnly(2022, 1, 1));

        Assert.Throws<BusinessRuleException>(() =>
            EmployeeValidator.ValidateTermination(emp, new DateOnly(2021, 12, 31)));
    }

    [Fact]
    public void ValidateTermination_TooFarInFuture_ShouldThrow()
    {
        var emp = Employee.Create("Ana", "Costa", "529.982.247-25", "ana@emp.com",
            2000m, ContractType.CLT, Guid.NewGuid(),
            hireDate: new DateOnly(2020, 1, 1));

        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(91);
        Assert.Throws<BusinessRuleException>(() =>
            EmployeeValidator.ValidateTermination(emp, futureDate));
    }
}

public class PayrollValidatorTests
{
    [Fact]
    public void ValidateNotDuplicate_NoDuplicate_ShouldNotThrow()
    {
        var ex = Record.Exception(() => PayrollValidator.ValidateNotDuplicate(false, 2024, 6));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateNotDuplicate_Duplicate_ShouldThrow()
    {
        Assert.Throws<BusinessRuleException>(() => PayrollValidator.ValidateNotDuplicate(true, 2024, 6));
    }

    [Fact]
    public void ValidatePeriodNotInFuture_CurrentMonth_ShouldNotThrow()
    {
        var now = DateTime.UtcNow;
        var ex = Record.Exception(() => PayrollValidator.ValidatePeriodNotInFuture(now.Year, now.Month));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidatePeriodNotInFuture_FuturePeriod_ShouldThrow()
    {
        var future = DateTime.UtcNow.AddMonths(1);
        Assert.Throws<BusinessRuleException>(() =>
            PayrollValidator.ValidatePeriodNotInFuture(future.Year, future.Month));
    }
}

public class TransactionValidatorTests
{
    [Fact]
    public void ValidateAmount_ValidAmount_ShouldNotThrow()
    {
        var ex = Record.Exception(() => TransactionValidator.ValidateAmount(100m));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateAmount_ZeroOrNegative_ShouldThrow(decimal amount)
    {
        Assert.Throws<BusinessRuleException>(() => TransactionValidator.ValidateAmount(amount));
    }

    [Fact]
    public void ValidateAmount_ExceedsLimit_ShouldThrow()
    {
        Assert.Throws<BusinessRuleException>(() => TransactionValidator.ValidateAmount(10_000_001m));
    }
}

public class JournalEntryValidatorTests
{
    [Fact]
    public void ValidateDoubleEntry_Balanced_ShouldNotThrow()
    {
        var lines = new List<(decimal, bool)> { (1000m, true), (1000m, false) };
        var ex = Record.Exception(() => JournalEntryValidator.ValidateDoubleEntry(lines));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateDoubleEntry_Empty_ShouldThrow()
    {
        Assert.Throws<DomainException>(() =>
            JournalEntryValidator.ValidateDoubleEntry([]));
    }

    [Fact]
    public void ValidateDoubleEntry_Unbalanced_ShouldThrow()
    {
        var lines = new List<(decimal, bool)> { (1000m, true), (500m, false) };
        Assert.Throws<UnbalancedJournalEntryException>(() =>
            JournalEntryValidator.ValidateDoubleEntry(lines));
    }

    [Fact]
    public void ValidateEntryDate_WithinPeriod_ShouldNotThrow()
    {
        var start = new DateTime(2024, 6, 1);
        var end = new DateTime(2024, 6, 30);
        var entryDate = new DateTime(2024, 6, 15);

        var ex = Record.Exception(() => JournalEntryValidator.ValidateEntryDate(entryDate, start, end));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateEntryDate_OutsidePeriod_ShouldThrow()
    {
        var start = new DateTime(2024, 6, 1);
        var end = new DateTime(2024, 6, 30);
        var entryDate = new DateTime(2024, 7, 1);

        Assert.Throws<DomainException>(() =>
            JournalEntryValidator.ValidateEntryDate(entryDate, start, end));
    }
}

public class AccountingPeriodValidatorTests
{
    [Fact]
    public void ValidateYearMonth_Valid_ShouldNotThrow()
    {
        var ex = Record.Exception(() => AccountingPeriodValidator.ValidateYearMonth(2024, 6));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(1999, 6)]
    [InlineData(2101, 6)]
    public void ValidateYearMonth_InvalidYear_ShouldThrow(int year, int month)
    {
        Assert.Throws<DomainException>(() => AccountingPeriodValidator.ValidateYearMonth(year, month));
    }

    [Theory]
    [InlineData(2024, 0)]
    [InlineData(2024, 13)]
    public void ValidateYearMonth_InvalidMonth_ShouldThrow(int year, int month)
    {
        Assert.Throws<DomainException>(() => AccountingPeriodValidator.ValidateYearMonth(year, month));
    }
}

public class DomainSpecificationsTests
{
    private static Employee CreateEmployee(EmployeeStatus status = EmployeeStatus.Active, Guid? deptId = null) =>
        Employee.Create("João", "Silva", "529.982.247-25", "joao@empresa.com",
            3000m, ContractType.CLT, deptId ?? Guid.NewGuid());

    [Fact]
    public void ActiveEmployeeSpecification_ActiveEmployee_ShouldBeSatisfied()
    {
        var emp = CreateEmployee();
        var spec = new ActiveEmployeeSpecification();

        Assert.True(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void ActiveEmployeeSpecification_InactiveEmployee_ShouldNotBeSatisfied()
    {
        var emp = CreateEmployee();
        emp.Deactivate();
        var spec = new ActiveEmployeeSpecification();

        Assert.False(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void EmployeeByDepartmentSpecification_CorrectDept_ShouldBeSatisfied()
    {
        var deptId = Guid.NewGuid();
        var emp = CreateEmployee(deptId: deptId);
        var spec = new EmployeeByDepartmentSpecification(deptId);

        Assert.True(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void EmployeeByDepartmentSpecification_WrongDept_ShouldNotBeSatisfied()
    {
        var emp = CreateEmployee(deptId: Guid.NewGuid());
        var spec = new EmployeeByDepartmentSpecification(Guid.NewGuid());

        Assert.False(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void PendingTransactionSpecification_PendingTransaction_ShouldBeSatisfied()
    {
        var tx = Transaction.Create(100m, TransactionType.Credit, TransactionCategory.Other, "Desc");
        var spec = new PendingTransactionSpecification();

        Assert.True(spec.IsSatisfiedBy(tx));
    }

    [Fact]
    public void PendingTransactionSpecification_ConfirmedTransaction_ShouldNotBeSatisfied()
    {
        var tx = Transaction.Create(100m, TransactionType.Credit, TransactionCategory.Other, "Desc");
        tx.Confirm();
        var spec = new PendingTransactionSpecification();

        Assert.False(spec.IsSatisfiedBy(tx));
    }

    [Fact]
    public void TransactionByDateRangeSpecification_WithinRange_ShouldBeSatisfied()
    {
        var date = new DateOnly(2024, 6, 15);
        var tx = Transaction.Create(100m, TransactionType.Credit, TransactionCategory.Other, "Desc", transactionDate: date);
        var spec = new TransactionByDateRangeSpecification(new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30));

        Assert.True(spec.IsSatisfiedBy(tx));
    }

    [Fact]
    public void OpenAccountingPeriodSpecification_OpenPeriod_ShouldBeSatisfied()
    {
        var period = AccountingPeriod.Create(2024, 6);
        var spec = new OpenAccountingPeriodSpecification();

        Assert.True(spec.IsSatisfiedBy(period));
    }

    [Fact]
    public void OpenAccountingPeriodSpecification_ClosedPeriod_ShouldNotBeSatisfied()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        var spec = new OpenAccountingPeriodSpecification();

        Assert.False(spec.IsSatisfiedBy(period));
    }

    [Fact]
    public void AndSpecification_BothSatisfied_ShouldBeSatisfied()
    {
        var deptId = Guid.NewGuid();
        var emp = CreateEmployee(deptId: deptId);
        var spec = new ActiveEmployeeSpecification().And(new EmployeeByDepartmentSpecification(deptId));

        Assert.True(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void AndSpecification_OneFails_ShouldNotBeSatisfied()
    {
        var emp = CreateEmployee();
        var spec = new ActiveEmployeeSpecification().And(new EmployeeByDepartmentSpecification(Guid.NewGuid()));

        Assert.False(spec.IsSatisfiedBy(emp));
    }

    [Fact]
    public void NotSpecification_ActiveEmployee_ShouldNotBeSatisfied()
    {
        var emp = CreateEmployee();
        var spec = new ActiveEmployeeSpecification().Not();

        Assert.False(spec.IsSatisfiedBy(emp));
    }
}

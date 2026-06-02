using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Validators;

public static class EmployeeValidator
{
    public static void ValidateSalaryUpdate(Employee employee, decimal newSalary)
    {
        if (newSalary <= 0)
            throw new BusinessRuleException("SalaryMustBePositive", "Salary must be greater than zero.");

        if (newSalary > employee.BaseSalary.Amount * 3)
            throw new BusinessRuleException("SalaryIncreaseLimit",
                "Salary increase cannot exceed 200% of the current salary in a single update.");
    }

    public static void ValidateTermination(Employee employee, DateOnly terminationDate)
    {
        if (terminationDate < employee.HireDate)
            throw new BusinessRuleException("TerminationBeforeHireDate",
                "Termination date cannot be before hire date.");

        if (terminationDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90))
            throw new BusinessRuleException("TerminationDateTooFar",
                "Termination date cannot be more than 90 days in the future.");
    }
}

public static class PayrollValidator
{
    public static void ValidateNotDuplicate(bool exists, int year, int month)
    {
        if (exists)
            throw new BusinessRuleException("DuplicatePayroll",
                $"A payroll for {month:D2}/{year} already exists.");
    }

    public static void ValidatePeriodNotInFuture(int year, int month)
    {
        var today = DateTime.UtcNow;
        if (year > today.Year || (year == today.Year && month > today.Month))
            throw new BusinessRuleException("FuturePayroll",
                "Cannot create a payroll for a future period.");
    }
}

public static class TransactionValidator
{
    public static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("InvalidTransactionAmount",
                "Transaction amount must be greater than zero.");

        if (amount > 10_000_000)
            throw new BusinessRuleException("TransactionAmountLimit",
                "Transaction amount exceeds the maximum allowed limit.");
    }
}

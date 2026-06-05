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

public static class ChartOfAccountValidator
{
    public static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("O código da conta é obrigatório.");

        if (code.Length > 20)
            throw new DomainException("O código da conta deve ter no máximo 20 caracteres.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[\d.]+$"))
            throw new DomainException("O código da conta deve conter apenas dígitos e pontos (ex: 1.1.01.001).");
    }

    public static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da conta é obrigatório.");

        if (name.Length > 150)
            throw new DomainException("O nome da conta deve ter no máximo 150 caracteres.");
    }
}

public static class JournalEntryValidator
{
    public static void ValidateDoubleEntry(IEnumerable<(decimal Amount, bool IsDebit)> lines)
    {
        var linesList = lines.ToList();

        if (!linesList.Any())
            throw new DomainException("O lançamento deve ter ao menos uma linha.");

        var debits = linesList.Where(l => l.IsDebit).Sum(l => l.Amount);
        var credits = linesList.Where(l => !l.IsDebit).Sum(l => l.Amount);

        if (debits != credits)
            throw new UnbalancedJournalEntryException(debits, credits);
    }

    public static void ValidateEntryDate(DateTime entryDate, DateTime periodStart, DateTime periodEnd)
    {
        if (entryDate < periodStart || entryDate > periodEnd)
            throw new DomainException(
                $"A data do lançamento ({entryDate:dd/MM/yyyy}) está fora do período contábil " +
                $"({periodStart:dd/MM/yyyy} a {periodEnd:dd/MM/yyyy}).");
    }
}

public static class AccountingPeriodValidator
{
    public static void ValidateYearMonth(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new DomainException("Ano do período contábil inválido (deve ser entre 2000 e 2100).");

        if (month < 1 || month > 12)
            throw new DomainException("Mês do período contábil inválido (deve ser entre 1 e 12).");
    }
}

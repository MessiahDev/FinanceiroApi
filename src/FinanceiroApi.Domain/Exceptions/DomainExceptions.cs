namespace FinanceiroApi.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"{entityName} with id '{id}' was not found.") { }

    public EntityNotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}

public class InvalidOperationDomainException : DomainException
{
    public InvalidOperationDomainException(string message) : base(message) { }
}

public class AccountingPeriodClosedException : DomainException
{
    public AccountingPeriodClosedException(string periodName)
        : base($"O período contábil '{periodName}' está fechado e não aceita lançamentos.") { }
}

public class AccountingPeriodLockedException : DomainException
{
    public AccountingPeriodLockedException(string periodName)
        : base($"O período contábil '{periodName}' está bloqueado.") { }
}

public class UnbalancedJournalEntryException : DomainException
{
    public UnbalancedJournalEntryException(decimal debits, decimal credits)
        : base($"Lançamento contábil desequilibrado. Débitos: {debits:C} | Créditos: {credits:C}") { }
}

public class AccountNotAcceptingEntriesException : DomainException
{
    public AccountNotAcceptingEntriesException(string accountCode, string accountName)
        : base($"A conta '{accountCode} - {accountName}' não aceita lançamentos diretos (conta sintética).") { }
}

public class DuplicateChartOfAccountCodeException : DomainException
{
    public DuplicateChartOfAccountCodeException(string code)
        : base($"Já existe uma conta com o código '{code}' no plano de contas.") { }
}

public class DuplicateAccountingPeriodException : DomainException
{
    public DuplicateAccountingPeriodException(int year, int month)
        : base($"Já existe um período contábil para {month:D2}/{year}.") { }
}

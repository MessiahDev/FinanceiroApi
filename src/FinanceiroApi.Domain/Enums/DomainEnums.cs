namespace FinanceiroApi.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Employee = 3
}

public enum EmployeeStatus
{
    Active = 1,
    Inactive = 2,
    OnLeave = 3,
    Terminated = 4
}

public enum Position
{
    Estagiario = 1,
    DesenvolvedorJunior = 2,
    DesenvolvedorPleno = 3,
    DesenvolvedorSenior = 4,
    LiderTecnico = 5,
    Gerente = 6,
    Diretor = 7,
    CEO = 8,

    Analista = 9,
    Coordenador = 10,
    Supervisor = 11,

    RecursosHumanos = 12,
    Contador = 13,
    Vendedor = 14,
    AtendimentoAoCliente = 15
}

public enum ContractType
{
    CLT = 1,
    PJ = 2,
    Internship = 3,
    Temporary = 4
}

public enum PayrollStatus
{
    Draft = 1,
    Processing = 2,
    Approved = 3,
    Paid = 4,
    Cancelled = 5
}

public enum TransactionType
{
    Credit = 1,
    Debit = 2
}

public enum TransactionCategory
{
    Salary = 1,
    Bonus = 2,
    Deduction = 3,
    Tax = 4,
    Benefit = 5,
    Reimbursement = 6,
    Other = 7
}

public enum TransactionStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3
}
public enum SupplierStatus
{
    Active = 1,
    Inactive = 2,
    Blocked = 3
}

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Blocked = 3
}

public enum PersonType
{
    Individual = 1,
    Company = 2
}

public enum AccountPayableStatus
{
    Pending = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum AccountReceivableStatus
{
    Pending = 1,
    PartiallyReceived = 2,
    Received = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum BankAccountType
{
    Checking = 1,
    Savings = 2,
    Payment = 3
}

public enum BudgetStatus
{
    Draft = 1,
    Approved = 2,
    Closed = 3,
    Cancelled = 4
}

public enum CostCenterStatus
{
    Active = 1,
    Inactive = 2
}

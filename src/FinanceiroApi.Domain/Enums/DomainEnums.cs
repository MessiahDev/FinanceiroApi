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

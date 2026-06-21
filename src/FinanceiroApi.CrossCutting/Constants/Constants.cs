namespace FinanceiroApi.CrossCutting.Constants;

public static class CacheKeys
{
    public static string Employee(Guid id) => $"employee:{id}";
    public static string EmployeeList => "employees:all";
    public static string EmployeesByDepartment(Guid deptId) => $"employees:dept:{deptId}";
    public static string Payroll(Guid id) => $"payroll:{id}";
    public static string PayrollHistory(Guid employeeId) => $"payroll:history:{employeeId}";
    public static string FinancialSummary(string from, string to) => $"report:summary:{from}:{to}";
    public const string EmployeePrefix = "employee:";
    public const string PayrollPrefix = "payroll:";
    public const string ReportPrefix = "report:";
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string ReadOnly = "ReadOnly";
}

public static class AppPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireManager = "RequireManager";
    public const string RequireEmployee = "RequireEmployee";
}

public static class AppMessages
{
    public const string NotFound = "Registro não encontrado.";
    public const string AlreadyExists = "Registro já existe.";
    public const string InvalidOperation = "Operação inválida.";
    public const string Unauthorized = "Acesso não autorizado.";
    public const string InternalError = "Erro interno. Tente novamente mais tarde.";
}

public static class DemoAccount
{
    public const string Email = "admin@financeiro.com";
}

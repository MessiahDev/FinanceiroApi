using System.Text.RegularExpressions;

namespace FinanceiroApi.CrossCutting.Helpers;

public static class CpfHelper
{
    public static bool IsValid(string cpf)
    {
        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

        int[] multipliers1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multipliers2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum = cpf[..9].Select((c, i) => (c - '0') * multipliers1[i]).Sum();
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        sum = cpf[..10].Select((c, i) => (c - '0') * multipliers2[i]).Sum();
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return cpf[9] - '0' == digit1 && cpf[10] - '0' == digit2;
    }

    public static string Format(string cpf)
    {
        cpf = Regex.Replace(cpf, @"[^\d]", "");
        return cpf.Length == 11
            ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
            : cpf;
    }

    public static string Strip(string cpf) => Regex.Replace(cpf, @"[^\d]", "");
}

public static class DateHelper
{
    public static DateOnly FirstDayOfMonth(int year, int month) => new(year, month, 1);
    public static DateOnly LastDayOfMonth(int year, int month) => new(year, month, DateTime.DaysInMonth(year, month));
    public static DateOnly CurrentMonth() => DateOnly.FromDateTime(DateTime.UtcNow);
    public static bool IsWeekend(DateOnly date) => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}

public static class MoneyHelper
{
    public static decimal CalculateInss(decimal grossSalary)
    {
        return grossSalary switch
        {
            <= 1412.00m => grossSalary * 0.075m,
            <= 2666.68m => grossSalary * 0.09m,
            <= 4000.03m => grossSalary * 0.12m,
            <= 7786.02m => grossSalary * 0.14m,
            _ => 908.86m // teto INSS
        };
    }

    public static decimal CalculateIrrf(decimal grossSalary, decimal inss)
    {
        var baseCalculo = grossSalary - inss;

        return baseCalculo switch
        {
            <= 2259.20m => 0,
            <= 2826.65m => baseCalculo * 0.075m - 169.44m,
            <= 3751.05m => baseCalculo * 0.15m - 381.44m,
            <= 4664.68m => baseCalculo * 0.225m - 662.77m,
            _ => baseCalculo * 0.275m - 896.00m
        };
    }

    public static decimal CalculateNetSalary(decimal grossSalary)
    {
        var inss = CalculateInss(grossSalary);
        var irrf = CalculateIrrf(grossSalary, inss);
        return grossSalary - inss - irrf;
    }
}

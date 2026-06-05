using System.Text.RegularExpressions;

namespace FinanceiroApi.CrossCutting.Helpers;

public static class CpfHelper
{
    public static bool IsValid(string cpf)
    {
        var digits = Strip(cpf);
        if (digits.Length != 11 || !digits.All(char.IsDigit)) return false;
        if (digits.Distinct().Count() == 1) return false;
        return ValidateDigit(digits, 9) && ValidateDigit(digits, 10);
    }

    private static bool ValidateDigit(string digits, int position)
    {
        var sum = 0;
        for (var i = 0; i < position; i++)
            sum += int.Parse(digits[i].ToString()) * (position + 1 - i);
        var remainder = sum % 11;
        var expected = remainder < 2 ? 0 : 11 - remainder;
        return int.Parse(digits[position].ToString()) == expected;
    }

    public static string Format(string cpf)
    {
        var digits = Strip(cpf);
        return digits.Length == 11
            ? $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}"
            : digits;
    }

    public static string Strip(string cpf) =>
        Regex.Replace(cpf ?? "", @"[^\d]", "");
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
            _ => 908.86m
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

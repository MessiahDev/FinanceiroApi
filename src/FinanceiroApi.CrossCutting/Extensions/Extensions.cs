using System.Text.Json;

namespace FinanceiroApi.CrossCutting.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);
    public static string ToSnakeCase(this string value)
        => string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())).ToLower();
}

public static class GuidExtensions
{
    public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;
    public static bool IsNotEmpty(this Guid guid) => guid != Guid.Empty;
}

public static class DecimalExtensions
{
    public static string ToBRL(this decimal value) => value.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
    public static decimal RoundTwo(this decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public static class DateOnlyExtensions
{
    public static string ToMonthYear(this DateOnly date)
        => date.ToString("MMMM/yyyy", new System.Globalization.CultureInfo("pt-BR"));
    public static bool IsCurrentMonth(this DateOnly date)
        => date.Year == DateTime.UtcNow.Year && date.Month == DateTime.UtcNow.Month;
    public static bool IsFuture(this DateOnly date) => date > DateOnly.FromDateTime(DateTime.UtcNow);
    public static bool IsPast(this DateOnly date) => date < DateOnly.FromDateTime(DateTime.UtcNow);
}

public static class ObjectExtensions
{
    public static string ToJson(this object obj)
        => JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
}

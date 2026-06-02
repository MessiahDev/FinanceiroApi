using FinanceiroApi.Domain.Exceptions;
namespace FinanceiroApi.Domain.ValueObjects;
public sealed class Cpf : IEquatable<Cpf>
{
    public string Value { get; }

    public Cpf(string value)
    {
        var digits = value?.Replace(".", "").Replace("-", "").Trim() ?? "";

        if (digits.Length != 11 || !digits.All(char.IsDigit))
            throw new DomainException($"'{value}' is not a valid CPF.");
        if (!IsValid(digits))
            throw new DomainException($"'{value}' is not a valid CPF.");

        Value = digits;
    }

    private static bool IsValid(string digits)
    {
        if (digits.Length != 11 || digits.Distinct().Count() == 1)
            return false;
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

    public string Formatted => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    public bool Equals(Cpf? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Cpf);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Formatted;
    public static implicit operator string(Cpf cpf) => cpf.Value;
}

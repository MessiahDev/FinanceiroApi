using FinanceiroApi.CrossCutting.Helpers;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.ValueObjects;

public sealed class Cpf : IEquatable<Cpf>
{
    public string Value { get; }

    public Cpf(string value)
    {
        var digits = CpfHelper.Strip(value ?? "");
        if (!CpfHelper.IsValid(digits))
            throw new DomainException($"'" + value + @"' is not a valid CPF.");
        Value = digits;
    }

    public string Formatted => CpfHelper.Format(Value);

    public bool Equals(Cpf? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Cpf);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Formatted;
    public static implicit operator string(Cpf cpf) => cpf.Value;
}

using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("user.name+tag@sub.domain.org")]
    public void Email_WithValidAddress_ShouldCreate(string address)
    {
        var email = new Email(address);
        email.Value.Should().Be(address.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@nodomain")]
    [InlineData("noat.domain")]
    public void Email_WithInvalidAddress_ShouldThrow(string address)
    {
        var act = () => new Email(address);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Email_Equality_ShouldBeValueBased()
    {
        var a = new Email("TEST@DOMAIN.COM");
        var b = new Email("test@domain.com");
        a.Should().Be(b);
    }

    [Fact]
    public void Email_WithNull_ShouldThrow()
    {
        var act = () => new Email(null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Email_ShouldNormalizeToLowercase()
    {
        var email = new Email("USER@DOMAIN.COM");
        email.Value.Should().Be("user@domain.com");
    }

    [Fact]
    public void Email_ImplicitConversion_ShouldReturnValue()
    {
        var email = new Email("user@domain.com");
        string value = email;
        value.Should().Be("user@domain.com");
    }
}

public class CpfTests
{
    [Fact]
    public void Cpf_WithValidDigits_ShouldCreate()
    {
        var cpf = new Cpf("52998224725");
        cpf.Value.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("1234567890A")]
    public void Cpf_WithInvalidFormat_ShouldThrow(string value)
    {
        var act = () => new Cpf(value);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void Cpf_WithAllSameDigits_ShouldThrow(string value)
    {
        var act = () => new Cpf(value);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cpf_WithInvalidCheckDigits_ShouldThrow()
    {
        var act = () => new Cpf("52998224724");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cpf_Equality_ShouldBeValueBased()
    {
        var a = new Cpf("52998224725");
        var b = new Cpf("52998224725");
        a.Should().Be(b);
    }

    [Fact]
    public void Cpf_WithFormattedInput_ShouldCreate()
    {
        var cpf = new Cpf("529.982.247-25");
        cpf.Value.Should().Be("52998224725");
    }

    [Fact]
    public void Cpf_Formatted_ShouldReturnMasked()
    {
        var cpf = new Cpf("52998224725");
        cpf.Formatted.Should().Be("529.982.247-25");
    }

    [Fact]
    public void Cpf_ImplicitConversion_ShouldReturnDigits()
    {
        var cpf = new Cpf("52998224725");
        string value = cpf;
        value.Should().Be("52998224725");
    }
}

public class MoneyTests
{
    [Fact]
    public void Money_WithPositiveValue_ShouldCreate()
    {
        var money = new Money(100.50m);
        money.Amount.Should().Be(100.50m);
    }

    [Fact]
    public void Money_WithNegativeValue_ShouldThrow()
    {
        var act = () => new Money(-1m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Money_Addition_ShouldReturnCorrectSum()
    {
        var a = new Money(100m);
        var b = new Money(50m);
        (a + b).Amount.Should().Be(150m);
    }

    [Fact]
    public void Money_Subtraction_BelowZero_ShouldThrow()
    {
        var a = new Money(50m);
        var b = new Money(100m);
        var act = () => _ = a - b;
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Money_Zero_ShouldHaveZeroAmount()
    {
        Money.Zero.Amount.Should().Be(0m);
    }

    [Fact]
    public void Money_Subtraction_EqualValues_ShouldReturnZero()
    {
        var a = new Money(100m);
        var b = new Money(100m);
        (a - b).Amount.Should().Be(0m);
    }

    [Fact]
    public void Money_DifferentCurrencies_ShouldThrowOnAdd()
    {
        var brl = new Money(100m, "BRL");
        var usd = new Money(100m, "USD");
        var act = () => _ = brl + usd;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Money_Multiply_ShouldReturnCorrectValue()
    {
        var money = new Money(100m);
        (money * 1.5m).Amount.Should().Be(150m);
    }

    [Fact]
    public void Money_ShouldRoundToTwoDecimals()
    {
        var money = new Money(100.999m);
        money.Amount.Should().Be(101.00m);
    }

    [Fact]
    public void Money_Equality_ShouldBeValueBased()
    {
        var a = new Money(100m, "BRL");
        var b = new Money(100m, "BRL");
        a.Should().Be(b);
    }

    [Fact]
    public void Money_WithZeroValue_ShouldCreate()
    {
        var act = () => new Money(0m);
        act.Should().NotThrow();
    }

    [Fact]
    public void Money_DefaultCurrency_ShouldBeBRL()
    {
        var money = new Money(100m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Money_IsGreaterThan_ShouldReturnCorrectly()
    {
        var a = new Money(100m);
        var b = new Money(50m);
        a.IsGreaterThan(b).Should().BeTrue();
        b.IsGreaterThan(a).Should().BeFalse();
    }
}
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
}

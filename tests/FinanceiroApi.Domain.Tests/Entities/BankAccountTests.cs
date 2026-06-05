using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class BankAccountTests
{
    private static BankAccount CreateValid() => BankAccount.Create(
        "Banco do Brasil", "001", "1234", "56789-0",
        BankAccountType.Checking, 1000m);

    [Fact]
    public void Create_WithValidData_ShouldCreateAccount()
    {
        var account = CreateValid();
        account.Should().NotBeNull();
        account.Balance.Amount.Should().Be(1000m);
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseBankAccountCreatedEvent()
    {
        var account = CreateValid();
        account.DomainEvents.Should().ContainSingle(e => e is BankAccountCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyBankName_ShouldThrow(string? name)
    {
        var act = () => BankAccount.Create(name!, "001", "1234", "56789-0", BankAccountType.Checking);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Credit_WithValidAmount_ShouldIncreaseBalance()
    {
        var account = CreateValid();
        account.Credit(new Money(500m), "deposito");
        account.Balance.Amount.Should().Be(1500m);
    }

    [Fact]
    public void Credit_WhenInactive_ShouldThrow()
    {
        var account = CreateValid();
        account.Deactivate();
        var act = () => account.Credit(new Money(100m), "deposito");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Debit_WithValidAmount_ShouldDecreaseBalance()
    {
        var account = CreateValid();
        account.Debit(new Money(500m), "saque");
        account.Balance.Amount.Should().Be(500m);
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ShouldThrow()
    {
        var account = CreateValid();
        var act = () => account.Debit(new Money(2000m), "saque");
        act.Should().Throw<DomainException>().WithMessage("*balance*");
    }

    [Fact]
    public void Debit_WhenInactive_ShouldThrow()
    {
        var account = CreateValid();
        account.Deactivate();
        var act = () => account.Debit(new Money(100m), "saque");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var account = CreateValid();
        account.Deactivate();
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var account = CreateValid();
        account.Deactivate();
        account.Activate();
        account.IsActive.Should().BeTrue();
    }
}

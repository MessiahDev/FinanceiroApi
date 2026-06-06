using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class ChartOfAccountTests
{
    private static ChartOfAccount CreateValid(string code = "1.1.01") =>
        ChartOfAccount.Create(code, "Caixa", null, AccountType.Asset, AccountNature.Debit, true);

    [Fact]
    public void Create_ValidData_ShouldReturnActiveAccount()
    {
        var account = CreateValid();

        Assert.Equal("1.1.01", account.Code);
        Assert.Equal("Caixa", account.Name);
        Assert.True(account.IsActive);
        Assert.True(account.AcceptsEntries);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyCode_ShouldThrowDomainException(string code)
    {
        Assert.Throws<DomainException>(() =>
            ChartOfAccount.Create(code, "Caixa", null, AccountType.Asset, AccountNature.Debit, true));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyName_ShouldThrowDomainException(string name)
    {
        Assert.Throws<DomainException>(() =>
            ChartOfAccount.Create("1.1.01", name, null, AccountType.Asset, AccountNature.Debit, true));
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var account = CreateValid();

        Assert.Single(account.DomainEvents);
    }

    [Fact]
    public void Update_ValidName_ShouldUpdateProperties()
    {
        var account = CreateValid();

        account.Update("Caixa Geral", "Conta de caixa principal", false);

        Assert.Equal("Caixa Geral", account.Name);
        Assert.Equal("Conta de caixa principal", account.Description);
        Assert.False(account.AcceptsEntries);
    }

    [Fact]
    public void Update_EmptyName_ShouldThrowDomainException()
    {
        var account = CreateValid();

        Assert.Throws<DomainException>(() => account.Update("", null, true));
    }

    [Fact]
    public void Deactivate_WithNoLines_ShouldSetIsActiveFalse()
    {
        var account = CreateValid();

        account.Deactivate();

        Assert.False(account.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldRaiseDomainEvent()
    {
        var account = CreateValid();
        account.ClearDomainEvents();

        account.Deactivate();

        Assert.Single(account.DomainEvents);
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveTrue()
    {
        var account = CreateValid();
        account.Deactivate();

        account.Reactivate();

        Assert.True(account.IsActive);
    }

    [Fact]
    public void Create_WithParentAccount_ShouldSetParentId()
    {
        var parentId = Guid.NewGuid();
        var account = ChartOfAccount.Create("1.1.01.001", "Sub-caixa", null, AccountType.Asset, AccountNature.Debit, true, parentId);

        Assert.Equal(parentId, account.ParentAccountId);
    }
}

using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class AccountingPeriodTests
{
    [Fact]
    public void Create_ValidYearAndMonth_ShouldReturnOpenPeriod()
    {
        var period = AccountingPeriod.Create(2024, 6);

        Assert.Equal(2024, period.Year);
        Assert.Equal(6, period.Month);
        Assert.Equal(AccountingPeriodStatus.Open, period.Status);
        Assert.NotNull(period.Period);
    }

    [Fact]
    public void Create_ShouldSetCorrectDateRange()
    {
        var period = AccountingPeriod.Create(2024, 2);

        Assert.Equal(new DateOnly(2024, 2, 1), period.Period.Start);
        Assert.Equal(new DateOnly(2024, 2, 29), period.Period.End);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Create_InvalidYear_ShouldThrowDomainException(int year)
    {
        Assert.Throws<DomainException>(() => AccountingPeriod.Create(year, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_InvalidMonth_ShouldThrowDomainException(int month)
    {
        Assert.Throws<DomainException>(() => AccountingPeriod.Create(2024, month));
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var period = AccountingPeriod.Create(2024, 6);

        Assert.Single(period.DomainEvents);
    }

    [Fact]
    public void Close_OpenPeriod_ShouldSetStatusToClosed()
    {
        var period = AccountingPeriod.Create(2024, 6);

        period.Close();

        Assert.Equal(AccountingPeriodStatus.Closed, period.Status);
    }

    [Fact]
    public void Close_AlreadyClosedPeriod_ShouldThrowDomainException()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();

        Assert.Throws<DomainException>(() => period.Close());
    }

    [Fact]
    public void Close_LockedPeriod_ShouldThrowDomainException()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        period.Lock();

        Assert.Throws<DomainException>(() => period.Close());
    }

    [Fact]
    public void Close_ShouldRaiseDomainEvent()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.ClearDomainEvents();

        period.Close();

        Assert.Single(period.DomainEvents);
    }

    [Fact]
    public void Lock_ClosedPeriod_ShouldSetStatusToLocked()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();

        period.Lock();

        Assert.Equal(AccountingPeriodStatus.Locked, period.Status);
    }

    [Fact]
    public void Lock_OpenPeriod_ShouldThrowDomainException()
    {
        var period = AccountingPeriod.Create(2024, 6);

        Assert.Throws<DomainException>(() => period.Lock());
    }

    [Fact]
    public void Lock_ShouldRaiseDomainEvent()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        period.ClearDomainEvents();

        period.Lock();

        Assert.Single(period.DomainEvents);
    }

    [Fact]
    public void Reopen_ClosedPeriod_ShouldSetStatusToOpen()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();

        period.Reopen();

        Assert.Equal(AccountingPeriodStatus.Open, period.Status);
    }

    [Fact]
    public void Reopen_OpenPeriod_ShouldThrowDomainException()
    {
        var period = AccountingPeriod.Create(2024, 6);

        Assert.Throws<DomainException>(() => period.Reopen());
    }

    [Fact]
    public void Reopen_LockedPeriod_ShouldThrowDomainException()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        period.Lock();

        Assert.Throws<DomainException>(() => period.Reopen());
    }

    [Fact]
    public void IsOpen_OpenPeriod_ShouldReturnTrue()
    {
        var period = AccountingPeriod.Create(2024, 6);

        Assert.True(period.IsOpen());
        Assert.True(period.AcceptsEntries());
    }

    [Fact]
    public void IsOpen_ClosedPeriod_ShouldReturnFalse()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();

        Assert.False(period.IsOpen());
        Assert.False(period.AcceptsEntries());
    }
}

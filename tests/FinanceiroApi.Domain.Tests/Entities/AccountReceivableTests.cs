using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class AccountReceivableTests
{
    private static AccountReceivable CreateValid() => AccountReceivable.Create(
        Guid.NewGuid(), "Venda de produto", 1000m,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var ar = CreateValid();
        ar.Should().NotBeNull();
        ar.Status.Should().Be(AccountReceivableStatus.Pending);
        ar.TotalAmount.Amount.Should().Be(1000m);
        ar.ReceivedAmount.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var ar = CreateValid();
        ar.DomainEvents.Should().ContainSingle(e => e is AccountReceivableCreatedEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidAmount_ShouldThrow(decimal amount)
    {
        var act = () => AccountReceivable.Create(Guid.NewGuid(), "desc", amount,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterReceipt_FullAmount_ShouldSetStatusToReceived()
    {
        var ar = CreateValid();
        ar.RegisterReceipt(1000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        ar.Status.Should().Be(AccountReceivableStatus.Received);
        ar.ReceivedAmount.Amount.Should().Be(1000m);
    }

    [Fact]
    public void RegisterReceipt_PartialAmount_ShouldSetStatusToPartiallyReceived()
    {
        var ar = CreateValid();
        ar.RegisterReceipt(500m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        ar.Status.Should().Be(AccountReceivableStatus.PartiallyReceived);
        ar.RemainingAmount.Amount.Should().Be(500m);
    }

    [Fact]
    public void RegisterReceipt_ExceedingAmount_ShouldThrow()
    {
        var ar = CreateValid();
        var act = () => ar.RegisterReceipt(1500m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterReceipt_WhenAlreadyReceived_ShouldThrow()
    {
        var ar = CreateValid();
        ar.RegisterReceipt(1000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        var act = () => ar.RegisterReceipt(100m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetStatusToCancelled()
    {
        var ar = CreateValid();
        ar.Cancel("erro");
        ar.Status.Should().Be(AccountReceivableStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenReceived_ShouldThrow()
    {
        var ar = CreateValid();
        ar.RegisterReceipt(1000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        var act = () => ar.Cancel("erro");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsOverdue_WhenPastDue_ShouldSetStatusToOverdue()
    {
        var ar = AccountReceivable.Create(Guid.NewGuid(), "venda", 1000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        ar.MarkAsOverdue();
        ar.Status.Should().Be(AccountReceivableStatus.Overdue);
    }
}

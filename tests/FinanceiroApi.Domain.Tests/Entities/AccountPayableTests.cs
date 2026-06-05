using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class AccountPayableTests
{
    private static AccountPayable CreateValid() => AccountPayable.Create(
        Guid.NewGuid(), "Compra de material", 2000m,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var ap = CreateValid();
        ap.Should().NotBeNull();
        ap.Status.Should().Be(AccountPayableStatus.Pending);
        ap.TotalAmount.Amount.Should().Be(2000m);
        ap.PaidAmount.Amount.Should().Be(0m);
    }

    [Fact]
    public void RegisterPayment_FullAmount_ShouldSetStatusToPaid()
    {
        var ap = CreateValid();
        ap.RegisterPayment(2000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        ap.Status.Should().Be(AccountPayableStatus.Paid);
    }

    [Fact]
    public void RegisterPayment_PartialAmount_ShouldSetStatusToPartiallyPaid()
    {
        var ap = CreateValid();
        ap.RegisterPayment(1000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        ap.Status.Should().Be(AccountPayableStatus.PartiallyPaid);
        ap.RemainingAmount.Amount.Should().Be(1000m);
    }

    [Fact]
    public void RegisterPayment_ExceedingAmount_ShouldThrow()
    {
        var ap = CreateValid();
        var act = () => ap.RegisterPayment(3000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterPayment_WhenAlreadyPaid_ShouldThrow()
    {
        var ap = CreateValid();
        ap.RegisterPayment(2000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        var act = () => ap.RegisterPayment(100m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetStatusToCancelled()
    {
        var ap = CreateValid();
        ap.Cancel("erro de lancamento");
        ap.Status.Should().Be(AccountPayableStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrow()
    {
        var ap = CreateValid();
        ap.RegisterPayment(2000m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        var act = () => ap.Cancel("erro");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsOverdue_WhenPastDue_ShouldSetStatusToOverdue()
    {
        var ap = AccountPayable.Create(Guid.NewGuid(), "compra", 1000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        ap.MarkAsOverdue();
        ap.Status.Should().Be(AccountPayableStatus.Overdue);
    }
}

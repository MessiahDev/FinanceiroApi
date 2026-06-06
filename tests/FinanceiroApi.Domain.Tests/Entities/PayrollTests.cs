using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Tests.Entities;

public class PayrollTests
{
    private static Payroll CreateValid() => Payroll.Create(2024, 6);

    private static void AddEmployee(Payroll payroll, Guid? id = null) =>
        payroll.AddItem(
            id ?? Guid.NewGuid(),
            new Money(5000m),
            new Money(550m),
            new Money(750m),
            new Money(0m));

    [Fact]
    public void Create_ValidYearAndMonth_ShouldReturnDraftPayroll()
    {
        var payroll = CreateValid();

        Assert.Equal(PayrollStatus.Draft, payroll.Status);
        Assert.Empty(payroll.Items);
        Assert.Equal(Money.Zero, payroll.TotalNet);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Create_InvalidYear_ShouldThrowDomainException(int year)
    {
        Assert.Throws<DomainException>(() => Payroll.Create(year, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_InvalidMonth_ShouldThrowDomainException(int month)
    {
        Assert.Throws<DomainException>(() => Payroll.Create(2024, month));
    }

    [Fact]
    public void AddItem_ValidEmployee_ShouldAddAndRecalculate()
    {
        var payroll = CreateValid();

        AddEmployee(payroll);

        Assert.Single(payroll.Items);
        Assert.Equal(5000m, payroll.TotalGross.Amount);
        Assert.Equal(3700m, payroll.TotalNet.Amount);
    }

    [Fact]
    public void AddItem_DuplicateEmployee_ShouldThrowDomainException()
    {
        var payroll = CreateValid();
        var employeeId = Guid.NewGuid();
        AddEmployee(payroll, employeeId);

        Assert.Throws<DomainException>(() => AddEmployee(payroll, employeeId));
    }

    [Fact]
    public void AddItem_ToNonDraftPayroll_ShouldThrowDomainException()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();

        Assert.Throws<DomainException>(() => AddEmployee(payroll));
    }

    [Fact]
    public void Process_DraftWithItems_ShouldSetStatusToProcessing()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);

        payroll.Process();

        Assert.Equal(PayrollStatus.Processing, payroll.Status);
        Assert.NotNull(payroll.ProcessedAt);
    }

    [Fact]
    public void Process_EmptyPayroll_ShouldThrowDomainException()
    {
        var payroll = CreateValid();

        Assert.Throws<DomainException>(() => payroll.Process());
    }

    [Fact]
    public void Process_ShouldRaiseDomainEvent()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);

        payroll.Process();

        Assert.Contains(payroll.DomainEvents, e => e is PayrollProcessedEvent);
    }

    [Fact]
    public void Approve_ProcessingPayroll_ShouldSetStatusToApproved()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();

        payroll.Approve();

        Assert.Equal(PayrollStatus.Approved, payroll.Status);
    }

    [Fact]
    public void Approve_DraftPayroll_ShouldThrowDomainException()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);

        Assert.Throws<DomainException>(() => payroll.Approve());
    }

    [Fact]
    public void MarkAsPaid_ApprovedPayroll_ShouldSetStatusToPaid()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();
        payroll.Approve();

        payroll.MarkAsPaid();

        Assert.Equal(PayrollStatus.Paid, payroll.Status);
        Assert.NotNull(payroll.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_NotApproved_ShouldThrowDomainException()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();

        Assert.Throws<DomainException>(() => payroll.MarkAsPaid());
    }

    [Fact]
    public void MarkAsPaid_ShouldRaiseDomainEvent()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();
        payroll.Approve();
        payroll.ClearDomainEvents();

        payroll.MarkAsPaid();

        Assert.Contains(payroll.DomainEvents, e => e is PayrollPaidEvent);
    }

    [Fact]
    public void Cancel_DraftPayroll_ShouldSetStatusToCancelled()
    {
        var payroll = CreateValid();

        payroll.Cancel("Folha incorreta");

        Assert.Equal(PayrollStatus.Cancelled, payroll.Status);
    }

    [Fact]
    public void Cancel_PaidPayroll_ShouldThrowDomainException()
    {
        var payroll = CreateValid();
        AddEmployee(payroll);
        payroll.Process();
        payroll.Approve();
        payroll.MarkAsPaid();

        Assert.Throws<DomainException>(() => payroll.Cancel("Tentativa"));
    }

    [Fact]
    public void Cancel_ShouldRaiseDomainEvent()
    {
        var payroll = CreateValid();
        payroll.ClearDomainEvents();

        payroll.Cancel("Motivo");

        Assert.Contains(payroll.DomainEvents, e => e is PayrollCancelledEvent);
    }
}

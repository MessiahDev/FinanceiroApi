using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class BankReconciliationTests
{
    private static readonly Guid BankAccountId = Guid.NewGuid();
    private static readonly Guid BankStatementId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private static BankReconciliation CreateValid() => BankReconciliation.Create(
        bankAccountId: BankAccountId,
        bankStatementId: BankStatementId,
        periodStart: PeriodStart,
        periodEnd: PeriodEnd,
        statementOpeningBalance: 1000m,
        statementClosingBalance: 3000m,
        systemBalance: 3000m);

    [Fact]
    public void Create_WithValidData_ShouldCreateReconciliation()
    {
        var rec = CreateValid();

        rec.Should().NotBeNull();
        rec.BankAccountId.Should().Be(BankAccountId);
        rec.BankStatementId.Should().Be(BankStatementId);
        rec.Status.Should().Be(ReconciliationStatus.Open);
        rec.StatementOpeningBalance.Amount.Should().Be(1000m);
        rec.StatementClosingBalance.Amount.Should().Be(3000m);
        rec.SystemBalance.Amount.Should().Be(3000m);
    }

    [Fact]
    public void Create_ShouldRaiseBankReconciliationCreatedEvent()
    {
        var rec = CreateValid();

        rec.DomainEvents.Should().ContainSingle(e => e is BankReconciliationCreatedEvent);
    }

    [Fact]
    public void Create_WhenPeriodEndBeforePeriodStart_ShouldThrow()
    {
        var act = () => BankReconciliation.Create(BankAccountId, BankStatementId,
            periodStart: new DateOnly(2025, 2, 1),
            periodEnd: new DateOnly(2025, 1, 1),
            statementOpeningBalance: 1000m,
            statementClosingBalance: 3000m,
            systemBalance: 3000m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsBalanced_WhenSystemBalanceEqualsStatementClosing_ShouldBeTrue()
    {
        var rec = CreateValid();

        rec.IsBalanced.Should().BeTrue();
        rec.Difference.Amount.Should().Be(0m);
    }

    [Fact]
    public void Difference_WhenSystemBalanceDiffers_ShouldReflectDifference()
    {
        var rec = BankReconciliation.Create(BankAccountId, BankStatementId,
            PeriodStart, PeriodEnd,
            statementOpeningBalance: 1000m,
            statementClosingBalance: 3000m,
            systemBalance: 2800m);

        rec.IsBalanced.Should().BeFalse();
        rec.Difference.Amount.Should().Be(200m);
    }

    [Fact]
    public void AddItem_WhenOpen_ShouldAddItemAndSetStatusInProgress()
    {
        var rec = CreateValid();

        rec.AddItem(Guid.NewGuid(), Guid.NewGuid(), 500m, ReconciliationItemStatus.Matched);

        rec.TotalItems.Should().Be(1);
        rec.Status.Should().Be(ReconciliationStatus.InProgress);
    }

    [Fact]
    public void AddItem_WhenCompleted_ShouldThrow()
    {
        var rec = CreateValid();
        var userId = Guid.NewGuid();
        rec.AddItem(Guid.NewGuid(), Guid.NewGuid(), 500m, ReconciliationItemStatus.Matched);
        rec.Complete(userId);

        var act = () => rec.AddItem(Guid.NewGuid(), null, 100m, ReconciliationItemStatus.Matched);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WhenCancelled_ShouldThrow()
    {
        var rec = CreateValid();
        rec.Cancel("motivo");

        var act = () => rec.AddItem(Guid.NewGuid(), null, 100m, ReconciliationItemStatus.Matched);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MatchedItems_ShouldCountOnlyMatchedStatus()
    {
        var rec = CreateValid();
        rec.AddItem(Guid.NewGuid(), Guid.NewGuid(), 100m, ReconciliationItemStatus.Matched);
        rec.AddItem(Guid.NewGuid(), Guid.NewGuid(), 200m, ReconciliationItemStatus.Matched);
        rec.AddItem(Guid.NewGuid(), null, 50m, ReconciliationItemStatus.Unmatched);

        rec.MatchedItems.Should().Be(2);
        rec.UnmatchedItems.Should().Be(1);
    }

    [Fact]
    public void Complete_WithNoItems_ShouldSetStatusCompleted()
    {
        var rec = CreateValid();
        var userId = Guid.NewGuid();

        rec.Complete(userId);

        rec.Status.Should().Be(ReconciliationStatus.Completed);
        rec.CompletedBy.Should().Be(userId);
        rec.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldRaiseBankReconciliationCompletedEvent()
    {
        var rec = CreateValid();

        rec.Complete(Guid.NewGuid());

        rec.DomainEvents.Should().Contain(e => e is BankReconciliationCompletedEvent);
    }

    [Fact]
    public void Complete_WithPendingItems_ShouldThrow()
    {
        var rec = CreateValid();
        rec.AddItem(Guid.NewGuid(), null, 100m, ReconciliationItemStatus.Pending);

        var act = () => rec.Complete(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var rec = CreateValid();
        rec.Complete(Guid.NewGuid());

        var act = () => rec.Complete(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_WhenCancelled_ShouldThrow()
    {
        var rec = CreateValid();
        rec.Cancel("motivo");

        var act = () => rec.Complete(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenOpen_ShouldSetStatusCancelled()
    {
        var rec = CreateValid();

        rec.Cancel("erro na conciliação");

        rec.Status.Should().Be(ReconciliationStatus.Cancelled);
        rec.Notes.Should().Contain("erro na conciliação");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrow()
    {
        var rec = CreateValid();
        rec.Complete(Guid.NewGuid());

        var act = () => rec.Cancel("motivo");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WithExistingNotes_ShouldAppendReason()
    {
        var rec = BankReconciliation.Create(BankAccountId, BankStatementId,
            PeriodStart, PeriodEnd, 1000m, 3000m, 3000m, notes: "nota original");

        rec.Cancel("cancelamento");

        rec.Notes.Should().Contain("nota original");
        rec.Notes.Should().Contain("cancelamento");
    }
}

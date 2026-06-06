using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class BankStatementTests
{
    private static readonly Guid BankAccountId = Guid.NewGuid();
    private static readonly DateOnly StatementDate = new(2025, 1, 31);
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private static BankStatement CreateValid() => BankStatement.Create(
        bankAccountId: BankAccountId,
        statementDate: StatementDate,
        periodStart: PeriodStart,
        periodEnd: PeriodEnd,
        openingBalance: 1000m,
        closingBalance: 3000m);

    [Fact]
    public void Create_WithValidData_ShouldCreateBankStatement()
    {
        var statement = CreateValid();

        statement.Should().NotBeNull();
        statement.BankAccountId.Should().Be(BankAccountId);
        statement.OpeningBalance.Amount.Should().Be(1000m);
        statement.ClosingBalance.Amount.Should().Be(3000m);
        statement.Status.Should().Be(BankStatementStatus.Imported);
    }

    [Fact]
    public void Create_ShouldRaiseBankStatementImportedEvent()
    {
        var statement = CreateValid();

        statement.DomainEvents.Should().ContainSingle(e => e is BankStatementImportedEvent);
    }

    [Fact]
    public void Create_WhenPeriodEndBeforePeriodStart_ShouldThrow()
    {
        var act = () => BankStatement.Create(BankAccountId, StatementDate,
            periodStart: new DateOnly(2025, 2, 1),
            periodEnd: new DateOnly(2025, 1, 1),
            openingBalance: 1000m, closingBalance: 3000m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddEntry_WithValidData_ShouldAddEntry()
    {
        var statement = CreateValid();

        statement.AddEntry(PeriodStart, "Depósito", 500m, BankStatementEntryType.Credit);

        statement.TotalEntries.Should().Be(1);
    }

    [Fact]
    public void AddEntry_WhenCancelled_ShouldThrow()
    {
        var statement = CreateValid();
        statement.Cancel("motivo");

        var act = () => statement.AddEntry(PeriodStart, "Depósito", 500m, BankStatementEntryType.Credit);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void AddEntry_WithEmptyDescription_ShouldThrow(string? description)
    {
        var statement = CreateValid();

        var act = () => statement.AddEntry(PeriodStart, description!, 500m, BankStatementEntryType.Credit);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddEntry_WithInvalidAmount_ShouldThrow(decimal amount)
    {
        var statement = CreateValid();

        var act = () => statement.AddEntry(PeriodStart, "Desc", amount, BankStatementEntryType.Credit);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TotalCredits_ShouldSumOnlyCreditEntries()
    {
        var statement = CreateValid();
        statement.AddEntry(PeriodStart, "Crédito 1", 1000m, BankStatementEntryType.Credit);
        statement.AddEntry(PeriodStart, "Crédito 2", 500m, BankStatementEntryType.Credit);
        statement.AddEntry(PeriodStart, "Débito", 200m, BankStatementEntryType.Debit);

        statement.TotalCredits.Amount.Should().Be(1500m);
    }

    [Fact]
    public void TotalDebits_ShouldSumOnlyDebitEntries()
    {
        var statement = CreateValid();
        statement.AddEntry(PeriodStart, "Débito 1", 300m, BankStatementEntryType.Debit);
        statement.AddEntry(PeriodStart, "Crédito", 1000m, BankStatementEntryType.Credit);

        statement.TotalDebits.Amount.Should().Be(300m);
    }

    [Fact]
    public void MarkAsReconciled_WhenImported_ShouldSetStatusReconciled()
    {
        var statement = CreateValid();

        statement.MarkAsReconciled();

        statement.Status.Should().Be(BankStatementStatus.Reconciled);
    }

    [Fact]
    public void MarkAsReconciled_WhenCancelled_ShouldThrow()
    {
        var statement = CreateValid();
        statement.Cancel("motivo");

        var act = () => statement.MarkAsReconciled();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenImported_ShouldSetStatusCancelled()
    {
        var statement = CreateValid();

        statement.Cancel("importação errada");

        statement.Status.Should().Be(BankStatementStatus.Cancelled);
        statement.Notes.Should().Contain("importação errada");
    }

    [Fact]
    public void Cancel_WhenReconciled_ShouldThrow()
    {
        var statement = CreateValid();
        statement.MarkAsReconciled();

        var act = () => statement.Cancel("motivo");

        act.Should().Throw<DomainException>();
    }
}

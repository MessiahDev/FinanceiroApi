using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class TaxEntryTests
{
    private static TaxEntry CreateValid() => TaxEntry.Create(
        taxType: TaxType.ISS,
        description: "ISS sobre serviços",
        baseAmount: 10000m,
        rate: 5m,
        competence: new DateOnly(2025, 1, 1),
        dueDate: new DateOnly(2025, 1, 31));

    [Fact]
    public void Create_WithValidData_ShouldCreateTaxEntry()
    {
        var entry = CreateValid();

        entry.Should().NotBeNull();
        entry.TaxType.Should().Be(TaxType.ISS);
        entry.Description.Should().Be("ISS sobre serviços");
        entry.BaseAmount.Amount.Should().Be(10000m);
        entry.Rate.Should().Be(5m);
        entry.TaxAmount.Amount.Should().Be(500m);
        entry.Status.Should().Be(TaxEntryStatus.Calculated);
    }

    [Fact]
    public void Create_ShouldRaiseTaxEntryCreatedEvent()
    {
        var entry = CreateValid();

        entry.DomainEvents.Should().ContainSingle(e => e is TaxEntryCreatedEvent);
    }

    [Fact]
    public void Create_ShouldCalculateTaxAmountCorrectly()
    {
        var entry = TaxEntry.Create(TaxType.ISS, "Serviço", 5000m, 10m,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        entry.TaxAmount.Amount.Should().Be(500m);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ShouldThrow(string? description)
    {
        var act = () => TaxEntry.Create(TaxType.ISS, description!, 10000m, 5m,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidBaseAmount_ShouldThrow(decimal amount)
    {
        var act = () => TaxEntry.Create(TaxType.ISS, "Desc", amount, 5m,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithInvalidRate_ShouldThrow(decimal rate)
    {
        var act = () => TaxEntry.Create(TaxType.ISS, "Desc", 10000m, rate,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenDueDateBeforeCompetence_ShouldThrow()
    {
        var act = () => TaxEntry.Create(TaxType.ISS, "Desc", 10000m, 5m,
            competence: new DateOnly(2025, 2, 1),
            dueDate: new DateOnly(2025, 1, 1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_WhenCalculated_ShouldSetStatusPaid()
    {
        var entry = CreateValid();

        entry.MarkAsPaid();

        entry.Status.Should().Be(TaxEntryStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldThrow()
    {
        var entry = CreateValid();
        entry.MarkAsPaid();

        var act = () => entry.MarkAsPaid();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_WhenCancelled_ShouldThrow()
    {
        var entry = CreateValid();
        entry.Cancel("motivo");

        var act = () => entry.MarkAsPaid();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenCalculated_ShouldSetStatusCancelled()
    {
        var entry = CreateValid();

        entry.Cancel("erro no lançamento");

        entry.Status.Should().Be(TaxEntryStatus.Cancelled);
        entry.Notes.Should().Contain("erro no lançamento");
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrow()
    {
        var entry = CreateValid();
        entry.MarkAsPaid();

        var act = () => entry.Cancel("motivo");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WithExistingNotes_ShouldAppendReason()
    {
        var entry = TaxEntry.Create(TaxType.ISS, "Desc", 10000m, 5m,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), notes: "nota original");

        entry.Cancel("cancelamento");

        entry.Notes.Should().Contain("nota original");
        entry.Notes.Should().Contain("cancelamento");
    }

    [Fact]
    public void UpdateNotes_ShouldUpdateNotes()
    {
        var entry = CreateValid();

        entry.UpdateNotes("nova nota");

        entry.Notes.Should().Be("nova nota");
    }
}

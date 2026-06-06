using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class JournalEntryTests
{
    private static JournalEntry CreateValid() =>
        JournalEntry.Create(
            "LCT-2024-001",
            "Lançamento de teste",
            DateTime.UtcNow,
            JournalEntryType.Manual,
            Guid.NewGuid(),
            Guid.NewGuid());

    [Fact]
    public void Create_ValidData_ShouldReturnDraftEntry()
    {
        var entry = CreateValid();

        Assert.Equal("LCT-2024-001", entry.EntryNumber);
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.Empty(entry.Lines);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyEntryNumber_ShouldThrowDomainException(string number)
    {
        Assert.Throws<DomainException>(() =>
            JournalEntry.Create(number, "Desc", DateTime.UtcNow, JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyDescription_ShouldThrowDomainException(string desc)
    {
        Assert.Throws<DomainException>(() =>
            JournalEntry.Create("LCT-001", desc, DateTime.UtcNow, JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Create_DefaultDate_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() =>
            JournalEntry.Create("LCT-001", "Desc", default, JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Create_EmptyAccountingPeriodId_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() =>
            JournalEntry.Create("LCT-001", "Desc", DateTime.UtcNow, JournalEntryType.Manual, Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void AddLine_ToDraftEntry_ShouldAddLine()
    {
        var entry = CreateValid();

        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 1000m);

        Assert.Single(entry.Lines);
    }

    [Fact]
    public void AddLine_ZeroAmount_ShouldThrowDomainException()
    {
        var entry = CreateValid();

        Assert.Throws<DomainException>(() =>
            entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 0m));
    }

    [Fact]
    public void AddLine_ToPostedEntry_ShouldThrowDomainException()
    {
        var entry = CreateValid();
        var accountId = Guid.NewGuid();
        entry.AddLine(accountId, DebitCredit.Debit, 1000m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 1000m);
        entry.Post();

        Assert.Throws<DomainException>(() =>
            entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 500m));
    }

    [Fact]
    public void Post_BalancedEntry_ShouldSetStatusToPosted()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 1000m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 1000m);

        entry.Post();

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
    }

    [Fact]
    public void Post_EmptyEntry_ShouldThrowDomainException()
    {
        var entry = CreateValid();

        Assert.Throws<DomainException>(() => entry.Post());
    }

    [Fact]
    public void Post_UnbalancedEntry_ShouldThrowDomainException()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 1000m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 500m);

        Assert.Throws<DomainException>(() => entry.Post());
    }

    [Fact]
    public void Post_AlreadyPosted_ShouldThrowDomainException()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 100m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 100m);
        entry.Post();

        Assert.Throws<DomainException>(() => entry.Post());
    }

    [Fact]
    public void Post_ShouldRaiseDomainEvent()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 100m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 100m);
        entry.ClearDomainEvents();

        entry.Post();

        Assert.Single(entry.DomainEvents);
    }

    [Fact]
    public void Reverse_PostedEntry_ShouldSetStatusToReversed()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 100m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 100m);
        entry.Post();

        entry.Reverse("Estorno de teste", Guid.NewGuid());

        Assert.Equal(JournalEntryStatus.Reversed, entry.Status);
    }

    [Fact]
    public void Reverse_DraftEntry_ShouldThrowDomainException()
    {
        var entry = CreateValid();

        Assert.Throws<DomainException>(() => entry.Reverse("Estorno", Guid.NewGuid()));
    }

    [Fact]
    public void TotalDebitsAndCredits_ShouldReturnCorrectValues()
    {
        var entry = CreateValid();
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 700m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 300m);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 1000m);

        Assert.Equal(1000m, entry.TotalDebits());
        Assert.Equal(1000m, entry.TotalCredits());
        Assert.True(entry.IsBalanced());
    }
}

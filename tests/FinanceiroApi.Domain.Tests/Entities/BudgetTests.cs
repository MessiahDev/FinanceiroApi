using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class BudgetTests
{
    private static Budget CreateValid() => Budget.Create(2025, "Orcamento Anual", "Descricao");

    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var budget = CreateValid();
        budget.Should().NotBeNull();
        budget.Status.Should().Be(BudgetStatus.Draft);
        budget.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Create_WithInvalidYear_ShouldThrow(int year)
    {
        var act = () => Budget.Create(year, "Orcamento");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddAndRecalculate()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Items.Should().HaveCount(1);
        budget.TotalPlanned.Amount.Should().Be(10000m);
    }

    [Fact]
    public void AddItem_DuplicateCostCenterCategory_ShouldThrow()
    {
        var budget = CreateValid();
        var ccId = Guid.NewGuid();
        budget.AddItem(ccId, "TI", 10000m);
        var act = () => budget.AddItem(ccId, "TI", 5000m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WhenNotDraft_ShouldThrow()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        var act = () => budget.AddItem(Guid.NewGuid(), "RH", 5000m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_WithItems_ShouldSetStatusToApproved()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        budget.Status.Should().Be(BudgetStatus.Approved);
        budget.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_WhenEmpty_ShouldThrow()
    {
        var budget = CreateValid();
        var act = () => budget.Approve(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldThrow()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        var act = () => budget.Approve(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Close_WhenApproved_ShouldSetStatusToClosed()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        budget.Close();
        budget.Status.Should().Be(BudgetStatus.Closed);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldSetStatusToCancelled()
    {
        var budget = CreateValid();
        budget.Cancel();
        budget.Status.Should().Be(BudgetStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenClosed_ShouldThrow()
    {
        var budget = CreateValid();
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        budget.Close();
        var act = () => budget.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterRealization_WhenApproved_ShouldUpdateTotals()
    {
        var budget = CreateValid();
        var ccId = Guid.NewGuid();
        budget.AddItem(ccId, "TI", 10000m);
        budget.Approve(Guid.NewGuid());
        budget.RegisterRealization(ccId, "TI", 3000m);
        budget.TotalRealized.Amount.Should().Be(3000m);
    }
}

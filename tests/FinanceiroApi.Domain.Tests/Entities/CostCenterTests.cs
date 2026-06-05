using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class CostCenterTests
{
    private static CostCenter CreateValid() => CostCenter.Create("TI-001", "Tecnologia", 50000m);

    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var cc = CreateValid();
        cc.Should().NotBeNull();
        cc.Status.Should().Be(CostCenterStatus.Active);
        cc.Code.Should().Be("TI-001");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyCode_ShouldThrow(string? code)
    {
        var act = () => CostCenter.Create(code!, "Nome", 1000m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeBudget_ShouldThrow()
    {
        var act = () => CostCenter.Create("CC-001", "Nome", -1m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdate()
    {
        var cc = CreateValid();
        cc.Update("TI-002", "Novo Nome", "desc", null);
        cc.Code.Should().Be("TI-002");
        cc.Name.Should().Be("Novo Nome");
    }

    [Fact]
    public void UpdateBudget_WithValidValue_ShouldUpdate()
    {
        var cc = CreateValid();
        cc.UpdateBudget(100000m);
        cc.AnnualBudget.Amount.Should().Be(100000m);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var cc = CreateValid();
        cc.Deactivate();
        cc.Status.Should().Be(CostCenterStatus.Inactive);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        var cc = CreateValid();
        cc.Deactivate();
        cc.Activate();
        cc.Status.Should().Be(CostCenterStatus.Active);
    }
}

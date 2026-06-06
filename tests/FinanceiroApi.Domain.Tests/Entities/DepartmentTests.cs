using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class DepartmentTests
{
    [Fact]
    public void Create_ValidData_ShouldReturnActiveDepartment()
    {
        var dept = Department.Create("Tecnologia", "TI-001", "Dept de TI");

        Assert.Equal("Tecnologia", dept.Name);
        Assert.Equal("TI-001", dept.CostCenter);
        Assert.True(dept.IsActive);
    }

    [Fact]
    public void Create_CostCenter_ShouldBeUpperCase()
    {
        var dept = Department.Create("RH", "rh-001");

        Assert.Equal("RH-001", dept.CostCenter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyName_ShouldThrowDomainException(string name)
    {
        Assert.Throws<DomainException>(() => Department.Create(name, "CC-001"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyCostCenter_ShouldThrowDomainException(string cc)
    {
        Assert.Throws<DomainException>(() => Department.Create("RH", cc));
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateProperties()
    {
        var dept = Department.Create("TI", "TI-001");

        dept.Update("Tecnologia", "TI-002", "Nova descrição");

        Assert.Equal("Tecnologia", dept.Name);
        Assert.Equal("TI-002", dept.CostCenter);
        Assert.Equal("Nova descrição", dept.Description);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var dept = Department.Create("TI", "TI-001");

        dept.Deactivate();

        Assert.False(dept.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var dept = Department.Create("TI", "TI-001");
        dept.Deactivate();

        dept.Activate();

        Assert.True(dept.IsActive);
    }
}
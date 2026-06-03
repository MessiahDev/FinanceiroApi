using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class EmployeeTests
{
    private static Employee CreateValidEmployee() => Employee.Create(
        firstName: "João",
        lastName: "Silva",
        cpf: "52998224725",
        email: "joao@empresa.com",
        baseSalary: 5000m,
        contractType: ContractType.CLT,
        departmentId: Guid.NewGuid(),
        position: Position.DesenvolvedorJunior);

    [Fact]
    public void Create_WithValidData_ShouldCreateEmployee()
    {
        var employee = CreateValidEmployee();

        employee.Should().NotBeNull();
        employee.FullName.Should().Be("João Silva");
        employee.Status.Should().Be(EmployeeStatus.Active);
        employee.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseEmployeeCreatedEvent()
    {
        var employee = CreateValidEmployee();

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldThrowDomainException(string? firstName)
    {
        var act = () => Employee.Create(firstName!, "Silva", "52998224725",
            "test@test.com", 1000m, ContractType.CLT, Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void UpdateSalary_WithValidValue_ShouldUpdateAndRaiseDomainEvent()
    {
        var employee = CreateValidEmployee();

        employee.UpdateSalary(6000m);

        employee.BaseSalary.Amount.Should().Be(6000m);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeSalaryUpdatedEvent);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var employee = CreateValidEmployee();

        employee.Deactivate();

        employee.Status.Should().Be(EmployeeStatus.Inactive);
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();
        employee.Deactivate();

        var act = () => employee.Deactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_WhenTerminated_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();
        employee.Terminate(DateOnly.FromDateTime(DateTime.UtcNow));

        var act = () => employee.Activate();

        act.Should().Throw<DomainException>().WithMessage("*terminated*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetStatusToActive()
    {
        var employee = CreateValidEmployee();
        employee.Deactivate();

        employee.Activate();

        employee.Status.Should().Be(EmployeeStatus.Active);
    }

    [Fact]
    public void Terminate_WhenActive_ShouldSetStatusAndRaiseDomainEvent()
    {
        var employee = CreateValidEmployee();
        var terminationDate = DateOnly.FromDateTime(DateTime.UtcNow);

        employee.Terminate(terminationDate);

        employee.Status.Should().Be(EmployeeStatus.Terminated);
        employee.TerminationDate.Should().Be(terminationDate);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeTerminatedEvent);
    }

    [Fact]
    public void Terminate_AlreadyTerminated_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();
        employee.Terminate(DateOnly.FromDateTime(DateTime.UtcNow));

        var act = () => employee.Terminate(DateOnly.FromDateTime(DateTime.UtcNow));

        act.Should().Throw<DomainException>().WithMessage("*already terminated*");
    }

    [Fact]
    public void PlaceOnLeave_WhenActive_ShouldSetStatusToOnLeave()
    {
        var employee = CreateValidEmployee();

        employee.PlaceOnLeave();

        employee.Status.Should().Be(EmployeeStatus.OnLeave);
    }

    [Fact]
    public void PlaceOnLeave_WhenInactive_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();
        employee.Deactivate();

        var act = () => employee.PlaceOnLeave();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReturnFromLeave_WhenOnLeave_ShouldSetStatusToActive()
    {
        var employee = CreateValidEmployee();
        employee.PlaceOnLeave();

        employee.ReturnFromLeave();

        employee.Status.Should().Be(EmployeeStatus.Active);
    }

    [Fact]
    public void ReturnFromLeave_WhenNotOnLeave_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();

        var act = () => employee.ReturnFromLeave();

        act.Should().Throw<DomainException>().WithMessage("*not on leave*");
    }

    [Fact]
    public void TransferToDepartment_WithNewDepartment_ShouldUpdateDepartmentId()
    {
        var employee = CreateValidEmployee();
        var newDepartmentId = Guid.NewGuid();

        employee.TransferToDepartment(newDepartmentId);

        employee.DepartmentId.Should().Be(newDepartmentId);
    }

    [Fact]
    public void TransferToDepartment_SameDepartment_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();
        var sameDepartmentId = employee.DepartmentId;

        var act = () => employee.TransferToDepartment(sameDepartmentId);

        act.Should().Throw<DomainException>().WithMessage("*already in this department*");
    }

    [Fact]
    public void UpdatePersonalInfo_WithValidData_ShouldUpdateFields()
    {
        var employee = CreateValidEmployee();

        employee.UpdatePersonalInfo("Carlos", "Souza", "carlos@empresa.com", Position.DesenvolvedorSenior);

        employee.FirstName.Should().Be("Carlos");
        employee.LastName.Should().Be("Souza");
        employee.FullName.Should().Be("Carlos Souza");
    }

    [Fact]
    public void UpdatePersonalInfo_WithEmptyFirstName_ShouldThrowDomainException()
    {
        var employee = CreateValidEmployee();

        var act = () => employee.UpdatePersonalInfo("", "Souza", "carlos@empresa.com", null);

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }
}
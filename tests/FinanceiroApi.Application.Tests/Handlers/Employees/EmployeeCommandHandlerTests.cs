using AutoMapper;
using FinanceiroApi.Application.Commands.Employees.DeleteEmployee;
using FinanceiroApi.Application.Commands.Employees.UpdateEmployee;
using FinanceiroApi.Application.Commands.Employees.UpdateSalary;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Employees;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IDepartmentRepository _deptRepo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private UpdateEmployeeCommandHandler CreateHandler() =>
        new(_employeeRepo, _deptRepo, _uow, _mapper, _notif);

    private static Employee MakeEmployee(Guid deptId) =>
        Employee.Create("João", "Silva", "529.982.247-25", "joao@email.com",
            5000m, ContractType.CLT, deptId, Position.Analista);

    private static Department MakeDepartment() =>
        Department.Create("TI", "CC-001", null);

    [Fact]
    public async Task Handle_ValidUpdate_ShouldUpdateAndReturnResponse()
    {
        var dept = MakeDepartment();
        var employee = MakeEmployee(dept.Id);
        var cmd = new UpdateEmployeeCommand(employee.Id, "Carlos", "Souza",
            "carlos@email.com", Position.DesenvolvedorSenior, dept.Id);

        _employeeRepo.GetByIdAsync(employee.Id, default).Returns(employee);
        _deptRepo.GetByIdAsync(dept.Id, default).Returns(dept);
        _mapper.Map<EmployeeResponse>(Arg.Any<Employee>()).Returns(new EmployeeResponse(
            employee.Id, "Carlos", "Souza", "Carlos Souza", "carlos@email.com",
            "529.982.247-25", Position.DesenvolvedorSenior, dept.Id, dept.Name,
            5000m, "BRL", "Active", "CLT", DateOnly.FromDateTime(DateTime.UtcNow), null,
            DateTime.UtcNow, null));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        employee.FirstName.Should().Be("Carlos");
        await _employeeRepo.Received(1).UpdateAsync(employee, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ShouldNotifyAndReturnNull()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var cmd = new UpdateEmployeeCommand(Guid.NewGuid(), "Nome", "Sobrenome",
            "e@e.com", Position.Analista, Guid.NewGuid());

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ShouldNotifyAndReturnNull()
    {
        var employee = MakeEmployee(Guid.NewGuid());
        _employeeRepo.GetByIdAsync(employee.Id, default).Returns(employee);
        _deptRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var cmd = new UpdateEmployeeCommand(employee.Id, "Nome", "Sobrenome",
            "e@e.com", Position.Analista, Guid.NewGuid());

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("DepartmentId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class DeleteEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _repo = Substitute.For<IEmployeeRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private DeleteEmployeeCommandHandler CreateHandler() => new(_repo, _uow, _notif);

    [Fact]
    public async Task Handle_ActiveEmployee_ShouldDeactivateAndReturnTrue()
    {
        var employee = Employee.Create("João", "Silva", "529.982.247-25",
            "joao@email.com", 5000m, ContractType.CLT, Guid.NewGuid());
        _repo.GetByIdAsync(employee.Id, default).Returns(employee);

        var result = await CreateHandler().Handle(new DeleteEmployeeCommand(employee.Id), default);

        result.Should().BeTrue();
        employee.Status.Should().Be(EmployeeStatus.Inactive);
        await _repo.Received(1).UpdateAsync(employee, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new DeleteEmployeeCommand(Guid.NewGuid()), default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class UpdateSalaryCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IDepartmentRepository _deptRepo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private UpdateSalaryCommandHandler CreateHandler() =>
        new(_employeeRepo, _deptRepo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_ExistingEmployee_ShouldUpdateSalaryAndReturnResponse()
    {
        var employee = Employee.Create("João", "Silva", "529.982.247-25",
            "joao@email.com", 5000m, ContractType.CLT, Guid.NewGuid());
        _employeeRepo.GetByIdAsync(employee.Id, default).Returns(employee);
        _mapper.Map<EmployeeResponse>(Arg.Any<Employee>()).Returns(new EmployeeResponse(
            employee.Id, employee.FirstName, employee.LastName, employee.FullName,
            employee.Email.ToString(), employee.Cpf.ToString(), employee.Position,
            employee.DepartmentId, "TI", 7000m, "BRL", "Active", "CLT",
            employee.HireDate, null, DateTime.UtcNow, null));

        var result = await CreateHandler().Handle(
            new UpdateSalaryCommand(employee.Id, 7000m, "Promoção"), default);

        result.Should().NotBeNull();
        employee.BaseSalary.Amount.Should().Be(7000m);
        await _employeeRepo.Received(1).UpdateAsync(employee, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ShouldNotifyAndReturnNull()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new UpdateSalaryCommand(Guid.NewGuid(), 7000m, "Promoção"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("EmployeeId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

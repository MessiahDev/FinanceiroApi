using AutoMapper;
using FinanceiroApi.Application.Commands.Employees.CreateEmployee;
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

namespace FinanceiroApi.Application.Tests.Handlers;

public class CreateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IDepartmentRepository _departmentRepo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notifications = Substitute.For<INotificationContext>();

    private CreateEmployeeCommandHandler CreateHandler() => new(
        _employeeRepo, _departmentRepo, _unitOfWork, _mapper, _notifications);

    private static CreateEmployeeCommand ValidCommand() => new(
        FirstName: "João",
        LastName: "Silva",
        Email: "joao@empresa.com",
        Cpf: "52998224725",
        Position: Position.DesenvolvedorJunior,
        DepartmentId: Guid.NewGuid(),
        BaseSalary: 5000m,
        ContractType: ContractType.CLT);

    private static EmployeeResponse MakeResponse(CreateEmployeeCommand cmd) => new(
        Id: Guid.NewGuid(),
        FirstName: cmd.FirstName,
        LastName: cmd.LastName,
        FullName: $"{cmd.FirstName} {cmd.LastName}",
        Email: cmd.Email,
        Cpf: cmd.Cpf,
        Position: cmd.Position,
        DepartmentId: cmd.DepartmentId,
        DepartmentName: "TI",
        Salary: cmd.BaseSalary,
        Currency: "BRL",
        Status: EmployeeStatus.Active.ToString(),
        ContractType: cmd.ContractType.ToString(),
        HireDate: DateOnly.FromDateTime(DateTime.UtcNow),
        TerminationDate: null,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: null);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEmployee()
    {
        var command = ValidCommand();
        var department = Department.Create("TI", "CC-001");
        var expected = MakeResponse(command);

        _departmentRepo.GetByIdAsync(command.DepartmentId, default).Returns(department);
        _employeeRepo.ExistsByCpfAsync(command.Cpf, default).Returns(false);
        _mapper.Map<EmployeeResponse>(Arg.Any<Employee>()).Returns(expected);

        var result = await CreateHandler().Handle(command, default);

        result.Should().NotBeNull();
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        await _unitOfWork.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentDepartment_ShouldAddNotificationAndReturnNull()
    {
        var command = ValidCommand();
        _departmentRepo.GetByIdAsync(command.DepartmentId, default).ReturnsNull();

        var result = await CreateHandler().Handle(command, default);

        result.Should().BeNull();
        _notifications.Received(1).AddNotification("DepartmentId", Arg.Any<string>());
        await _unitOfWork.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithDuplicateCpf_ShouldAddNotificationAndReturnNull()
    {
        var command = ValidCommand();
        var department = Department.Create("TI", "CC-001");

        _departmentRepo.GetByIdAsync(command.DepartmentId, default).Returns(department);
        _employeeRepo.ExistsByCpfAsync(command.Cpf, default).Returns(true);

        var result = await CreateHandler().Handle(command, default);

        result.Should().BeNull();
        _notifications.Received(1).AddNotification("Cpf", Arg.Any<string>());
        await _unitOfWork.DidNotReceive().CommitAsync(default);
    }
}

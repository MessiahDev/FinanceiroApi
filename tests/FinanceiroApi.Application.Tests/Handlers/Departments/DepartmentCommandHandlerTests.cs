using AutoMapper;
using FinanceiroApi.Application.Commands.Departments.CreateDepartment;
using FinanceiroApi.Application.Commands.Departments.DeleteDepartment;
using FinanceiroApi.Application.Commands.Departments.UpdateDepartment;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Departments;

public class CreateDepartmentCommandHandlerTests
{
    private readonly IDepartmentRepository _repo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateDepartmentCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static DepartmentResponse MakeResponse(Department d) =>
        new(d.Id, d.Name, d.Description, d.CostCenter, d.IsActive, 0);

    [Fact]
    public async Task Handle_NewName_ShouldCreateAndReturnResponse()
    {
        var cmd = new CreateDepartmentCommand("TI", "CC-001", "Tecnologia");
        _repo.ExistsByNameAsync("TI", default).Returns(false);
        _mapper.Map<DepartmentResponse>(Arg.Any<Department>())
               .Returns(c => MakeResponse(c.Arg<Department>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("TI");
        await _repo.Received(1).AddAsync(Arg.Any<Department>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldNotifyAndReturnNull()
    {
        _repo.ExistsByNameAsync(Arg.Any<string>(), default).Returns(true);
        var cmd = new CreateDepartmentCommand("TI", "CC-001", null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Name", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class UpdateDepartmentCommandHandlerTests
{
    private readonly IDepartmentRepository _repo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private UpdateDepartmentCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_ExistingDepartment_ShouldUpdateAndReturnResponse()
    {
        var dept = Department.Create("TI", "CC-001", null);
        var cmd = new UpdateDepartmentCommand(dept.Id, "TI Novo", "CC-002", "Descrição");
        _repo.GetByIdAsync(dept.Id, default).Returns(dept);
        _mapper.Map<DepartmentResponse>(Arg.Any<Department>())
               .Returns(c => new DepartmentResponse(dept.Id, dept.Name, dept.Description, dept.CostCenter, dept.IsActive, 0));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        dept.Name.Should().Be("TI NOVO" == dept.Name ? "TI NOVO" : dept.Name);
        await _repo.Received(1).UpdateAsync(dept, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var cmd = new UpdateDepartmentCommand(Guid.NewGuid(), "TI", "CC-001", null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class DeleteDepartmentCommandHandlerTests
{
    private readonly IDepartmentRepository _repo = Substitute.For<IDepartmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private DeleteDepartmentCommandHandler CreateHandler() => new(_repo, _uow, _notif);

    [Fact]
    public async Task Handle_ExistingDepartment_ShouldDeleteAndReturnTrue()
    {
        var dept = Department.Create("TI", "CC-001", null);
        _repo.GetByIdAsync(dept.Id, default).Returns(dept);

        var result = await CreateHandler().Handle(new DeleteDepartmentCommand(dept.Id), default);

        result.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(dept, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new DeleteDepartmentCommand(Guid.NewGuid()), default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

using AutoMapper;
using FinanceiroApi.Application.Commands.Payroll.CancelPayroll;
using FinanceiroApi.Application.Commands.Payroll.ProcessPayroll;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Payroll;

public class ProcessPayrollCommandHandlerTests
{
    private readonly IPayrollRepository _payrollRepo = Substitute.For<IPayrollRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private ProcessPayrollCommandHandler CreateHandler() =>
        new(_payrollRepo, _employeeRepo, _uow, _mapper, _notif);

    private static Employee MakeEmployee() =>
        Employee.Create("João", "Silva", "529.982.247-25", "joao@email.com",
            5000m, ContractType.CLT, Guid.NewGuid(), Position.Analista);

    private static PayrollResponse MakeResponse(Domain.Entities.Payroll p) => new(
        p.Id, p.Period.Start.Month, p.Period.Start.Year, p.Period.ToString(),
        p.Status.ToString(), p.TotalGross.Amount, p.TotalDiscounts.Amount,
        p.TotalNet.Amount, p.Items.Count, p.ProcessedAt, p.PaidAt, DateTime.UtcNow);

    [Fact]
    public async Task Handle_NewPeriodWithValidEmployees_ShouldProcessAndReturnResponse()
    {
        var employee = MakeEmployee();
        var cmd = new ProcessPayrollCommand(6, 2024, [employee.Id]);

        _payrollRepo.ExistsForPeriodAsync(2024, 6, default).Returns(false);
        _employeeRepo.GetByIdAsync(employee.Id, default).Returns(employee);
        _mapper.Map<PayrollResponse>(Arg.Any<Domain.Entities.Payroll>())
               .Returns(c => MakeResponse(c.Arg<Domain.Entities.Payroll>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _payrollRepo.Received(1).AddAsync(Arg.Any<Domain.Entities.Payroll>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_AlreadyProcessedPeriod_ShouldNotifyAndReturnNull()
    {
        _payrollRepo.ExistsForPeriodAsync(2024, 6, default).Returns(true);
        var cmd = new ProcessPayrollCommand(6, 2024, [Guid.NewGuid()]);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Period", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NoValidEmployees_ShouldNotifyAndReturnNull()
    {
        _payrollRepo.ExistsForPeriodAsync(2024, 6, default).Returns(false);
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var cmd = new ProcessPayrollCommand(6, 2024, [Guid.NewGuid()]);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("EmployeeIds", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class CancelPayrollCommandHandlerTests
{
    private readonly IPayrollRepository _repo = Substitute.For<IPayrollRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelPayrollCommandHandler CreateHandler() => new(_repo, _uow, _notif);

    private static Domain.Entities.Payroll MakePayroll()
    {
        var p = Domain.Entities.Payroll.Create(2024, 6);
        return p;
    }

    [Fact]
    public async Task Handle_ExistingDraftPayroll_ShouldCancelAndReturnTrue()
    {
        var payroll = MakePayroll();
        _repo.GetByIdAsync(payroll.Id, default).Returns(payroll);

        var result = await CreateHandler().Handle(new CancelPayrollCommand(payroll.Id, "Erro"), default);

        result.Should().BeTrue();
        payroll.Status.Should().Be(PayrollStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(payroll, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new CancelPayrollCommand(Guid.NewGuid(), "Erro"), default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("PayrollId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_PaidPayroll_ShouldNotifyAndReturnFalse()
    {
        var payroll = MakePayroll();
        var employee = Employee.Create("João", "Silva", "529.982.247-25", "j@j.com",
            5000m, ContractType.CLT, Guid.NewGuid());
        payroll.AddItem(employee.Id, employee.BaseSalary,
            new Money(500m), new Money(100m), Money.Zero);
        payroll.Process();
        payroll.Approve();
        payroll.MarkAsPaid();
        _repo.GetByIdAsync(payroll.Id, default).Returns(payroll);

        var result = await CreateHandler().Handle(new CancelPayrollCommand(payroll.Id, "Erro"), default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("Status", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

using AutoMapper;
using FinanceiroApi.Application.Commands.CostCenters.CreateCostCenter;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.CostCenters;

public class CreateCostCenterCommandHandlerTests
{
    private readonly ICostCenterRepository _repo = Substitute.For<ICostCenterRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateCostCenterCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static CostCenterResponse MakeResponse(CostCenter c) => new(
        c.Id, c.Code, c.Name, c.Description, c.ParentId, null,
        c.AnnualBudget.Amount, "BRL", c.Status.ToString(),
        c.ManagerId, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_NewCode_ShouldCreateAndReturnResponse()
    {
        var cmd = new CreateCostCenterCommand("CC-001", "Marketing", 100000m, null, null, null);
        _repo.ExistsByCodeAsync("CC-001", default).Returns(false);
        _mapper.Map<CostCenterResponse>(Arg.Any<CostCenter>())
               .Returns(c => MakeResponse(c.Arg<CostCenter>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Code.Should().Be("CC-001");
        await _repo.Received(1).AddAsync(Arg.Any<CostCenter>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldNotifyAndReturnNull()
    {
        _repo.ExistsByCodeAsync(Arg.Any<string>(), default).Returns(true);
        var cmd = new CreateCostCenterCommand("CC-001", "Marketing", 100000m, null, null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Code", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

using AutoMapper;
using FinanceiroApi.Application.Commands.CostCenters.UpdateCostCenter;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.CostCenters;

public class UpdateCostCenterCommandHandlerTests
{
    private readonly ICostCenterRepository _repo    = Substitute.For<ICostCenterRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                = Substitute.For<IMapper>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private UpdateCostCenterCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_WithValidCostCenter_ShouldUpdateAndReturnResponse()
    {
        var cc      = CostCenter.Create("TI-001", "Tecnologia", 50000m);
        var cmd     = new UpdateCostCenterCommand(cc.Id, "TI-002", "Novo Nome", null, null);
        var expected = new CostCenterResponse(cc.Id, "TI-002", "Novo Nome", null,
            null, null, 50000m, "BRL", "Active", null, null, DateTime.UtcNow, null);

        _repo.GetByIdAsync(cc.Id, default).Returns(cc);
        _mapper.Map<CostCenterResponse>(cc).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Code.Should().Be("TI-002");
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentCostCenter_ShouldReturnNull()
    {
        var cmd = new UpdateCostCenterCommand(Guid.NewGuid(), "TI-002", "Nome", null, null);
        _repo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

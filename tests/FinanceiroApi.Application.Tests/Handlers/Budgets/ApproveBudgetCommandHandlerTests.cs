using AutoMapper;
using FinanceiroApi.Application.Commands.Budgets.ApproveBudget;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Budgets;

public class ApproveBudgetCommandHandlerTests
{
    private readonly IBudgetRepository _repo        = Substitute.For<IBudgetRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                = Substitute.For<IMapper>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private ApproveBudgetCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_WithValidBudget_ShouldApproveAndReturnResponse()
    {
        var approvedBy = Guid.NewGuid();
        var budget = Budget.Create(2025, "Orcamento");
        budget.AddItem(Guid.NewGuid(), "TI", 10000m);
        var cmd = new ApproveBudgetCommand(budget.Id, approvedBy);
        var expected = new BudgetSummaryResponse(budget.Id, 2025, "Orcamento", "Approved", 10000m, 0m, 10000m, null);

        _repo.GetWithItemsAsync(budget.Id, default).Returns(budget);
        _mapper.Map<BudgetSummaryResponse>(budget).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentBudget_ShouldAddNotificationAndReturnNull()
    {
        var cmd = new ApproveBudgetCommand(Guid.NewGuid(), Guid.NewGuid());
        _repo.GetWithItemsAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

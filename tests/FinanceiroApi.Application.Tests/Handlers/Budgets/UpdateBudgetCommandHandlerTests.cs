using AutoMapper;
using FinanceiroApi.Application.Commands.Budgets.UpdateBudget;
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

public class UpdateBudgetCommandHandlerTests
{
    private readonly IBudgetRepository _repo        = Substitute.For<IBudgetRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                = Substitute.For<IMapper>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private UpdateBudgetCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_WithValidBudget_ShouldAddItemAndReturnResponse()
    {
        var budget = Budget.Create(2025, "Orcamento");
        var cmd = new UpdateBudgetCommand(budget.Id, Guid.NewGuid(), "RH", 5000m);
        var expected = new BudgetResponse(budget.Id, 2025, "Orcamento", null, "Draft",
            5000m, 0m, 5000m, "BRL", null, null, DateTime.UtcNow, null, []);

        _repo.GetWithItemsAsync(budget.Id, Arg.Any<CancellationToken>()).Returns(budget);
        _mapper.Map<BudgetResponse>(Arg.Any<Budget>()).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentBudget_ShouldReturnNull()
    {
        var cmd = new UpdateBudgetCommand(Guid.NewGuid(), Guid.NewGuid(), "TI", 1000m);
        _repo.GetWithItemsAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

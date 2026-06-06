using AutoMapper;
using FinanceiroApi.Application.Commands.Budgets.CreateBudget;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Budgets;

public class CreateBudgetCommandHandlerTests
{
    private readonly IBudgetRepository _repo = Substitute.For<IBudgetRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateBudgetCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BudgetSummaryResponse MakeResponse(Budget b) => new(
        b.Id, b.Year, b.Name, b.Status.ToString(),
        b.TotalPlanned.Amount, b.TotalRealized.Amount,
        b.Variance.Amount, b.ApprovedAt);

    [Fact]
    public async Task Handle_NewBudget_ShouldCreateAndReturnResponse()
    {
        var cmd = new CreateBudgetCommand(2024, "Orçamento Anual 2024", null);
        _repo.GetByYearAsync(2024, default).Returns([]);
        _mapper.Map<BudgetSummaryResponse>(Arg.Any<Budget>())
               .Returns(c => MakeResponse(c.Arg<Budget>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Year.Should().Be(2024);
        result.Name.Should().Be("Orçamento Anual 2024");
        await _repo.Received(1).AddAsync(Arg.Any<Budget>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateNameInYear_ShouldNotifyAndReturnNull()
    {
        var existing = Budget.Create(2024, "Orçamento Anual 2024", null);
        _repo.GetByYearAsync(2024, default).Returns([existing]);

        var cmd = new CreateBudgetCommand(2024, "Orçamento Anual 2024", null);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Name", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_SameYearDifferentName_ShouldCreateSuccessfully()
    {
        var existing = Budget.Create(2024, "Orçamento TI", null);
        _repo.GetByYearAsync(2024, default).Returns([existing]);
        _mapper.Map<BudgetSummaryResponse>(Arg.Any<Budget>())
               .Returns(c => MakeResponse(c.Arg<Budget>()));

        var cmd = new CreateBudgetCommand(2024, "Orçamento Marketing", null);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _repo.Received(1).AddAsync(Arg.Any<Budget>(), default);
        await _uow.Received(1).CommitAsync(default);
    }
}

using AutoMapper;
using FinanceiroApi.Application.Commands.AccountsReceivable.CreateAccountReceivable;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.AccountsReceivable;

public class CreateAccountReceivableCommandHandlerTests
{
    private readonly IAccountReceivableRepository _repo     = Substitute.For<IAccountReceivableRepository>();
    private readonly ICustomerRepository _customerRepo      = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _uow                       = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                        = Substitute.For<IMapper>();
    private readonly INotificationContext _notif            = Substitute.For<INotificationContext>();

    private CreateAccountReceivableCommandHandler CreateHandler() =>
        new(_repo, _customerRepo, _uow, _mapper, _notif);

    private static CreateAccountReceivableCommand MakeCommand(Guid customerId) =>
        new(customerId, "Venda de produtos", 1500m, DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            null, "NF-001", null);

    [Fact]
    public async Task Handle_WithValidCustomer_ShouldCreateReceivableAndReturnResponse()
    {
        var customerId = Guid.NewGuid();
        var cmd        = MakeCommand(customerId);
        var expected   = new AccountReceivableResponse(
            Guid.NewGuid(), customerId, "Cliente", null, null,
            "Venda de produtos", 1500m, 0m, 1500m, "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            null, "Pending", null, null, DateTime.UtcNow, null);

        _customerRepo.ExistsAsync(customerId, default).Returns(true);
        _mapper.Map<AccountReceivableResponse>(Arg.Any<object>()).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _repo.Received(1).AddAsync(Arg.Any<FinanceiroApi.Domain.Entities.AccountReceivable>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldNotifyAndReturnNull()
    {
        var cmd = MakeCommand(Guid.NewGuid());
        _customerRepo.ExistsAsync(cmd.CustomerId, default).Returns(false);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("CustomerId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

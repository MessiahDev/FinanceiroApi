using FinanceiroApi.Application.Commands.ChartOfAccounts.CreateChartOfAccount;
using FinanceiroApi.Application.Commands.ChartOfAccounts.DeactivateChartOfAccount;
using FinanceiroApi.Application.Commands.ChartOfAccounts.UpdateChartOfAccount;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.ChartOfAccounts;

public class CreateChartOfAccountCommandHandlerTests
{
    private readonly IChartOfAccountRepository _repo = Substitute.For<IChartOfAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private CreateChartOfAccountCommandHandler CreateHandler() => new(_repo, _uow);

    private static ChartOfAccount MakeAccount(string code = "1.1.01") =>
        ChartOfAccount.Create(code, "Caixa", null, AccountType.Asset, AccountNature.Debit, true);

    [Fact]
    public async Task Handle_NewCode_ShouldCreateAndReturnId()
    {
        var cmd = new CreateChartOfAccountCommand(
            "1.1.01", "Caixa", null, AccountType.Asset, AccountNature.Debit, true, null);
        _repo.ExistsCodeAsync("1.1.01", null, default).Returns(false);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<ChartOfAccount>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldThrowDuplicateException()
    {
        _repo.ExistsCodeAsync("1.1.01", null, default).Returns(true);
        var cmd = new CreateChartOfAccountCommand(
            "1.1.01", "Caixa", null, AccountType.Asset, AccountNature.Debit, true, null);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<DuplicateChartOfAccountCodeException>();
    }

    [Fact]
    public async Task Handle_WithValidParent_ShouldCreateSuccessfully()
    {
        var parent = MakeAccount("1.1");
        _repo.ExistsCodeAsync("1.1.01", null, default).Returns(false);
        _repo.GetByIdAsync(parent.Id, default).Returns(parent);

        var cmd = new CreateChartOfAccountCommand(
            "1.1.01", "Caixa", null, AccountType.Asset, AccountNature.Debit, true, parent.Id);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithInactiveParent_ShouldThrowDomainException()
    {
        var parent = MakeAccount("1.1");
        parent.Deactivate();
        _repo.ExistsCodeAsync("1.1.01", null, default).Returns(false);
        _repo.GetByIdAsync(parent.Id, default).Returns(parent);

        var cmd = new CreateChartOfAccountCommand(
            "1.1.01", "Caixa", null, AccountType.Asset, AccountNature.Debit, true, parent.Id);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

public class UpdateChartOfAccountCommandHandlerTests
{
    private readonly IChartOfAccountRepository _repo = Substitute.For<IChartOfAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UpdateChartOfAccountCommandHandler CreateHandler() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ExistingAccount_ShouldUpdateSuccessfully()
    {
        var account = ChartOfAccount.Create("1.1.01", "Caixa", null,
            AccountType.Asset, AccountNature.Debit, true);
        _repo.GetByIdAsync(account.Id, default).Returns(account);

        await CreateHandler().Handle(
            new UpdateChartOfAccountCommand(account.Id, "Caixa Geral", "Desc", false), default);

        account.Name.Should().Be("Caixa Geral");
        account.AcceptsEntries.Should().BeFalse();
        await _repo.Received(1).UpdateAsync(account, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowDomainException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var act = () => CreateHandler().Handle(
            new UpdateChartOfAccountCommand(Guid.NewGuid(), "Nome", null, true), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

public class DeactivateChartOfAccountCommandHandlerTests
{
    private readonly IChartOfAccountRepository _repo = Substitute.For<IChartOfAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private DeactivateChartOfAccountCommandHandler CreateHandler() => new(_repo, _uow);

    [Fact]
    public async Task Handle_ActiveAccount_ShouldDeactivateSuccessfully()
    {
        var account = ChartOfAccount.Create("1.1.01", "Caixa", null,
            AccountType.Asset, AccountNature.Debit, true);
        _repo.GetByIdAsync(account.Id, default).Returns(account);

        await CreateHandler().Handle(new DeactivateChartOfAccountCommand(account.Id), default);

        account.IsActive.Should().BeFalse();
        await _repo.Received(1).UpdateAsync(account, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowDomainException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var act = () => CreateHandler().Handle(
            new DeactivateChartOfAccountCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

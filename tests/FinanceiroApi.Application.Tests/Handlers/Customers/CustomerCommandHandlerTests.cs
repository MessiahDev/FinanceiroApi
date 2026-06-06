using AutoMapper;
using FinanceiroApi.Application.Commands.Customers.BlockCustomer;
using FinanceiroApi.Application.Commands.Customers.CreateCustomer;
using FinanceiroApi.Application.Commands.Customers.DeleteCustomer;
using FinanceiroApi.Application.Commands.Customers.UpdateCustomer;
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

namespace FinanceiroApi.Application.Tests.Handlers.Customers;

public class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateCustomerCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static CreateCustomerCommand ValidCommand() => new(
        "Empresa Teste Ltda", "12345678000195",
        PersonType.Company, "contato@empresa.com",
        "(11) 99999-9999", "João Silva", 5000m);

    private static CustomerResponse MakeResponse(Customer c) => new(
        c.Id, c.Name, c.TaxId, c.PersonType.ToString(),
        c.Email.ToString(), c.Phone, c.ContactName,
        c.Status.ToString(), c.CreditLimit.Amount, "BRL",
        DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_NewTaxId_ShouldCreateCustomerAndReturnResponse()
    {
        var cmd = ValidCommand();
        _repo.ExistsByTaxIdAsync(cmd.TaxId, default).Returns(false);
        _mapper.Map<CustomerResponse>(Arg.Any<Customer>())
               .Returns(c => MakeResponse(c.Arg<Customer>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be(cmd.Name);
        await _repo.Received(1).AddAsync(Arg.Any<Customer>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateTaxId_ShouldNotifyAndReturnNull()
    {
        var cmd = ValidCommand();
        _repo.ExistsByTaxIdAsync(cmd.TaxId, default).Returns(true);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("TaxId", Arg.Any<string>());
        await _repo.DidNotReceive().AddAsync(Arg.Any<Customer>(), default);
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class UpdateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private UpdateCustomerCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static Customer MakeCustomer() =>
        Customer.Create("Empresa Teste", "12345678000195",
            PersonType.Company, "old@empresa.com", null, null, 1000m);

    private static CustomerResponse MakeResponse(Customer c) => new(
        c.Id, c.Name, c.TaxId, c.PersonType.ToString(),
        c.Email.ToString(), c.Phone, c.ContactName,
        c.Status.ToString(), c.CreditLimit.Amount, "BRL",
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task Handle_ExistingCustomer_ShouldUpdateAndReturnResponse()
    {
        var customer = MakeCustomer();
        var cmd = new UpdateCustomerCommand(customer.Id, "Novo Nome",
            "novo@empresa.com", "(11) 88888-8888", "Maria");

        _repo.GetByIdAsync(customer.Id, default).Returns(customer);
        _mapper.Map<CustomerResponse>(Arg.Any<Customer>())
               .Returns(c => MakeResponse(c.Arg<Customer>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        customer.Name.Should().Be("Novo Nome");
        await _repo.Received(1).UpdateAsync(customer, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NonExistentCustomer_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var cmd = new UpdateCustomerCommand(Guid.NewGuid(), "Nome", "e@e.com", null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class DeleteCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private DeleteCustomerCommandHandler CreateHandler() => new(_repo, _uow, _notif);

    private static Customer MakeCustomer() =>
        Customer.Create("Empresa Teste", "12345678000195",
            PersonType.Company, "contato@empresa.com", null, null, 0m);

    [Fact]
    public async Task Handle_ExistingCustomer_ShouldDeleteAndReturnTrue()
    {
        var customer = MakeCustomer();
        _repo.GetByIdAsync(customer.Id, default).Returns(customer);

        var result = await CreateHandler().Handle(new DeleteCustomerCommand(customer.Id), default);

        result.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(customer, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NonExistentCustomer_ShouldNotifyAndReturnFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new DeleteCustomerCommand(Guid.NewGuid()), default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class BlockCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private BlockCustomerCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static Customer MakeCustomer() =>
        Customer.Create("Empresa Teste", "12345678000195",
            PersonType.Company, "contato@empresa.com", null, null, 1000m);

    private static CustomerResponse MakeResponse(Customer c) => new(
        c.Id, c.Name, c.TaxId, c.PersonType.ToString(),
        c.Email.ToString(), c.Phone, c.ContactName,
        c.Status.ToString(), c.CreditLimit.Amount, "BRL",
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task Handle_ActiveCustomer_ShouldBlockAndReturnResponse()
    {
        var customer = MakeCustomer();
        var cmd = new BlockCustomerCommand(customer.Id, "Inadimplência");

        _repo.GetByIdAsync(customer.Id, default).Returns(customer);
        _mapper.Map<CustomerResponse>(Arg.Any<Customer>())
               .Returns(c => MakeResponse(c.Arg<Customer>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        customer.Status.Should().Be(CustomerStatus.Blocked);
        await _repo.Received(1).UpdateAsync(customer, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NonExistentCustomer_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new BlockCustomerCommand(Guid.NewGuid(), "Motivo"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_AlreadyBlockedCustomer_ShouldThrowDomainException()
    {
        var customer = MakeCustomer();
        customer.Block("primeiro bloqueio");
        _repo.GetByIdAsync(customer.Id, default).Returns(customer);

        var act = () => CreateHandler().Handle(
            new BlockCustomerCommand(customer.Id, "segundo bloqueio"), default);

        await act.Should().ThrowAsync<Exception>();
    }
}

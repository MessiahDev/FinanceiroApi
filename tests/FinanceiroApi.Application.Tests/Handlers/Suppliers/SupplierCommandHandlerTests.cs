using AutoMapper;
using FinanceiroApi.Application.Commands.Suppliers.BlockSupplier;
using FinanceiroApi.Application.Commands.Suppliers.DeleteSupplier;
using FinanceiroApi.Application.Commands.Suppliers.UpdateSupplier;
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

namespace FinanceiroApi.Application.Tests.Handlers.Suppliers;

public class BlockSupplierCommandHandlerTests
{
    private readonly ISupplierRepository _repo      = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                = Substitute.For<IMapper>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private BlockSupplierCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    private static Supplier MakeSupplier() =>
        Supplier.Create("Fornecedor", "12345678000195", PersonType.Company, "f@email.com");

    private static SupplierResponse MakeResponse(Supplier s) =>
        new(s.Id, s.Name, s.TaxId, s.PersonType.ToString(), s.Email.Value,
            null, null, "Blocked", null, null, null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_WithValidSupplier_ShouldBlockAndReturnResponse()
    {
        var supplier = MakeSupplier();
        var cmd      = new BlockSupplierCommand(supplier.Id, "fraude");
        _repo.GetByIdAsync(supplier.Id, default).Returns(supplier);
        _mapper.Map<SupplierResponse>(supplier).Returns(MakeResponse(supplier));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentSupplier_ShouldReturnNull()
    {
        var cmd = new BlockSupplierCommand(Guid.NewGuid(), "fraude");
        _repo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class DeleteSupplierCommandHandlerTests
{
    private readonly ISupplierRepository _repo      = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private DeleteSupplierCommandHandler CreateHandler() =>
        new(_repo, _uow, _notif);

    [Fact]
    public async Task Handle_WithValidSupplier_ShouldDeleteAndReturnTrue()
    {
        var supplier = Supplier.Create("Fornecedor", "12345678000195", PersonType.Company, "f@email.com");
        var cmd      = new DeleteSupplierCommand(supplier.Id);
        _repo.GetByIdAsync(supplier.Id, default).Returns(supplier);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(supplier, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentSupplier_ShouldReturnFalse()
    {
        var cmd = new DeleteSupplierCommand(Guid.NewGuid());
        _repo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class UpdateSupplierCommandHandlerTests
{
    private readonly ISupplierRepository _repo      = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper                = Substitute.For<IMapper>();
    private readonly INotificationContext _notif    = Substitute.For<INotificationContext>();

    private UpdateSupplierCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_WithValidSupplier_ShouldUpdateAndReturnResponse()
    {
        var supplier = Supplier.Create("Fornecedor", "12345678000195", PersonType.Company, "f@email.com");
        var cmd      = new UpdateSupplierCommand(supplier.Id, "Novo Nome", "novo@email.com", null, null);
        var expected = new SupplierResponse(supplier.Id, "Novo Nome", supplier.TaxId,
            supplier.PersonType.ToString(), "novo@email.com", null, null, "Active",
            null, null, null, null, DateTime.UtcNow, null);

        _repo.GetByIdAsync(supplier.Id, default).Returns(supplier);
        _mapper.Map<SupplierResponse>(supplier).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Novo Nome");
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentSupplier_ShouldReturnNull()
    {
        var cmd = new UpdateSupplierCommand(Guid.NewGuid(), "Nome", "a@b.com", null, null);
        _repo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

using AutoMapper;
using FinanceiroApi.Application.Commands.Suppliers.CreateSupplier;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Suppliers;

public class CreateSupplierCommandHandlerTests
{
    private readonly ISupplierRepository _repo = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateSupplierCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static CreateSupplierCommand MakeCommand(string taxId = "12345678000195") =>
        new("Fornecedor Ltda", taxId, PersonType.Company, "fornecedor@email.com", "11999999999", "Joao");

    [Fact]
    public async Task Handle_WithNewTaxId_ShouldCreateSupplierAndReturnResponse()
    {
        var cmd = MakeCommand();
        var expected = new SupplierResponse(Guid.NewGuid(), cmd.Name, cmd.TaxId, "Company",
            cmd.Email, cmd.Phone, cmd.ContactName, "Active", null, null, null, null, DateTime.UtcNow, null);

        _repo.ExistsByTaxIdAsync(cmd.TaxId, default).Returns(false);
        _mapper.Map<SupplierResponse>(Arg.Any<object>()).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Fornecedor Ltda");
        await _repo.Received(1).AddAsync(Arg.Any<FinanceiroApi.Domain.Entities.Supplier>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithDuplicateTaxId_ShouldNotifyAndReturnNull()
    {
        var cmd = MakeCommand();
        _repo.ExistsByTaxIdAsync(cmd.TaxId, default).Returns(true);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("TaxId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

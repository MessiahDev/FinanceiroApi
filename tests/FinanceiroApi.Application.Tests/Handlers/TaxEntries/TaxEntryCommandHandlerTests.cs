using AutoMapper;
using FinanceiroApi.Application.Commands.TaxEntries.CancelTaxEntry;
using FinanceiroApi.Application.Commands.TaxEntries.CreateTaxEntry;
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

namespace FinanceiroApi.Application.Tests.Handlers.TaxEntries;

public class CreateTaxEntryCommandHandlerTests
{
    private readonly ITaxEntryRepository _repo = Substitute.For<ITaxEntryRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateTaxEntryCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static TaxEntry MakeEntry() => TaxEntry.Create(
        TaxType.ISS, "ISS Junho", 10000m, 5m,
        new DateOnly(2024, 6, 1), new DateOnly(2024, 7, 15));

    private static TaxEntryResponse MakeResponse(TaxEntry e) => new(
        e.Id, e.TaxType.ToString(), e.Description, e.BaseAmount.Amount,
        e.Rate, e.TaxAmount.Amount, "BRL", e.Competence, e.DueDate,
        e.Status.ToString(), e.ReferenceDocument, e.ReferenceDocumentId,
        e.CostCenterId, null, e.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_ValidEntry_ShouldCreateAndReturnResponse()
    {
        var entry = MakeEntry();
        var cmd = new CreateTaxEntryCommand(
            TaxType.ISS, "ISS Junho", 10000m, 5m,
            new DateOnly(2024, 6, 1), new DateOnly(2024, 7, 15),
            null, null, null, null);

        _repo.GetWithPaymentsAsync(Arg.Any<Guid>(), default).Returns(entry);
        _mapper.Map<TaxEntryResponse>(Arg.Any<TaxEntry>())
               .Returns(c => MakeResponse(c.Arg<TaxEntry>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _repo.Received(1).AddAsync(Arg.Any<TaxEntry>(), default);
        await _uow.Received(1).CommitAsync(default);
    }
}

public class CancelTaxEntryCommandHandlerTests
{
    private readonly ITaxEntryRepository _repo = Substitute.For<ITaxEntryRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelTaxEntryCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static TaxEntry MakeEntry() => TaxEntry.Create(
        TaxType.ISS, "ISS Junho", 10000m, 5m,
        new DateOnly(2024, 6, 1), new DateOnly(2024, 7, 15));

    private static TaxEntryResponse MakeResponse(TaxEntry e) => new(
        e.Id, e.TaxType.ToString(), e.Description, e.BaseAmount.Amount,
        e.Rate, e.TaxAmount.Amount, "BRL", e.Competence, e.DueDate,
        e.Status.ToString(), e.ReferenceDocument, e.ReferenceDocumentId,
        e.CostCenterId, null, e.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_ExistingEntry_ShouldCancelAndReturnResponse()
    {
        var entry = MakeEntry();
        _repo.GetWithPaymentsAsync(entry.Id, default).Returns(entry);
        _mapper.Map<TaxEntryResponse>(Arg.Any<TaxEntry>())
               .Returns(c => MakeResponse(c.Arg<TaxEntry>()));

        var result = await CreateHandler().Handle(
            new CancelTaxEntryCommand(entry.Id, "Erro"), default);

        result.Should().NotBeNull();
        entry.Status.Should().Be(TaxEntryStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(entry, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithPaymentsAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CancelTaxEntryCommand(Guid.NewGuid(), "Erro"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_PaidEntry_ShouldThrowDomainException()
    {
        var entry = MakeEntry();
        entry.MarkAsPaid();
        _repo.GetWithPaymentsAsync(entry.Id, default).Returns(entry);

        var act = () => CreateHandler().Handle(
            new CancelTaxEntryCommand(entry.Id, "Erro"), default);

        await act.Should().ThrowAsync<Exception>();
    }
}

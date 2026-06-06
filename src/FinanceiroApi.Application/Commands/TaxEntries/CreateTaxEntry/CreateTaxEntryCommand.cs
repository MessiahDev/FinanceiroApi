using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.TaxEntries.CreateTaxEntry;

public record CreateTaxEntryCommand(
    TaxType TaxType,
    string Description,
    decimal BaseAmount,
    decimal Rate,
    DateOnly Competence,
    DateOnly DueDate,
    Guid? CostCenterId,
    string? ReferenceDocument,
    Guid? ReferenceDocumentId,
    string? Notes) : IRequest<TaxEntryResponse>;

public class CreateTaxEntryCommandHandler : IRequestHandler<CreateTaxEntryCommand, TaxEntryResponse>
{
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateTaxEntryCommandHandler(
        ITaxEntryRepository taxEntryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _taxEntryRepository = taxEntryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TaxEntryResponse> Handle(CreateTaxEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = TaxEntry.Create(
            request.TaxType,
            request.Description,
            request.BaseAmount,
            request.Rate,
            request.Competence,
            request.DueDate,
            request.CostCenterId,
            request.ReferenceDocument,
            request.ReferenceDocumentId,
            request.Notes);

        await _taxEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _taxEntryRepository.GetWithPaymentsAsync(entry.Id, cancellationToken);
        return _mapper.Map<TaxEntryResponse>(result!);
    }
}

using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.TaxEntries.CancelTaxEntry;

public record CancelTaxEntryCommand(Guid Id, string Reason) : IRequest<TaxEntryResponse>;

public class CancelTaxEntryCommandHandler : IRequestHandler<CancelTaxEntryCommand, TaxEntryResponse>
{
	private readonly ITaxEntryRepository _taxEntryRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly INotificationContext _notifications;

	public CancelTaxEntryCommandHandler(
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

	public async Task<TaxEntryResponse> Handle(CancelTaxEntryCommand request, CancellationToken cancellationToken)
	{
		var entry = await _taxEntryRepository.GetWithPaymentsAsync(request.Id, cancellationToken);
		if (entry is null)
		{
			_notifications.AddNotification("Id", "Lançamento fiscal não encontrado.");
			return null!;
		}

		entry.Cancel(request.Reason);

		await _taxEntryRepository.UpdateAsync(entry, cancellationToken);
		await _unitOfWork.CommitAsync(cancellationToken);

		return _mapper.Map<TaxEntryResponse>(entry);
	}
}

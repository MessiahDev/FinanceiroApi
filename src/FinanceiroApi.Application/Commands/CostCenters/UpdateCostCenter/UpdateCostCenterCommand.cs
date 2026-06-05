using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.CostCenters.UpdateCostCenter;

public record UpdateCostCenterCommand(
	Guid Id,
	string Code,
	string Name,
	string? Description,
	Guid? ManagerId) : IRequest<CostCenterResponse?>;

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, CostCenterResponse?>
{
	private readonly ICostCenterRepository _costCenterRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly INotificationContext _notifications;

	public UpdateCostCenterCommandHandler(
		ICostCenterRepository costCenterRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper,
		INotificationContext notifications)
	{
		_costCenterRepository = costCenterRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_notifications = notifications;
	}

	public async Task<CostCenterResponse?> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
	{
		var costCenter = await _costCenterRepository.GetByIdAsync(request.Id, cancellationToken);
		if (costCenter is null)
		{
			_notifications.AddNotification("Id", "Centro de custo não encontrado.");
			return null;
		}

		costCenter.Update(request.Code, request.Name, request.Description, request.ManagerId);

		await _costCenterRepository.UpdateAsync(costCenter, cancellationToken);
		await _unitOfWork.CommitAsync(cancellationToken);

		return _mapper.Map<CostCenterResponse>(costCenter);
	}
}

using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.CostCenters.CreateCostCenter;

public record CreateCostCenterCommand(
    string Code,
    string Name,
    decimal AnnualBudget,
    Guid? ParentId,
    Guid? ManagerId,
    string? Description) : IRequest<CostCenterResponse>;

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, CostCenterResponse>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateCostCenterCommandHandler(
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

    public async Task<CostCenterResponse> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _costCenterRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
        {
            _notifications.AddNotification("Code", "Já existe um centro de custo com este código.");
            return null!;
        }

        var costCenter = CostCenter.Create(
            request.Code,
            request.Name,
            request.AnnualBudget,
            request.ParentId,
            request.ManagerId,
            request.Description);

        await _costCenterRepository.AddAsync(costCenter, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<CostCenterResponse>(costCenter);
    }
}

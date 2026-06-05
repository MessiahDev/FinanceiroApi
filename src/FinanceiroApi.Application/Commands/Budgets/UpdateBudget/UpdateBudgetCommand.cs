using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.Budgets.UpdateBudget;

public record UpdateBudgetCommand(
    Guid Id,
    Guid CostCenterId,
    string Category,
    decimal PlannedAmount) : IRequest<BudgetResponse?>;

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetResponse?>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public UpdateBudgetCommandHandler(
        IBudgetRepository budgetRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BudgetResponse?> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetWithItemsAsync(request.Id, cancellationToken);
        if (budget is null)
        {
            _notifications.AddNotification("Id", "Orçamento não encontrado.");
            return null;
        }

        budget.AddItem(request.CostCenterId, request.Category, request.PlannedAmount);

        await _unitOfWork.CommitAsync(cancellationToken);

        var persisted = await _budgetRepository.GetWithItemsAsync(request.Id, cancellationToken, tracking: false);
        return _mapper.Map<BudgetResponse>(persisted);
    }
}
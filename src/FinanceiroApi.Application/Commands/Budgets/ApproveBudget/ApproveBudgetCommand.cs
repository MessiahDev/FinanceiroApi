using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.Budgets.ApproveBudget;

public record ApproveBudgetCommand(Guid Id, Guid ApprovedBy) : IRequest<BudgetSummaryResponse?>;

public class ApproveBudgetCommandHandler : IRequestHandler<ApproveBudgetCommand, BudgetSummaryResponse?>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public ApproveBudgetCommandHandler(
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

    public async Task<BudgetSummaryResponse?> Handle(ApproveBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetWithItemsAsync(request.Id, cancellationToken);
        if (budget is null)
        {
            _notifications.AddNotification("Id", "Orçamento não encontrado.");
            return null;
        }

        budget.Approve(request.ApprovedBy);

        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<BudgetSummaryResponse>(budget);
    }
}
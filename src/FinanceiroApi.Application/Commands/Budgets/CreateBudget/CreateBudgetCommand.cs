using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Budgets.CreateBudget;

public record CreateBudgetCommand(
    int Year,
    string Name,
    string? Description) : IRequest<BudgetSummaryResponse>;

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetSummaryResponse>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateBudgetCommandHandler(
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

    public async Task<BudgetSummaryResponse> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var existing = await _budgetRepository.GetByYearAsync(request.Year, cancellationToken);
        if (existing.Any(b => b.Name == request.Name.Trim()))
        {
            _notifications.AddNotification("Name", "Já existe um orçamento com este nome para o ano informado.");
            return null!;
        }

        var budget = Budget.Create(request.Year, request.Name, request.Description);

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<BudgetSummaryResponse>(budget);
    }
}
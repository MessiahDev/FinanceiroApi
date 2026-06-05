using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Budgets.GetBudgets;

public record GetBudgetsQuery(int? Year = null, BudgetStatus? Status = null) : IRequest<IReadOnlyList<BudgetSummaryResponse>>;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, IReadOnlyList<BudgetSummaryResponse>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IMapper _mapper;

    public GetBudgetsQueryHandler(IBudgetRepository budgetRepository, IMapper mapper)
    {
        _budgetRepository = budgetRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BudgetSummaryResponse>> Handle(
        GetBudgetsQuery request,
        CancellationToken cancellationToken)
    {
        var budgets = request.Year.HasValue
            ? await _budgetRepository.GetByYearAsync(request.Year.Value, cancellationToken)
            : request.Status.HasValue
                ? await _budgetRepository.GetByStatusAsync(request.Status.Value, cancellationToken)
                : await _budgetRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<BudgetSummaryResponse>>(budgets);
    }
}
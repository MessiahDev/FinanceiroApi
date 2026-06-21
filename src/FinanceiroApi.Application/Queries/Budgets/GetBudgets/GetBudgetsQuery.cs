using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Budgets.GetBudgets;

public record GetBudgetsQuery(int? Year = null, BudgetStatus? Status = null, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<BudgetSummaryResponse>>;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, PagedResult<BudgetSummaryResponse>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IMapper _mapper;

    public GetBudgetsQueryHandler(IBudgetRepository budgetRepository, IMapper mapper)
    {
        _budgetRepository = budgetRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<BudgetSummaryResponse>> Handle(
        GetBudgetsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _budgetRepository.GetPagedAsync(
            request.Year, request.Status, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<BudgetSummaryResponse>(
            _mapper.Map<IReadOnlyList<BudgetSummaryResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

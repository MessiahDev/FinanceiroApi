using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Queries.Budgets.GetBudgetById;

public record GetBudgetByIdQuery(Guid Id) : IRequest<BudgetResponse?>;

public class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetResponse?>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IMapper _mapper;

    public GetBudgetByIdQueryHandler(IBudgetRepository budgetRepository, IMapper mapper)
    {
        _budgetRepository = budgetRepository;
        _mapper = mapper;
    }

    public async Task<BudgetResponse?> Handle(
        GetBudgetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetWithItemsAsync(request.Id, cancellationToken);
        return budget is null ? null : _mapper.Map<BudgetResponse>(budget);
    }
}
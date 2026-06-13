using MediatR;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;

namespace FinanceiroApi.Application.Queries.Budgets.GetBudgetVsActual;

public record GetBudgetVsActualQuery(Guid BudgetId) : IRequest<BudgetVsActualResponse?>;

public record BudgetVsActualItemResponse(
    Guid CostCenterId,
    string? CostCenterName,
    string Category,
    decimal PlannedAmount,
    decimal ActualPaid,
    decimal ActualReceived,
    decimal Variance);

public record BudgetVsActualResponse(
    Guid BudgetId,
    int Year,
    string Name,
    decimal TotalPlanned,
    decimal TotalActualPaid,
    decimal TotalActualReceived,
    IReadOnlyList<BudgetVsActualItemResponse> Items);

public class GetBudgetVsActualQueryHandler : IRequestHandler<GetBudgetVsActualQuery, BudgetVsActualResponse?>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IAccountPayableRepository _payableRepository;
    private readonly IAccountReceivableRepository _receivableRepository;

    public GetBudgetVsActualQueryHandler(
        IBudgetRepository budgetRepository,
        IAccountPayableRepository payableRepository,
        IAccountReceivableRepository receivableRepository)
    {
        _budgetRepository = budgetRepository;
        _payableRepository = payableRepository;
        _receivableRepository = receivableRepository;
    }

    public async Task<BudgetVsActualResponse?> Handle(GetBudgetVsActualQuery request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetWithItemsAsync(request.BudgetId, cancellationToken);
        if (budget is null) return null;

        var allPayables = await _payableRepository.GetAllAsync(cancellationToken);
        var allReceivables = await _receivableRepository.GetAllAsync(cancellationToken);

        var payablesInYear = allPayables
            .Where(p => p.CostCenterId.HasValue
                && (p.Status == AccountPayableStatus.Paid || p.Status == AccountPayableStatus.PartiallyPaid)
                && p.PaymentDate.HasValue
                && p.PaymentDate.Value.Year == budget.Year)
            .ToList();

        var receivablesInYear = allReceivables
            .Where(r => r.CostCenterId.HasValue
                && (r.Status == AccountReceivableStatus.Received || r.Status == AccountReceivableStatus.PartiallyReceived))
            .ToList();

        var items = new List<BudgetVsActualItemResponse>();
        decimal totalActualPaid = 0;
        decimal totalActualReceived = 0;

        foreach (var item in budget.Items)
        {
            var paid = payablesInYear
                .Where(p => p.CostCenterId == item.CostCenterId)
                .Sum(p => p.PaidAmount.Amount);

            var received = receivablesInYear
                .Where(r => r.CostCenterId == item.CostCenterId)
                .Sum(r => r.ReceivedAmount.Amount);

            totalActualPaid += paid;
            totalActualReceived += received;

            items.Add(new BudgetVsActualItemResponse(
                item.CostCenterId,
                item.CostCenter?.Name,
                item.Category,
                item.PlannedAmount.Amount,
                paid,
                received,
                item.PlannedAmount.Amount - paid));
        }

        return new BudgetVsActualResponse(
            budget.Id,
            budget.Year,
            budget.Name,
            budget.TotalPlanned.Amount,
            totalActualPaid,
            totalActualReceived,
            items);
    }
}

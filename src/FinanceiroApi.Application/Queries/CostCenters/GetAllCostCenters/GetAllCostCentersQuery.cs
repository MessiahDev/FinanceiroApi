using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Application.DTOs.Response;
namespace FinanceiroApi.Application.Queries.CostCenters.GetAllCostCenters;

public record GetAllCostCentersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<CostCenterResponse>>;
public class GetAllCostCentersQueryHandler : IRequestHandler<GetAllCostCentersQuery, PagedResult<CostCenterResponse>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IMapper _mapper;
    public GetAllCostCentersQueryHandler(ICostCenterRepository costCenterRepository, IMapper mapper)
    {
        _costCenterRepository = costCenterRepository;
        _mapper = mapper;
    }
    public async Task<PagedResult<CostCenterResponse>> Handle(
        GetAllCostCentersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _costCenterRepository.GetActivePagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<CostCenterResponse>(
            _mapper.Map<IReadOnlyList<CostCenterResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

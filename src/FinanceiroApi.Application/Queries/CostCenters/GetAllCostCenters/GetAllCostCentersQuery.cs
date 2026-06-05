using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Queries.CostCenters.GetAllCostCenters;

public record GetAllCostCentersQuery() : IRequest<IReadOnlyList<CostCenterResponse>>;

public class GetAllCostCentersQueryHandler : IRequestHandler<GetAllCostCentersQuery, IReadOnlyList<CostCenterResponse>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IMapper _mapper;

    public GetAllCostCentersQueryHandler(ICostCenterRepository costCenterRepository, IMapper mapper)
    {
        _costCenterRepository = costCenterRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CostCenterResponse>> Handle(
        GetAllCostCentersQuery request,
        CancellationToken cancellationToken)
    {
        var costCenters = await _costCenterRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CostCenterResponse>>(costCenters);
    }
}

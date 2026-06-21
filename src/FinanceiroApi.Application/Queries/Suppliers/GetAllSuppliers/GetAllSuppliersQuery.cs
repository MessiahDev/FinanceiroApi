using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Suppliers.GetAllSuppliers;

public record GetAllSuppliersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<SupplierSummaryResponse>>;

public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, PagedResult<SupplierSummaryResponse>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public GetAllSuppliersQueryHandler(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<SupplierSummaryResponse>> Handle(
        GetAllSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _supplierRepository.GetActivePagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<SupplierSummaryResponse>(
            _mapper.Map<IReadOnlyList<SupplierSummaryResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

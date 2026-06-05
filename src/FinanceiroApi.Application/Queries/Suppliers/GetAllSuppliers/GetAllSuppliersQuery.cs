using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Suppliers.GetAllSuppliers;

public record GetAllSuppliersQuery() : IRequest<IReadOnlyList<SupplierSummaryResponse>>;

public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, IReadOnlyList<SupplierSummaryResponse>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public GetAllSuppliersQueryHandler(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SupplierSummaryResponse>> Handle(
        GetAllSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await _supplierRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SupplierSummaryResponse>>(suppliers);
    }
}

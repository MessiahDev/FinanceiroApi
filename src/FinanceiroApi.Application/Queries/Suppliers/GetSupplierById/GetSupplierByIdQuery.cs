using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Suppliers.GetSupplierById;

public record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierResponse?>;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierResponse?>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public GetSupplierByIdQueryHandler(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<SupplierResponse?> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);
        return supplier is null ? null : _mapper.Map<SupplierResponse>(supplier);
    }
}

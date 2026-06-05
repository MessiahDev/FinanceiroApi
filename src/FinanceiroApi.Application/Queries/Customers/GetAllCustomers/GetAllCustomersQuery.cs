using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Customers.GetAllCustomers;

public record GetAllCustomersQuery() : IRequest<IReadOnlyList<CustomerSummaryResponse>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerSummaryResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CustomerSummaryResponse>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CustomerSummaryResponse>>(customers);
    }
}

using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Customers.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerResponse?>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponse?>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<CustomerResponse?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        return customer is null ? null : _mapper.Map<CustomerResponse>(customer);
    }
}

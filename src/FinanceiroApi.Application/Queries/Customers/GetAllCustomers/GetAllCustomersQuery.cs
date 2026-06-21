using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Customers.GetAllCustomers;

public record GetAllCustomersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<CustomerSummaryResponse>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerSummaryResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CustomerSummaryResponse>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _customerRepository.GetActivePagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<CustomerSummaryResponse>(
            _mapper.Map<IReadOnlyList<CustomerSummaryResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;
namespace FinanceiroApi.Application.Queries.AccountsReceivable.GetAccountsReceivable;

public record GetAccountsReceivableQuery(
    Guid? CustomerId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<AccountReceivableResponse>>;
public class GetAccountsReceivableQueryHandler : IRequestHandler<GetAccountsReceivableQuery, PagedResult<AccountReceivableResponse>>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly IMapper _mapper;
    public GetAccountsReceivableQueryHandler(IAccountReceivableRepository accountReceivableRepository, IMapper mapper)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _mapper = mapper;
    }
    public async Task<PagedResult<AccountReceivableResponse>> Handle(
        GetAccountsReceivableQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _accountReceivableRepository.GetPagedAsync(
            request.CustomerId, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<AccountReceivableResponse>(
            _mapper.Map<IReadOnlyList<AccountReceivableResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

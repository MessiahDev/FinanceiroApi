using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;
namespace FinanceiroApi.Application.Queries.AccountsPayable.GetAccountsPayable;

public record GetAccountsPayableQuery(
    AccountPayableStatus? Status = null,
    Guid? SupplierId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<AccountPayableResponse>>;
public class GetAccountsPayableQueryHandler : IRequestHandler<GetAccountsPayableQuery, PagedResult<AccountPayableResponse>>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly IMapper _mapper;
    public GetAccountsPayableQueryHandler(IAccountPayableRepository accountPayableRepository, IMapper mapper)
    {
        _accountPayableRepository = accountPayableRepository;
        _mapper = mapper;
    }
    public async Task<PagedResult<AccountPayableResponse>> Handle(
        GetAccountsPayableQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _accountPayableRepository.GetPagedAsync(
            request.Status, request.SupplierId, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<AccountPayableResponse>(
            _mapper.Map<IReadOnlyList<AccountPayableResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}

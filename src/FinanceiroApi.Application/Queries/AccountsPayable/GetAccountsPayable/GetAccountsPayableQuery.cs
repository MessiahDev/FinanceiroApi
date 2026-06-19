using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.AccountsPayable.GetAccountsPayable;

public record GetAccountsPayableQuery(
    AccountPayableStatus? Status = null,
    Guid? SupplierId = null) : IRequest<IReadOnlyList<AccountPayableResponse>>;

public class GetAccountsPayableQueryHandler : IRequestHandler<GetAccountsPayableQuery, IReadOnlyList<AccountPayableResponse>>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly IMapper _mapper;

    public GetAccountsPayableQueryHandler(IAccountPayableRepository accountPayableRepository, IMapper mapper)
    {
        _accountPayableRepository = accountPayableRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AccountPayableResponse>> Handle(
        GetAccountsPayableQuery request,
        CancellationToken cancellationToken)
    {
        var payables = request.Status.HasValue
            ? await _accountPayableRepository.GetByStatusAsync(request.Status.Value, cancellationToken)
            : request.SupplierId.HasValue
                ? await _accountPayableRepository.GetBySupplierAsync(request.SupplierId.Value, cancellationToken)
                : await _accountPayableRepository.GetAllWithDetailsAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<AccountPayableResponse>>(payables);
    }
}

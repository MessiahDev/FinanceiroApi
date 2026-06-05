using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.AccountsPayable.GetAccountPayableById;

public record GetAccountPayableByIdQuery(Guid Id) : IRequest<AccountPayableResponse?>;

public class GetAccountPayableByIdQueryHandler : IRequestHandler<GetAccountPayableByIdQuery, AccountPayableResponse?>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly IMapper _mapper;

    public GetAccountPayableByIdQueryHandler(IAccountPayableRepository accountPayableRepository, IMapper mapper)
    {
        _accountPayableRepository = accountPayableRepository;
        _mapper = mapper;
    }

    public async Task<AccountPayableResponse?> Handle(
        GetAccountPayableByIdQuery request,
        CancellationToken cancellationToken)
    {
        var payable = await _accountPayableRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        return payable is null ? null : _mapper.Map<AccountPayableResponse>(payable);
    }
}

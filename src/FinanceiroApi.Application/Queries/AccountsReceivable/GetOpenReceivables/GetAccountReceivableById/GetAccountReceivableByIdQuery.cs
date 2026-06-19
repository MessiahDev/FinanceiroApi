using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.AccountsReceivable.GetAccountReceivableById;

public record GetAccountReceivableByIdQuery(Guid Id) : IRequest<AccountReceivableResponse?>;

public class GetAccountReceivableByIdQueryHandler : IRequestHandler<GetAccountReceivableByIdQuery, AccountReceivableResponse?>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly IMapper _mapper;

    public GetAccountReceivableByIdQueryHandler(IAccountReceivableRepository accountReceivableRepository, IMapper mapper)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _mapper = mapper;
    }

    public async Task<AccountReceivableResponse?> Handle(
        GetAccountReceivableByIdQuery request,
        CancellationToken cancellationToken)
    {
        var receivable = await _accountReceivableRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        return receivable is null ? null : _mapper.Map<AccountReceivableResponse>(receivable);
    }
}

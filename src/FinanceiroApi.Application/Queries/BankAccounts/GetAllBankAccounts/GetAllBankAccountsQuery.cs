using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.BankAccounts.GetAllBankAccounts;

public record GetAllBankAccountsQuery() : IRequest<IReadOnlyList<BankAccountResponse>>;

public class GetAllBankAccountsQueryHandler : IRequestHandler<GetAllBankAccountsQuery, IReadOnlyList<BankAccountResponse>>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;

    public GetAllBankAccountsQueryHandler(IBankAccountRepository bankAccountRepository, IMapper mapper)
    {
        _bankAccountRepository = bankAccountRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BankAccountResponse>> Handle(
        GetAllBankAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await _bankAccountRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<BankAccountResponse>>(accounts);
    }
}

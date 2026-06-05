using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Queries.BankAccounts.GetBankAccountById;

public record GetBankAccountByIdQuery(Guid Id) : IRequest<BankAccountResponse?>;

public class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountResponse?>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;

    public GetBankAccountByIdQueryHandler(IBankAccountRepository bankAccountRepository, IMapper mapper)
    {
        _bankAccountRepository = bankAccountRepository;
        _mapper = mapper;
    }

    public async Task<BankAccountResponse?> Handle(
        GetBankAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var account = await _bankAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        return account is null ? null : _mapper.Map<BankAccountResponse>(account);
    }
}

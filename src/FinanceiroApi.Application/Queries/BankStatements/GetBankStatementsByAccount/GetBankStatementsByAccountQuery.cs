using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Entities;
using MediatR;

namespace FinanceiroApi.Application.Queries.BankStatements.GetBankStatementsByAccount;

public record GetBankStatementsByAccountQuery(
    Guid? BankAccountId,
    DateOnly? From,
    DateOnly? To) : IRequest<IReadOnlyList<BankStatementSummaryResponse>>;

public class GetBankStatementsByAccountQueryHandler
    : IRequestHandler<GetBankStatementsByAccountQuery, IReadOnlyList<BankStatementSummaryResponse>>
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IMapper _mapper;

    public GetBankStatementsByAccountQueryHandler(IBankStatementRepository bankStatementRepository, IMapper mapper)
    {
        _bankStatementRepository = bankStatementRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BankStatementSummaryResponse>> Handle(
    GetBankStatementsByAccountQuery request,
    CancellationToken cancellationToken)
    {
        var bankAccountId = request.BankAccountId!.Value;

        IReadOnlyList<BankStatement> statements;

        if (request.From.HasValue && request.To.HasValue)
        {
            statements = await _bankStatementRepository.GetByPeriodAsync(
                bankAccountId,
                request.From.Value,
                request.To.Value,
                cancellationToken);
        }
        else
        {
            statements = await _bankStatementRepository.GetByBankAccountAsync(
                bankAccountId,
                cancellationToken);
        }

        return _mapper.Map<IReadOnlyList<BankStatementSummaryResponse>>(statements);
    }
}

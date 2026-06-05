using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.Accounting.GetTrialBalance;

public record GetTrialBalanceQuery(Guid AccountingPeriodId) : IRequest<TrialBalanceResponse>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceResponse>
{
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IJournalEntryRepository _entryRepository;
    private readonly IChartOfAccountRepository _accountRepository;

    public GetTrialBalanceQueryHandler(
        IAccountingPeriodRepository periodRepository,
        IJournalEntryRepository entryRepository,
        IChartOfAccountRepository accountRepository)
    {
        _periodRepository = periodRepository;
        _entryRepository = entryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<TrialBalanceResponse> Handle(
        GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var period = await _periodRepository.GetByIdAsync(request.AccountingPeriodId, cancellationToken)
            ?? throw new DomainException($"Período contábil '{request.AccountingPeriodId}' não encontrado.");

        var entries = await _entryRepository.GetPostedEntriesAsync(request.AccountingPeriodId, cancellationToken);
        var accounts = await _accountRepository.GetActiveAccountsAsync(cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id);

        var lines = entries
            .SelectMany(e => e.Lines)
            .GroupBy(l => l.ChartOfAccountId)
            .Select(g =>
            {
                var account = accountDict.TryGetValue(g.Key, out var acc) ? acc : null;
                var debits = g.Where(l => l.DebitCredit == DebitCredit.Debit).Sum(l => l.Amount);
                var credits = g.Where(l => l.DebitCredit == DebitCredit.Credit).Sum(l => l.Amount);
                var balance = account?.AccountNature == AccountNature.Debit
                    ? debits - credits
                    : credits - debits;

                return new TrialBalanceLineResponse(
                    g.Key,
                    account?.Code ?? "?",
                    account?.Name ?? "Conta não encontrada",
                    account?.AccountType ?? AccountType.Asset,
                    debits,
                    credits,
                    Math.Abs(balance),
                    balance >= 0
                        ? (account?.AccountNature ?? AccountNature.Debit)
                        : (account?.AccountNature == AccountNature.Debit ? AccountNature.Credit : AccountNature.Debit)
                );
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        return new TrialBalanceResponse(
            period.Id,
            period.Name,
            DateTime.UtcNow,
            lines.Sum(l => l.TotalDebits),
            lines.Sum(l => l.TotalCredits),
            lines);
    }
}

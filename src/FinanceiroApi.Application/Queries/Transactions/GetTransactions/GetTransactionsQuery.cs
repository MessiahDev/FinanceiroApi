using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Enums;
using MediatR;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.Transactions.GetTransactions;

public record GetTransactionsQuery(
    Guid? EmployeeId,
    Guid? BankAccountId,
    TransactionType? Type,
    TransactionStatus? Status,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<TransactionResponse>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TransactionResponse>> Handle(
    GetTransactionsQuery request,
    CancellationToken cancellationToken)
    {
        var result = await _transactionRepository.GetPagedAsync(
            request.EmployeeId,
            request.BankAccountId,
            request.Type,
            request.Status,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedResult<TransactionResponse>(
            _mapper.Map<IReadOnlyList<TransactionResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
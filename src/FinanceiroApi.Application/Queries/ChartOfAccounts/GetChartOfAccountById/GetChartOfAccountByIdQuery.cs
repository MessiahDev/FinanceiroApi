using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.ChartOfAccounts.GetChartOfAccountById;

public record GetChartOfAccountByIdQuery(Guid Id) : IRequest<ChartOfAccountResponse>;

public class GetChartOfAccountByIdQueryHandler
    : IRequestHandler<GetChartOfAccountByIdQuery, ChartOfAccountResponse>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IMapper _mapper;

    public GetChartOfAccountByIdQueryHandler(IChartOfAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ChartOfAccountResponse> Handle(
        GetChartOfAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Conta contábil '{request.Id}' não encontrada.");

        return _mapper.Map<ChartOfAccountResponse>(account);
    }
}

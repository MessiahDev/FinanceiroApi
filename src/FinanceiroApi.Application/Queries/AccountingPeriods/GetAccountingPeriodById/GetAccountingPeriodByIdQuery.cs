using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.AccountingPeriods.GetAccountingPeriodById;

public record GetAccountingPeriodByIdQuery(Guid Id) : IRequest<AccountingPeriodResponse>;

public class GetAccountingPeriodByIdQueryHandler
    : IRequestHandler<GetAccountingPeriodByIdQuery, AccountingPeriodResponse>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IMapper _mapper;

    public GetAccountingPeriodByIdQueryHandler(IAccountingPeriodRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AccountingPeriodResponse> Handle(
        GetAccountingPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Período contábil '{request.Id}' não encontrado.");

        return _mapper.Map<AccountingPeriodResponse>(period);
    }
}

using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.BankStatements.GetBankStatementById;

public record GetBankStatementByIdQuery(Guid Id) : IRequest<BankStatementResponse>;

public class GetBankStatementByIdQueryHandler : IRequestHandler<GetBankStatementByIdQuery, BankStatementResponse>
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public GetBankStatementByIdQueryHandler(
        IBankStatementRepository bankStatementRepository,
        IMapper mapper,
        INotificationContext notifications)
    {
        _bankStatementRepository = bankStatementRepository;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankStatementResponse> Handle(GetBankStatementByIdQuery request, CancellationToken cancellationToken)
    {
        var statement = await _bankStatementRepository.GetWithEntriesAsync(request.Id, cancellationToken);
        if (statement is null)
        {
            _notifications.AddNotification("Id", "Extrato bancário não encontrado.");
            return null!;
        }
        return _mapper.Map<BankStatementResponse>(statement);
    }
}

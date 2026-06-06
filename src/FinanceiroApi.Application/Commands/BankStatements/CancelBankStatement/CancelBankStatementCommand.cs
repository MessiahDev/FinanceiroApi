using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankStatements.CancelBankStatement;

public record CancelBankStatementCommand(Guid Id, string Reason) : IRequest<BankStatementSummaryResponse>;

public class CancelBankStatementCommandHandler : IRequestHandler<CancelBankStatementCommand, BankStatementSummaryResponse>
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CancelBankStatementCommandHandler(
        IBankStatementRepository bankStatementRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _bankStatementRepository = bankStatementRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankStatementSummaryResponse> Handle(CancelBankStatementCommand request, CancellationToken cancellationToken)
    {
        var statement = await _bankStatementRepository.GetWithEntriesAsync(request.Id, cancellationToken);
        if (statement is null)
        {
            _notifications.AddNotification("Id", "Extrato bancário não encontrado.");
            return null!;
        }

        statement.Cancel(request.Reason);

        await _bankStatementRepository.UpdateAsync(statement, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<BankStatementSummaryResponse>(statement);
    }
}

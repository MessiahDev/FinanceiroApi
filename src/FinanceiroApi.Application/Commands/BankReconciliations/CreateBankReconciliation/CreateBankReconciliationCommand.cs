using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankReconciliations.CreateBankReconciliation;

public record CreateBankReconciliationCommand(
    Guid BankAccountId,
    Guid BankStatementId,
    decimal SystemBalance,
    string? Notes) : IRequest<BankReconciliationResponse>;

public class CreateBankReconciliationCommandHandler : IRequestHandler<CreateBankReconciliationCommand, BankReconciliationResponse>
{
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateBankReconciliationCommandHandler(
        IBankReconciliationRepository reconciliationRepository,
        IBankStatementRepository bankStatementRepository,
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _reconciliationRepository = reconciliationRepository;
        _bankStatementRepository = bankStatementRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankReconciliationResponse> Handle(CreateBankReconciliationCommand request, CancellationToken cancellationToken)
    {
        var bankAccountExists = await _bankAccountRepository.ExistsAsync(request.BankAccountId, cancellationToken);
        if (!bankAccountExists)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return null!;
        }

        var statement = await _bankStatementRepository.GetWithEntriesAsync(request.BankStatementId, cancellationToken);
        if (statement is null)
        {
            _notifications.AddNotification("BankStatementId", "Extrato bancário não encontrado.");
            return null!;
        }

        var alreadyExists = await _reconciliationRepository.ExistsForStatementAsync(request.BankStatementId, cancellationToken);
        if (alreadyExists)
        {
            _notifications.AddNotification("BankStatementId", "Já existe uma conciliação ativa para este extrato.");
            return null!;
        }

        var reconciliation = BankReconciliation.Create(
            request.BankAccountId,
            request.BankStatementId,
            statement.PeriodStart,
            statement.PeriodEnd,
            statement.OpeningBalance.Amount,
            statement.ClosingBalance.Amount,
            request.SystemBalance,
            request.Notes);

        await _reconciliationRepository.AddAsync(reconciliation, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _reconciliationRepository.GetWithItemsAsync(reconciliation.Id, cancellationToken);
        return _mapper.Map<BankReconciliationResponse>(result!);
    }
}

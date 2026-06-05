using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankAccounts.CreateBankAccount;

public record CreateBankAccountCommand(
    string BankName,
    string BankCode,
    string Agency,
    string AccountNumber,
    BankAccountType AccountType,
    decimal InitialBalance,
    string? PixKey,
    string? Description) : IRequest<BankAccountResponse>;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountResponse>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateBankAccountCommandHandler(
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankAccountResponse> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _bankAccountRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);
        if (existing is not null)
        {
            _notifications.AddNotification("AccountNumber", "Já existe uma conta com este número.");
            return null!;
        }

        var account = BankAccount.Create(
            request.BankName,
            request.BankCode,
            request.Agency,
            request.AccountNumber,
            request.AccountType,
            request.InitialBalance,
            request.PixKey,
            request.Description);

        await _bankAccountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<BankAccountResponse>(account);
    }
}

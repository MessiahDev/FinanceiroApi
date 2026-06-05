using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.AccountsReceivable.CreateAccountReceivable;

public record CreateAccountReceivableCommand(
    Guid CustomerId,
    string Description,
    decimal TotalAmount,
    DateOnly DueDate,
    Guid? CostCenterId,
    string? InvoiceNumber,
    string? Notes) : IRequest<AccountReceivableResponse>;

public class CreateAccountReceivableCommandHandler : IRequestHandler<CreateAccountReceivableCommand, AccountReceivableResponse>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateAccountReceivableCommandHandler(
        IAccountReceivableRepository accountReceivableRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountReceivableResponse> Handle(CreateAccountReceivableCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _customerRepository.ExistsAsync(request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            _notifications.AddNotification("CustomerId", "Cliente não encontrado.");
            return null!;
        }

        var receivable = AccountReceivable.Create(
            request.CustomerId,
            request.Description,
            request.TotalAmount,
            request.DueDate,
            request.CostCenterId,
            request.InvoiceNumber,
            request.Notes);

        await _accountReceivableRepository.AddAsync(receivable, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountReceivableResponse>(receivable);
    }
}

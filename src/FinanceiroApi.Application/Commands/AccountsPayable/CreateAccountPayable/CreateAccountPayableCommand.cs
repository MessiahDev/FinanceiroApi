using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.AccountsPayable.CreateAccountPayable;

public record CreateAccountPayableCommand(
    Guid SupplierId,
    string Description,
    decimal TotalAmount,
    DateOnly DueDate,
    Guid? CostCenterId,
    string? InvoiceNumber,
    string? Notes) : IRequest<AccountPayableResponse>;

public class CreateAccountPayableCommandHandler : IRequestHandler<CreateAccountPayableCommand, AccountPayableResponse>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateAccountPayableCommandHandler(
        IAccountPayableRepository accountPayableRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountPayableRepository = accountPayableRepository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountPayableResponse> Handle(CreateAccountPayableCommand request, CancellationToken cancellationToken)
    {
        var supplierExists = await _supplierRepository.ExistsAsync(request.SupplierId, cancellationToken);
        if (!supplierExists)
        {
            _notifications.AddNotification("SupplierId", "Fornecedor não encontrado.");
            return null!;
        }

        var payable = AccountPayable.Create(
            request.SupplierId,
            request.Description,
            request.TotalAmount,
            request.DueDate,
            request.CostCenterId,
            request.InvoiceNumber,
            request.Notes);

        await _accountPayableRepository.AddAsync(payable, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountPayableResponse>(payable);
    }
}

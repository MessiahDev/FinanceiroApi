using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.Suppliers.DeleteSupplier;

public record DeleteSupplierCommand(Guid Id) : IRequest<bool>;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public DeleteSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
        {
            _notifications.AddNotification("Id", "Fornecedor não encontrado.");
            return false;
        }

        await _supplierRepository.DeleteAsync(supplier, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}

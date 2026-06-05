using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.Suppliers.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? ContactName) : IRequest<SupplierResponse?>;

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierResponse?>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public UpdateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<SupplierResponse?> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
        {
            _notifications.AddNotification("Id", "Fornecedor não encontrado.");
            return null;
        }

        supplier.Update(request.Name, request.Email, request.Phone, request.ContactName);

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<SupplierResponse>(supplier);
    }
}

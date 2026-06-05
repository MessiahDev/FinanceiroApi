using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Suppliers.CreateSupplier;

public record CreateSupplierCommand(
    string Name,
    string TaxId,
    PersonType PersonType,
    string Email,
    string? Phone,
    string? ContactName) : IRequest<SupplierResponse>;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierResponse>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateSupplierCommandHandler(
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

    public async Task<SupplierResponse> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var exists = await _supplierRepository.ExistsByTaxIdAsync(request.TaxId, cancellationToken);
        if (exists)
        {
            _notifications.AddNotification("TaxId", "Já existe um fornecedor com este CPF/CNPJ.");
            return null!;
        }

        var supplier = Supplier.Create(
            request.Name,
            request.TaxId,
            request.PersonType,
            request.Email,
            request.Phone,
            request.ContactName);

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<SupplierResponse>(supplier);
    }
}

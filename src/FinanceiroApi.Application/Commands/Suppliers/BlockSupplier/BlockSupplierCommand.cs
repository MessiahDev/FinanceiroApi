using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.Suppliers.BlockSupplier;

public record BlockSupplierCommand(Guid Id, string Reason) : IRequest<SupplierResponse?>;

public class BlockSupplierCommandHandler : IRequestHandler<BlockSupplierCommand, SupplierResponse?>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public BlockSupplierCommandHandler(
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

    public async Task<SupplierResponse?> Handle(BlockSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
        {
            _notifications.AddNotification("Id", "Fornecedor não encontrado.");
            return null;
        }

        supplier.Block(request.Reason);

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<SupplierResponse>(supplier);
    }
}

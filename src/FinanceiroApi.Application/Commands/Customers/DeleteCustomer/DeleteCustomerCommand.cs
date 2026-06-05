using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Customers.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            _notifications.AddNotification("Id", "Cliente não encontrado.");
            return false;
        }

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
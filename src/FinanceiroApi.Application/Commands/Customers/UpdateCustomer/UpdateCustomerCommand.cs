using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Customers.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? ContactName) : IRequest<CustomerResponse?>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponse?>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<CustomerResponse?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            _notifications.AddNotification("Id", "Cliente não encontrado.");
            return null;
        }

        customer.Update(request.Name, request.Email, request.Phone, request.ContactName);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<CustomerResponse>(customer);
    }
}

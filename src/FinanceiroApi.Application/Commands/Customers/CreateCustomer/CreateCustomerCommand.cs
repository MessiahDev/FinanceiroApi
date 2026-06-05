using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Customers.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string TaxId,
    PersonType PersonType,
    string Email,
    string? Phone,
    string? ContactName,
    decimal CreditLimit) : IRequest<CustomerResponse>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateCustomerCommandHandler(
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

    public async Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var exists = await _customerRepository.ExistsByTaxIdAsync(request.TaxId, cancellationToken);
        if (exists)
        {
            _notifications.AddNotification("TaxId", "Já existe um cliente com este CPF/CNPJ.");
            return null!;
        }

        var customer = Customer.Create(
            request.Name,
            request.TaxId,
            request.PersonType,
            request.Email,
            request.Phone,
            request.ContactName,
            request.CreditLimit);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<CustomerResponse>(customer);
    }
}

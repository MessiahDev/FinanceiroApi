using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.AccountsPayable.CancelAccountPayable;

public record CancelAccountPayableCommand(Guid Id, string Reason) : IRequest<AccountPayableResponse?>;

public class CancelAccountPayableCommandHandler : IRequestHandler<CancelAccountPayableCommand, AccountPayableResponse?>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CancelAccountPayableCommandHandler(
        IAccountPayableRepository accountPayableRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountPayableRepository = accountPayableRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountPayableResponse?> Handle(CancelAccountPayableCommand request, CancellationToken cancellationToken)
    {
        var payable = await _accountPayableRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payable is null)
        {
            _notifications.AddNotification("Id", "Conta a pagar não encontrada.");
            return null;
        }

        payable.Cancel(request.Reason);

        await _accountPayableRepository.UpdateAsync(payable, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountPayableResponse>(payable);
    }
}

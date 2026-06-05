using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.AccountsReceivable.CancelAccountReceivable;

public record CancelAccountReceivableCommand(Guid Id, string Reason) : IRequest<AccountReceivableResponse?>;

public class CancelAccountReceivableCommandHandler : IRequestHandler<CancelAccountReceivableCommand, AccountReceivableResponse?>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CancelAccountReceivableCommandHandler(
        IAccountReceivableRepository accountReceivableRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountReceivableResponse?> Handle(CancelAccountReceivableCommand request, CancellationToken cancellationToken)
    {
        var receivable = await _accountReceivableRepository.GetByIdAsync(request.Id, cancellationToken);
        if (receivable is null)
        {
            _notifications.AddNotification("Id", "Conta a receber não encontrada.");
            return null;
        }

        receivable.Cancel(request.Reason);

        await _accountReceivableRepository.UpdateAsync(receivable, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountReceivableResponse>(receivable);
    }
}

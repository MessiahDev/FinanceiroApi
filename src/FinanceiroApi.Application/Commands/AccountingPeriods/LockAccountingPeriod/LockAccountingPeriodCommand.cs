using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.LockAccountingPeriod;

public record LockAccountingPeriodCommand(Guid Id) : IRequest;

public class LockAccountingPeriodCommandHandler : IRequestHandler<LockAccountingPeriodCommand>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LockAccountingPeriodCommandHandler(IAccountingPeriodRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LockAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"PerÃ­odo contÃ¡bil '{request.Id}' nÃ£o encontrado.");

        period.Lock();
        await _repository.UpdateAsync(period, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}


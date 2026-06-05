using MediatR;
using FluentValidation;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.CreateAccountingPeriod;

public record CreateAccountingPeriodCommand(int Year, int Month) : IRequest<Guid>;

public class CreateAccountingPeriodCommandHandler : IRequestHandler<CreateAccountingPeriodCommand, Guid>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountingPeriodCommandHandler(IAccountingPeriodRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByYearMonthAsync(request.Year, request.Month, null, cancellationToken);
        if (exists)
            throw new DuplicateAccountingPeriodException(request.Year, request.Month);

        var period = AccountingPeriod.Create(request.Year, request.Month);
        await _repository.AddAsync(period, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return period.Id;
    }
}

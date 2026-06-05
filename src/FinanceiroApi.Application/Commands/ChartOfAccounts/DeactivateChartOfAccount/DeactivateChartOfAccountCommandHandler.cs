using MediatR;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.DeactivateChartOfAccount;

public class DeactivateChartOfAccountCommandHandler : IRequestHandler<DeactivateChartOfAccountCommand>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateChartOfAccountCommandHandler(IChartOfAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateChartOfAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Conta contabil '{request.Id}' nao encontrada.");

        account.Deactivate();
        await _repository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}

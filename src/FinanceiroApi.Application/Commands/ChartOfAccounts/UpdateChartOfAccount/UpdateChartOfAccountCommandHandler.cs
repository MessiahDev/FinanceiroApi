using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using MediatR;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.UpdateChartOfAccount;

public class UpdateChartOfAccountCommandHandler : IRequestHandler<UpdateChartOfAccountCommand>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateChartOfAccountCommandHandler(IChartOfAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateChartOfAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Conta contÃ¡bil '{request.Id}' nÃ£o encontrada.");

        account.Update(request.Name, request.Description, request.AcceptsEntries);

        await _repository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}


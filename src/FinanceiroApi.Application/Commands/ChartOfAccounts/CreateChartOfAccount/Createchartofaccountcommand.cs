using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using MediatR;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.CreateChartOfAccount;

public record CreateChartOfAccountCommand(
    string Code,
    string Name,
    string? Description,
    AccountType AccountType,
    AccountNature AccountNature,
    bool AcceptsEntries,
    Guid? ParentAccountId
) : IRequest<Guid>;

public class CreateChartOfAccountCommandHandler : IRequestHandler<CreateChartOfAccountCommand, Guid>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateChartOfAccountCommandHandler(
        IChartOfAccountRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateChartOfAccountCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _repository.ExistsCodeAsync(request.Code, null, cancellationToken);
        if (codeExists)
            throw new DuplicateChartOfAccountCodeException(request.Code);

        if (request.ParentAccountId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(request.ParentAccountId.Value, cancellationToken)
                ?? throw new DomainException($"Conta pai com id '{request.ParentAccountId}' nÃ£o encontrada.");

            if (!parent.IsActive)
                throw new DomainException("NÃ£o Ã© possÃ­vel criar uma subconta de uma conta inativa.");
        }

        var account = ChartOfAccount.Create(
            request.Code,
            request.Name,
            request.Description,
            request.AccountType,
            request.AccountNature,
            request.AcceptsEntries,
            request.ParentAccountId);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return account.Id;
    }
}

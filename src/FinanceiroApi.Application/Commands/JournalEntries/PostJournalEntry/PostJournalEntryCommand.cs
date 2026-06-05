using MediatR;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Application.Commands.JournalEntries.PostJournalEntry;

public record PostJournalEntryCommand(Guid Id) : IRequest;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PostJournalEntryCommandHandler(IJournalEntryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetWithLinesAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Lancamento '{request.Id}' nao encontrado.");

        entry.Post();
        await _repository.UpdateAsync(entry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}

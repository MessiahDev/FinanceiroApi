using MediatR;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Application.Commands.JournalEntries.ReverseJournalEntry;

public record ReverseJournalEntryCommand(
    Guid Id,
    string ReversalDescription,
    Guid ReversedByUserId) : IRequest<Guid>;

public class ReverseJournalEntryCommandHandler : IRequestHandler<ReverseJournalEntryCommand, Guid>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReverseJournalEntryCommandHandler(
        IJournalEntryRepository repository,
        IAccountingPeriodRepository periodRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var original = await _repository.GetWithLinesAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Lancamento '{request.Id}' nao encontrado.");

        var period = await _periodRepository.GetCurrentOpenPeriodAsync(cancellationToken)
            ?? throw new DomainException("Nao ha periodo contabil aberto para lancar o estorno.");

        var entryNumber = await _repository.GetNextEntryNumberAsync(DateTime.UtcNow.Year, cancellationToken);

        var reversal = JournalEntry.Create(
            entryNumber,
            request.ReversalDescription,
            DateTime.UtcNow,
            JournalEntryType.Reversal,
            period.Id,
            request.ReversedByUserId,
            original.ReferenceDocument,
            original.ReferenceDocumentType,
            original.ReferenceDocumentId);

        foreach (var line in original.Lines)
        {
            var invertedDC = line.DebitCredit == DebitCredit.Debit
                ? DebitCredit.Credit
                : DebitCredit.Debit;

            reversal.AddLine(line.ChartOfAccountId, invertedDC, line.Amount, line.Description);
        }

        reversal.Post();
        original.Reverse(request.ReversalDescription, request.ReversedByUserId);

        await _repository.AddAsync(reversal, cancellationToken);
        await _repository.UpdateAsync(original, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return reversal.Id;
    }
}

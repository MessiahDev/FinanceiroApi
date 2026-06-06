using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class TransactionTests
{
    private static Transaction CreateValid(decimal amount = 1000m) =>
        Transaction.Create(amount, TransactionType.Credit, TransactionCategory.Salary, "Pagamento salário");

    // --- Create ---

    [Fact]
    public void Create_ValidData_ShouldReturnPendingTransaction()
    {
        var tx = CreateValid();

        Assert.Equal(1000m, tx.Amount.Amount);
        Assert.Equal(TransactionType.Credit, tx.Type);
        Assert.Equal(TransactionCategory.Salary, tx.Category);
        Assert.Equal(TransactionStatus.Pending, tx.Status);
        Assert.Equal("Pagamento salário", tx.Description);
    }

    [Fact]
    public void Create_WithoutDate_ShouldDefaultToToday()
    {
        var tx = CreateValid();

        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), tx.TransactionDate);
    }

    [Fact]
    public void Create_WithExplicitDate_ShouldUseProvidedDate()
    {
        var date = new DateOnly(2024, 1, 15);
        var tx = Transaction.Create(500m, TransactionType.Debit, TransactionCategory.Tax, "DARF", transactionDate: date);

        Assert.Equal(date, tx.TransactionDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_EmptyDescription_ShouldThrowDomainException(string? desc)
    {
        Assert.Throws<DomainException>(() =>
            Transaction.Create(100m, TransactionType.Credit, TransactionCategory.Other, desc!));
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var tx = CreateValid();

        Assert.Single(tx.DomainEvents);
    }

    // --- Confirm ---

    [Fact]
    public void Confirm_PendingTransaction_ShouldSetStatusToConfirmed()
    {
        var tx = CreateValid();

        tx.Confirm();

        Assert.Equal(TransactionStatus.Confirmed, tx.Status);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ShouldThrowDomainException()
    {
        var tx = CreateValid();
        tx.Confirm();

        Assert.Throws<DomainException>(() => tx.Confirm());
    }

    [Fact]
    public void Confirm_CancelledTransaction_ShouldThrowDomainException()
    {
        var tx = CreateValid();
        tx.Cancel("Erro");

        Assert.Throws<DomainException>(() => tx.Confirm());
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_PendingTransaction_ShouldSetStatusToCancelled()
    {
        var tx = CreateValid();

        tx.Cancel("Cancelado por teste");

        Assert.Equal(TransactionStatus.Cancelled, tx.Status);
    }

    [Fact]
    public void Cancel_ConfirmedTransaction_ShouldThrowDomainException()
    {
        var tx = CreateValid();
        tx.Confirm();

        Assert.Throws<DomainException>(() => tx.Cancel("Tentativa"));
    }

    [Fact]
    public void Cancel_ShouldAppendReasonToNotes()
    {
        var tx = Transaction.Create(100m, TransactionType.Credit, TransactionCategory.Other, "Desc", notes: "Nota original");

        tx.Cancel("Motivo cancelamento");

        Assert.Contains("Motivo cancelamento", tx.Notes);
    }
}

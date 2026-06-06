using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class TaxPaymentTests
{
    private static readonly Guid TaxEntryId = Guid.NewGuid();
    private static readonly Guid BankAccountId = Guid.NewGuid();
    private static readonly DateOnly PaymentDate = new(2025, 1, 31);

    private static TaxPayment CreateValid() => TaxPayment.Create(
        taxEntryId: TaxEntryId,
        bankAccountId: BankAccountId,
        amount: 500m,
        paymentDate: PaymentDate);

    [Fact]
    public void Create_WithValidData_ShouldCreateTaxPayment()
    {
        var payment = CreateValid();

        payment.Should().NotBeNull();
        payment.TaxEntryId.Should().Be(TaxEntryId);
        payment.BankAccountId.Should().Be(BankAccountId);
        payment.Amount.Amount.Should().Be(500m);
        payment.PaymentDate.Should().Be(PaymentDate);
        payment.Status.Should().Be(TaxPaymentStatus.Paid);
    }

    [Fact]
    public void Create_ShouldRaiseTaxPaymentRegisteredEvent()
    {
        var payment = CreateValid();

        payment.DomainEvents.Should().ContainSingle(e => e is TaxPaymentRegisteredEvent);
    }

    [Fact]
    public void Create_WithFineAndInterest_ShouldCalculateTotalPaidCorrectly()
    {
        var payment = TaxPayment.Create(TaxEntryId, BankAccountId, 500m, PaymentDate,
            fine: 10m, interest: 5m);

        payment.Fine.Amount.Should().Be(10m);
        payment.Interest.Amount.Should().Be(5m);
        payment.TotalPaid.Amount.Should().Be(515m);
    }

    [Fact]
    public void Create_WithoutFineAndInterest_TotalPaidShouldEqualAmount()
    {
        var payment = CreateValid();

        payment.TotalPaid.Amount.Should().Be(500m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidAmount_ShouldThrow(decimal amount)
    {
        var act = () => TaxPayment.Create(TaxEntryId, BankAccountId, amount, PaymentDate);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeFine_ShouldThrow()
    {
        var act = () => TaxPayment.Create(TaxEntryId, BankAccountId, 500m, PaymentDate, fine: -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeInterest_ShouldThrow()
    {
        var act = () => TaxPayment.Create(TaxEntryId, BankAccountId, 500m, PaymentDate, interest: -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithDarfAndReceiptCode_ShouldPersistThem()
    {
        var payment = TaxPayment.Create(TaxEntryId, BankAccountId, 500m, PaymentDate,
            darfNumber: "DARF-001", receiptCode: "REC-999");

        payment.DarfNumber.Should().Be("DARF-001");
        payment.ReceiptCode.Should().Be("REC-999");
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldSetStatusCancelled()
    {
        var payment = CreateValid();

        payment.Cancel("pagamento duplicado");

        payment.Status.Should().Be(TaxPaymentStatus.Cancelled);
        payment.Notes.Should().Contain("pagamento duplicado");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var payment = CreateValid();
        payment.Cancel("motivo");

        var act = () => payment.Cancel("outro motivo");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WithExistingNotes_ShouldAppendReason()
    {
        var payment = TaxPayment.Create(TaxEntryId, BankAccountId, 500m, PaymentDate,
            notes: "nota original");

        payment.Cancel("cancelamento");

        payment.Notes.Should().Contain("nota original");
        payment.Notes.Should().Contain("cancelamento");
    }
}

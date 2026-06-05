using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class SupplierTests
{
    private static Supplier CreateValid() => Supplier.Create(
        "Fornecedor SA", "12345678000195", PersonType.Company, "fornecedor@email.com");

    [Fact]
    public void Create_WithValidData_ShouldCreateSupplier()
    {
        var supplier = CreateValid();
        supplier.Should().NotBeNull();
        supplier.Status.Should().Be(SupplierStatus.Active);
    }

    [Fact]
    public void Create_ShouldRaiseSupplierCreatedEvent()
    {
        var supplier = CreateValid();
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        var act = () => Supplier.Create(name!, "12345678000195", PersonType.Company, "a@b.com");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Block_ShouldSetStatusToBlocked()
    {
        var supplier = CreateValid();
        supplier.Block("fraude");
        supplier.Status.Should().Be(SupplierStatus.Blocked);
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierBlockedEvent);
    }

    [Fact]
    public void Block_WhenAlreadyBlocked_ShouldThrow()
    {
        var supplier = CreateValid();
        supplier.Block("motivo");
        var act = () => supplier.Block("outro motivo");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var supplier = CreateValid();
        supplier.Update("Novo Nome", "novo@email.com", "11999999999", "Contato");
        supplier.Name.Should().Be("Novo Nome");
        supplier.Phone.Should().Be("11999999999");
    }

    [Fact]
    public void UpdateBankingInfo_WithValidData_ShouldUpdate()
    {
        var supplier = CreateValid();
        supplier.UpdateBankingInfo("BB", "1234", "56789", "pix@key.com");
        supplier.BankName.Should().Be("BB");
        supplier.PixKey.Should().Be("pix@key.com");
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var supplier = CreateValid();
        supplier.Deactivate();
        supplier.Status.Should().Be(SupplierStatus.Inactive);
    }
}

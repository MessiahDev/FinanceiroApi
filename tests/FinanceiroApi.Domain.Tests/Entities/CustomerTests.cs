using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinanceiroApi.Domain.Tests.Entities;

public class CustomerTests
{
    private static Customer CreateValid() => Customer.Create(
        "Cliente SA", "12345678000195", PersonType.Company,
        "cliente@email.com", creditLimit: 5000m);

    [Fact]
    public void Create_WithValidData_ShouldCreate()
    {
        var customer = CreateValid();
        customer.Should().NotBeNull();
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        var act = () => Customer.Create(name!, "12345678000195", PersonType.Company, "a@b.com");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Block_ShouldSetStatusToBlocked()
    {
        var customer = CreateValid();
        customer.Block("inadimplencia");
        customer.Status.Should().Be(CustomerStatus.Blocked);
    }

    [Fact]
    public void Block_WhenAlreadyBlocked_ShouldThrow()
    {
        var customer = CreateValid();
        customer.Block("motivo");
        var act = () => customer.Block("outro");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        var customer = CreateValid();
        customer.Block("motivo");
        customer.Activate();
        customer.Status.Should().Be(CustomerStatus.Active);
    }
}

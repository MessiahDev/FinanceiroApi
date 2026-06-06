using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ValidData_ShouldReturnActiveUser()
    {
        var user = User.Create("Alex Silva", "alex@empresa.com", "hash_seguro_123", UserRole.Admin);

        Assert.Equal("Alex Silva", user.Name);
        Assert.Equal("alex@empresa.com", user.Email);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Create_EmailShouldBeLowerCase()
    {
        var user = User.Create("Teste", "ALEX@EMPRESA.COM", "hash", UserRole.Employee);

        Assert.Equal("alex@empresa.com", user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyName_ShouldThrowDomainException(string name)
    {
        Assert.Throws<DomainException>(() => User.Create(name, "a@b.com", "hash", UserRole.Employee));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyEmail_ShouldThrowDomainException(string email)
    {
        Assert.Throws<DomainException>(() => User.Create("Nome", email, "hash", UserRole.Employee));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyPasswordHash_ShouldThrowDomainException(string hash)
    {
        Assert.Throws<DomainException>(() => User.Create("Nome", "a@b.com", hash, UserRole.Employee));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = User.Create("Alex", "alex@b.com", "hash", UserRole.Manager);

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = User.Create("Alex", "alex@b.com", "hash", UserRole.Manager);
        user.Deactivate();

        user.Activate();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void UpdatePasswordHash_ShouldUpdateHash()
    {
        var user = User.Create("Alex", "alex@b.com", "hash_antigo", UserRole.Admin);

        user.UpdatePasswordHash("hash_novo");

        Assert.Equal("hash_novo", user.PasswordHash);
    }
}

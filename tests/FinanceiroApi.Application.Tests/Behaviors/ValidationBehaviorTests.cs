using FinanceiroApi.Application.Behaviors;
using FinanceiroApi.Application.Commands.Employees.CreateEmployee;
using FinanceiroApi.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Xunit;
namespace FinanceiroApi.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        var validators = new[] { new CreateEmployeeCommandValidator() };
        var behavior = new ValidationBehavior<CreateEmployeeCommand, object>(validators);
        var called = false;
        var validCommand = new CreateEmployeeCommand(
            "Joao", "Silva", "joao@test.com", "52998224725",
            Position.DesenvolvedorJunior, Guid.NewGuid(), 5000m, ContractType.CLT);
        await behavior.Handle(validCommand, () => { called = true; return Task.FromResult<object>(null!); }, default);
        called.Should().BeTrue();
    }
    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        var validators = new[] { new CreateEmployeeCommandValidator() };
        var behavior = new ValidationBehavior<CreateEmployeeCommand, object>(validators);
        var invalidCommand = new CreateEmployeeCommand(
            string.Empty, string.Empty, "not-an-email", "123",
            (Position)999, Guid.Empty, -1m, ContractType.CLT);
        var act = async () => await behavior.Handle(
            invalidCommand,
            () => Task.FromResult<object>(null!),
            default);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any());
    }
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNextDirectly()
    {
        var behavior = new ValidationBehavior<CreateEmployeeCommand, object>(
            Array.Empty<IValidator<CreateEmployeeCommand>>());
        var called = false;
        var command = new CreateEmployeeCommand(
            "N", "S", "e@e.com", "52998224725",
            Position.DesenvolvedorJunior, Guid.NewGuid(), 1000m, ContractType.CLT);
        await behavior.Handle(command, () => { called = true; return Task.FromResult<object>(null!); }, default);
        called.Should().BeTrue();
    }
}


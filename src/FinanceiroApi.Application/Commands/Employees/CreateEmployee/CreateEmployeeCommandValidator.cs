using FluentValidation;

namespace FinanceiroApi.Application.Commands.Employees.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .Matches(@"^\d{11}$")
            .WithMessage("CPF deve conter exatamente 11 dígitos numéricos, sem pontuação.");

        RuleFor(x => x.Position)
            .NotEmpty();

        RuleFor(x => x.DepartmentId)
            .NotEmpty();

        RuleFor(x => x.BaseSalary)
            .GreaterThan(0);

        RuleFor(x => x.ContractType)
            .IsInEnum();
    }
}
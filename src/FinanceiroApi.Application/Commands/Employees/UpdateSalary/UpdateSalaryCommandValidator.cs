using FluentValidation;

namespace FinanceiroApi.Application.Commands.Employees.UpdateSalary;

public class UpdateSalaryCommandValidator : AbstractValidator<UpdateSalaryCommand>
{
    public UpdateSalaryCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.NewSalary).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
using FluentValidation;

namespace FinanceiroApi.Application.Commands.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
	public CreateDepartmentCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(x => x.CostCenter)
			.NotEmpty()
			.MaximumLength(50);

		RuleFor(x => x.Description)
			.MaximumLength(500)
			.When(x => x.Description is not null);
	}
}
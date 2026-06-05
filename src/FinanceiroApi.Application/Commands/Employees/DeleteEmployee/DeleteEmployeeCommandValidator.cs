using FluentValidation;

namespace FinanceiroApi.Application.Commands.Employees.DeleteEmployee;

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id do funcionário é obrigatório.");
    }
}
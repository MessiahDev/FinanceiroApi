using FluentValidation;

namespace FinanceiroApi.Application.Commands.Budgets.ApproveBudget;

public class ApproveBudgetCommandValidator : AbstractValidator<ApproveBudgetCommand>
{
    public ApproveBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApprovedBy).NotEmpty();
    }
}
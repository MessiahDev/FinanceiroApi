using FluentValidation;

namespace FinanceiroApi.Application.Commands.Customers.BlockCustomer;

public class BlockCustomerCommandValidator : AbstractValidator<BlockCustomerCommand>
{
    public BlockCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
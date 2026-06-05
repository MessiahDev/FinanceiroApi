using FluentValidation;

namespace FinanceiroApi.Application.Commands.Suppliers.BlockSupplier;

public class BlockSupplierCommandValidator : AbstractValidator<BlockSupplierCommand>
{
    public BlockSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
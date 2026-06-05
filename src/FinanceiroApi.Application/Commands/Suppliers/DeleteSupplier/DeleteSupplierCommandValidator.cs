using FluentValidation;

namespace FinanceiroApi.Application.Commands.Suppliers.DeleteSupplier;

public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
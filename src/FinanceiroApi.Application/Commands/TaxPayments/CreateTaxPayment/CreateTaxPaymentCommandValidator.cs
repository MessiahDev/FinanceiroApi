using FluentValidation;

namespace FinanceiroApi.Application.Commands.TaxPayments.CreateTaxPayment;

public class CreateTaxPaymentCommandValidator : AbstractValidator<CreateTaxPaymentCommand>
{
    public CreateTaxPaymentCommandValidator()
    {
        RuleFor(x => x.TaxEntryId).NotEmpty().WithMessage("Id do lançamento fiscal é obrigatório.");
        RuleFor(x => x.BankAccountId).NotEmpty().WithMessage("Conta bancária é obrigatória.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
        RuleFor(x => x.Fine).GreaterThanOrEqualTo(0).WithMessage("Multa não pode ser negativa.");
        RuleFor(x => x.Interest).GreaterThanOrEqualTo(0).WithMessage("Juros não pode ser negativo.");
    }
}

using FluentValidation;

namespace FinanceiroApi.Application.Commands.TaxEntries.CreateTaxEntry;

public class CreateTaxEntryCommandValidator : AbstractValidator<CreateTaxEntryCommand>
{
    public CreateTaxEntryCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição não pode exceder 500 caracteres.");

        RuleFor(x => x.BaseAmount)
            .GreaterThan(0).WithMessage("Valor base deve ser maior que zero.");

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100).WithMessage("Alíquota deve estar entre 0 e 100.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.Competence)
            .WithMessage("Data de vencimento não pode ser anterior à competência.");
    }
}

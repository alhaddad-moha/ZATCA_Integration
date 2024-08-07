namespace ZATCA_V3.CustomValidators;

using FluentValidation;
using ZATCA_V3.Requests;

public class AllowanceChargeValidator : AbstractValidator<AllowanceCharge>
{
    public AllowanceChargeValidator()
    {
        RuleFor(x => x.TaxCategory).NotEmpty().WithMessage("TaxCategory is required.")
            .Matches("O|S|E|Z").WithMessage("TaxCategory must be 'S', 'O', 'E', or 'Z'.");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0.");

        RuleFor(x => x.TaxCategoryPercent).InclusiveBetween(0, 100).WithMessage("TaxCategoryPercent must be between 0 and 100.");
    }
}
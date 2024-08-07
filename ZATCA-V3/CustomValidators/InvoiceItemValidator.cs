namespace ZATCA_V3.CustomValidators;

using FluentValidation;
using ZATCA_V3.Requests;

public class InvoiceItemValidator : AbstractValidator<InvoiceItem>
{
    public InvoiceItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.BaseQuantity).GreaterThan(0).WithMessage("Base Quantity must be greater than zero.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(x => x.TaxCategory).NotEmpty().WithMessage("TaxCategory is required.")
            .Matches("O|S|E|Z").WithMessage("TaxCategory must be 'S', 'O', 'E', or 'Z'.");
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100).WithMessage("Vat Percentage must be between 0 and 100.");
    }
}
namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class InvoiceTypeValidator : AbstractValidator<InvoiceType>
{
    public InvoiceTypeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .InclusiveBetween(381, 388)
            .WithMessage("Id must be '388' (invoice), '383' (debit note), or '381' (credit note).");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Matches("0100000|0200000")
            .WithMessage("Name must be '0100000' (standard invoice) or '0200000' (simplified invoice).");
       
        RuleFor(x => x.DocumentCurrencyCode)
            .NotEmpty()
            .WithMessage("DocumentCurrencyCode is required.")
            .Length(3)
            .WithMessage("DocumentCurrencyCode must be 3 characters long.");
        
        RuleFor(x => x.TaxCurrencyCode).NotEmpty().WithMessage("TaxCurrencyCode is required.")
            .Length(3).WithMessage("TaxCurrencyCode must be 3 characters long.");
    }
}
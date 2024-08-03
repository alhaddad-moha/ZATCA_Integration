namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class CustomerInformationValidator : AbstractValidator<CustomerInformation>
{
    public CustomerInformationValidator()
    {
        RuleFor(x => x.CommercialRegistrationNumber).NotEmpty().WithMessage("Commercial Number is required.");
        RuleFor(x => x.CommercialNumberType).NotEmpty().WithMessage("Commercial NumberType is required.");
        RuleFor(x => x.RegistrationName).NotEmpty().WithMessage("Registration Name is required.");
        RuleFor(x => x.TaxRegistrationNumber).NotEmpty().WithMessage("Tax Registration Number is required.");
    }
}
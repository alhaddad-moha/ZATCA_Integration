namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class CustomerInformationValidator : AbstractValidator<CustomerInformation>
{
    public CustomerInformationValidator()
    {
        RuleFor(x => x.CommercialNumber).NotEmpty().WithMessage("Commercial Number is required.");
        RuleFor(x => x.CommercialNumberType).NotEmpty().WithMessage("Commercial NumberType is required.");
        RuleFor(x => x.RegistrationName).NotEmpty().WithMessage("Registration Name is required.");
        RuleFor(x => x.RegistrationNumber).NotEmpty().WithMessage("Registration Number is required.");
    }
}
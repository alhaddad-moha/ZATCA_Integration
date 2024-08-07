namespace ZATCA_V3.CustomValidators;

using FluentValidation;
using ZATCA_V3.Requests;

public class PaymentDetailsValidator : AbstractValidator<PaymentDetails>
{
    public PaymentDetailsValidator()
    {
        RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required.")
            .Matches("10|30|42|48")
            .WithMessage(
                "Type must be '10' (on cash), '30' (on credit), '42' (bank account payment), or '48' (bank card payment).");
    }
}
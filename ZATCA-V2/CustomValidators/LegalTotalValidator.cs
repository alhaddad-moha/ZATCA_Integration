using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class LegalTotalValidator : AbstractValidator<LegalTotal>
{
    public LegalTotalValidator()
    {
        RuleFor(x => x.PrepaidAmount).GreaterThanOrEqualTo(0)
            .WithMessage("PrepaidAmount must be greater than or equal to zero.");
    }
}
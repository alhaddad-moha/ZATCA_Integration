using ZATCA_V3.Repositories.Interfaces;

namespace ZATCA_V3.CustomValidators;

using FluentValidation;
using ZATCA_V3.Requests;

public class LegalTotalValidator : AbstractValidator<LegalTotal>
{
    public LegalTotalValidator()
    {
        RuleFor(x => x.PrepaidAmount).GreaterThanOrEqualTo(0)
            .WithMessage("PrepaidAmount must be greater than or equal to zero.");
    }
}
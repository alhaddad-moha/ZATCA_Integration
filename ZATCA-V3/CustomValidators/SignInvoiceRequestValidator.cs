using ZATCA_V3.Repositories.Interfaces;

namespace ZATCA_V3.CustomValidators;

using FluentValidation;
using ZATCA_V3.Requests;

public class SignInvoiceRequestValidator : AbstractValidator<SingleInvoiceRequest>
{
    private readonly ICompanyRepository _companyRepository;

    public SignInvoiceRequestValidator(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;


        RuleFor(x => x.Invoice).SetValidator(new InvoiceValidator());

        RuleFor(x => x.InvoiceType).NotEmpty()
            .WithMessage("{PropertyName} is required").SetValidator(new InvoiceTypeValidator());
        RuleFor(p => p.CompanyId)
            .GreaterThan(0)
            .MustAsync(async (id, token) =>
            {
                var invitationExists = await _companyRepository.GetById(id);
                return invitationExists != null;
            }).WithMessage("{PropertyName} does not exist");
    }
}
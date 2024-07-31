using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class BulkInvoiceRequestValidator : AbstractValidator<BulkInvoiceRequest>
{
    private readonly ICompanyRepository _companyRepository; 
    public BulkInvoiceRequestValidator(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
        
        
        RuleForEach(x => x.Invoices).SetValidator(new InvoiceValidator());

        RuleFor(x => x.InvoicesType).SetValidator(new InvoiceTypeValidator());
        RuleFor(p => p.companyId)
            .GreaterThan(0)
            .MustAsync(async (id, token) =>
            {
                var companyExists = await _companyRepository.GetById(id);
                return companyExists != null;
            }).WithMessage("{PropertyName} does not exist");
    }
}
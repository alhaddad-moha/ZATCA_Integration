namespace ZATCA_V2.CustomValidators;

using FluentValidation;
using ZATCA_V2.Requests;

public class InvoiceValidator : AbstractValidator<InvoiceData>
{
    public InvoiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.AddtionalId).NotEmpty().WithMessage("AddtionalId is required.");
        RuleFor(x => x.IssueDate).NotEmpty().WithMessage("IssueDate is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("IssueDate must be a valid date in the format yyyy-MM-dd.");
        RuleFor(x => x.IssueTime).NotEmpty().WithMessage("IssueTime is required.")
            .Matches(@"^\d{2}:\d{2}:\d{2}$").WithMessage("IssueTime must be a valid time in the format HH:mm:ss.");
        RuleFor(x => x.ActualDeliveryDate).NotEmpty().WithMessage("ActualDeliveryDate is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .WithMessage("ActualDeliveryDate must be a valid date in the format yyyy-MM-dd.");
        RuleFor(x => x.LatestDeliveryDate).NotEmpty().WithMessage("LatestDeliveryDate is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .WithMessage("LatestDeliveryDate must be a valid date in the format yyyy-MM-dd.");

        RuleFor(x => x.PaymentDetails).SetValidator(new PaymentDetailsValidator());
        RuleForEach(x => x.InvoiceItems).SetValidator(new InvoiceItemValidator());
        RuleFor(x => x.CustomerInformation).SetValidator(new CustomerInformationValidator());
        RuleFor(x => x.AllowanceCharge).SetValidator(new AllowanceChargeValidator());
        RuleFor(x => x.LegalTotal).SetValidator(new LegalTotalValidator());
    }
}
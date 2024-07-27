using System.ComponentModel.DataAnnotations;

namespace ZATCA_V2.Requests;

public class SingleInvoiceRequest
{
    [Required]
    public int CompanyId { get; set; }
    [Required]
    public InvoiceType InvoiceType { get; set; }
    [Required]

    public InvoiceData? Invoice { get; set; }

}
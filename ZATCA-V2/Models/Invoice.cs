
namespace ZATCA_V2.Models;

public class Invoice
{
    public int Id { get; set; }
    public string UUID { get; set; }
    public string SystemInvoiceId { get; set; }
    public string Hash { get; set; }
    public string EncodedInvoice { get; set; }
    public string QRCode { get; set; }
    public bool IsSigned { get; set; } = false;
    public int StatusCode { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
    public string? ZatcaStatus { get; set; }
    public string Type { get; set; } = "0100000";
    public string? ZatcaResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
}
using Microsoft.EntityFrameworkCore;

namespace ZATCA_V2.Models
{
    [Index(nameof(InvoiceHash), IsUnique = true)]
    [Index(nameof(UUID), IsUnique = true)]

    public class SignedInvoice
    {
        public int Id { get; set; }
        public string UUID { get; set; }
        public string InvoiceHash { get; set; }
        public string InvoiceType { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public string SingedXML { get; set; }
        public string EncodedInvoice { get; set; }
        public string QRCode { get; set; }
        public string SingedXMLFileName { get; set; }
        public int Status { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

    }
}
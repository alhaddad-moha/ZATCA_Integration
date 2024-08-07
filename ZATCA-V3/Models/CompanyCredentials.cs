namespace ZATCA_V3.Models
{
    // Models/CompanyCredentials.cs
    public class CompanyCredentials
    {
        public int Id { get; set; }
        public string? Certificate { get; set; }
        public string PrivateKey { get; set; }
        public string CSR { get; set; }
        public string? SecretToken { get; set; }
        public string? Secret { get; set; }
        public int? requestId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Company Company { get; set; }
    }
}
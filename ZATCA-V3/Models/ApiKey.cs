namespace ZATCA_V3.Models;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
}
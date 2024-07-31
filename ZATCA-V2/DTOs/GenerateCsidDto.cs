using System.ComponentModel.DataAnnotations;

namespace ZATCA_V2.DTOs;

public class GenerateCsidDto
{
    [Required(ErrorMessage = "OTP is required.")]
    public string Otp { get; set; }

    [Required(ErrorMessage = "Company Id is required.")]

    public int CompanyId { get; set; }
}
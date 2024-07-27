namespace ZATCA_V2.CustomValidators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class ValidDateFormatAttribute : ValidationAttribute
{
    private readonly string _dateFormat;

    public ValidDateFormatAttribute(string dateFormat)
    {
        _dateFormat = dateFormat;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null && DateTime.TryParseExact(value.ToString(), _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult($"The field {validationContext.DisplayName} must be a valid date in the format {_dateFormat}.");
    }
}

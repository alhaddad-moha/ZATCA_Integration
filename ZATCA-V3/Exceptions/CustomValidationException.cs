using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace ZATCA_V3.Exceptions
{
    public class CustomValidationException : ApplicationException
    {
        public List<string> Errors { get; set; } = new List<string>();


        public CustomValidationException(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
            {
                Errors.Add(error.ErrorMessage);
            }
        }
    }
}
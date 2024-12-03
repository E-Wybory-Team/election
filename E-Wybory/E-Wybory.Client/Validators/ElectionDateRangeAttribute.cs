using E_Wybory.Client.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;

public class ElectionDateRangeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not { })
            return ValidationResult.Success;

        var model = (ElectionViewModel)validationContext.ObjectInstance;

        if (model.ElectionEndDate < model.ElectionStartDate)
        {
            return new ValidationResult("Data zakończenia wyborów nie może być wcześniejsza niż data rozpoczęcia wyborów.");
        }

        return ValidationResult.Success;
    }
}

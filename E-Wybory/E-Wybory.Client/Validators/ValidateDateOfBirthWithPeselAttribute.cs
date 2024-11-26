using System;
using System.ComponentModel.DataAnnotations;

public class ValidateDateOfBirthWithPeselAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Sprawdź typ obiektu
        if (validationContext.ObjectInstance is not { })
        {
            return ValidationResult.Success;
        }

        dynamic model = validationContext.ObjectInstance;

        string pesel = model?.PESEL as string;
        string dateOfBirth = model?.DateOfBirthString as string;

        if (!string.IsNullOrEmpty(pesel) && pesel.Length == 11 && long.TryParse(pesel, out _) &&
            !string.IsNullOrEmpty(dateOfBirth) &&
            DateTime.TryParse(dateOfBirth, out DateTime parsedDate))
        {
            DateTime peselDate = GetDateOfBirthFromPesel(pesel);

            if (parsedDate != peselDate)
            {
                return new ValidationResult("Data urodzenia nie zgadza się z numerem PESEL.");
            }
        }

        return ValidationResult.Success;
    }

    private DateTime GetDateOfBirthFromPesel(string pesel)
    {
        int year = int.Parse(pesel.Substring(0, 2));
        int month = int.Parse(pesel.Substring(2, 2));
        int day = int.Parse(pesel.Substring(4, 2));

        if (month > 40) { year += 2100; month -= 40; }
        else if (month > 20) { year += 2000; month -= 20; }
        else { year += 1900; }

        return new DateTime(year, month, day);
    }
}

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class PersonViewModel
    {
        [Required] public int IdPerson { get; set; } = 0;
        [Required(ErrorMessage = "Imię jest obowiązkowe!")] public string Name { get; set; } = String.Empty;
        [Required(ErrorMessage = "Nazwisko jest obowiązkowe!")] public string Surname { get; set; } = String.Empty;

        [Required(ErrorMessage = "PESEL jest obowiązkowy!")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = "PESEL musi zawierać tylko cyfry.")] 
        public string PESEL { get; set; } = String.Empty;
        [Required(ErrorMessage = "Data urodzenia jest obowiązkowa!")] public DateTime BirthDate { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Tekstowa data urodzenia jest obowiązkowa!")]

        [ValidateDateOfBirthWithPesel]
        public string DateOfBirthString
        {
            get => BirthDate != DateTime.MinValue ? BirthDate.ToString("yyyy-MM-dd") : string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    BirthDate = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else
                {
                    BirthDate = DateTime.MinValue;
                }
            }
        }
    }
}

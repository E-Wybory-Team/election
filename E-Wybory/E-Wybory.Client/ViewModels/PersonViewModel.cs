using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class PersonViewModel
    {
        [Required] public int IdPerson { get; set; } = 0;
        [Required(ErrorMessage = "Imię jest obowiązkowe!")] public string Name { get; set; } = String.Empty;
        [Required(ErrorMessage = "Nazwisko jest obowiąakowe!")] public string Surname { get; set; } = String.Empty;
        [Required(ErrorMessage = "PESEL jest obowiązkowy!")] public string PESEL { get; set; } = String.Empty;
        [Required(ErrorMessage = "Data urodzenia jest obiowiązkowa!")] public DateTime BirthDate { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Tekstowa data urodzenia jest obowiązkowa!")]
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

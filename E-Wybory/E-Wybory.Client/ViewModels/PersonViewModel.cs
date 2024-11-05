using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class PersonViewModel
    {
        [Required] public int IdPerson { get; set; } = 0;
        [Required] public string Name { get; set; } = String.Empty;
        [Required] public string Surname { get; set; } = String.Empty;
        [Required] public string PESEL { get; set; } = String.Empty;
        [Required] public DateTime BirthDate { get; set; } = DateTime.MinValue;

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

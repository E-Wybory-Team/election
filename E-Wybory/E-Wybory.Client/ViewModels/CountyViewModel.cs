using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class CountyViewModel
    {
        [Required] public int IdCounty { get; set; } = 0;
        [Required] public string CountyName { get; set; } = String.Empty;
        [Required] public int IdVoivodeship { get; set; } = 0;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class ProvinceViewModel
    {
        [Required] public int IdProvince { get; set; } = 0;
        [Required] public string ProvinceName { get; set; } = String.Empty;
        [Required] public int IdCounty { get; set; } = 0;
    }
}

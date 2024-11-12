using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class DistrictViewModel
    {
        [Required(ErrorMessage ="Wybór obwodu jest obowiązkowy!")] public int IdDistrict { get; set; } = 0;
        [Required(ErrorMessage = "Nazwa obwodu jest obowiązkowa!")] public string DistrictName { get; set; }
        [Required] public bool DisabledFacilities { get; set; } = false;
        [Required(ErrorMessage = "Siedziba obwodu jest obowiązkowa!")] public string DistrictHeadquarters { get; set; } = String.Empty;
        [Required(ErrorMessage = "Okręg obowdu jest obowiązkowy!")] public int IdConstituency { get; set; } = 0;
        public int? IdProvince { get; set; } = null;
    }
}

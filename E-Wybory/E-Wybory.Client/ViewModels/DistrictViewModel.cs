using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class DistrictViewModel
    {
        [Required] public int IdDistrict { get; set; } = 0;
        [Required] public string DistrictName { get; set; }
        [Required] public bool DisabledFacilities { get; set; } = false;
        [Required] public string DistrictHeadquarters { get; set; } = String.Empty;
        [Required] public int IdConstituency { get; set; } = 0;
        public int IdProvince { get; set; } = 0;
    }
}

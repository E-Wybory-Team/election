using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class DistrictViewModel
    {
        [Required] public int IdDistrict { get; set; }
        [Required] public string DistrictName { get; set; }
        [Required] public bool DisabledFacilities { get; set; }
        [Required] public string DistrictHeadquarters { get; set; }
    }
}

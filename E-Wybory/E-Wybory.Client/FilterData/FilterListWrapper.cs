using E_Wybory.Client.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.FilterData
{
    public class FilterListWrapper
    {
        [Required]
        public List<VoivodeshipViewModel> VoivodeshipFilter { get; set; } = new List<VoivodeshipViewModel>();

        [Required]
        public List<CountyViewModel> CountyFilter { get; set; } = new List<CountyViewModel>();

        [Required]
        public List<ProvinceViewModel> ProvinceFilter { get; set; } = new List<ProvinceViewModel>();

        [Required]
        public List<DistrictViewModel> DistrictFilter { get; set; } = new List<DistrictViewModel>();
    }
}

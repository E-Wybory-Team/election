using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.FilterData
{
    public class FilterListWrapperDistrict
    {
        public List<ConstituencyViewModel> ConstituencyFilter { get; set; } = new List<ConstituencyViewModel>();

        public FilterListWrapper FilterListWrapper { get; set; } = new FilterListWrapper();
    }
}

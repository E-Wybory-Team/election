using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.FilterData
{
    public class FilterListWrapperFull
    {
        public List<ElectionTypeViewModel> ElectionFilter {  get; set; } = new List<ElectionTypeViewModel>();

        public FilterListWrapper FilterListWrapper { get; set; }  = new FilterListWrapper();
    }
}

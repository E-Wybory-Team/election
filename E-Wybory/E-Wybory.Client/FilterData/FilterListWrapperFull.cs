using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.FilterData
{
    public class FilterListWrapperFull
    {
        List<ElectionViewModelShort> ElectionFilter {  get; set; } = new List<ElectionViewModelShort>();

        public FilterListWrapper FilterListWrapper { get; set; }  = new FilterListWrapper();
    }
}

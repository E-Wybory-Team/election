using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ConstituencyViewModel
    {
            [Required] public int idConstituency { get; set; } = 0;
            [Required(ErrorMessage = "Nazwa okręgu jest obowiązkowa!")] public string constituencyName { get; set; } = String.Empty;
    }
}

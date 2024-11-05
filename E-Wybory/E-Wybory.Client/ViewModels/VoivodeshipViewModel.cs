using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class VoivodeshipViewModel
    {
        [Required] public int idVoivodeship { get; set; } = 0;
        [Required] public string voivodeshipName { get; set; } = String.Empty;
    }
}

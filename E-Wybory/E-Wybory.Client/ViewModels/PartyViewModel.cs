using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class PartyViewModel
    {
        [Required] public int IdParty { get; set; } = 0;
        [Required(ErrorMessage ="Nazwa partii jest obowiązkowa!")] public string PartyName { get; set; } = String.Empty;
        public string? Abbreviation { get; set; } = null;
        public string? PartyAddress { get; set; } = null;
        [Required(ErrorMessage = "Rodzaj partii jest obowiązkowy!")] public string PartyType { get; set; } = String.Empty;
        [Required(ErrorMessage ="Koalicyjnośc jest obowiązkowa!")] public bool IsCoalition { get; set; } = false;
        public int? ListCommiteeNumber { get; set; } = null;
        public string? Website { get; set; } = null;
    }
}

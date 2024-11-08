using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class PartyViewModel
    {
        [Required] public int IdParty { get; set; } = 0;
        [Required] public string PartyName { get; set; } = String.Empty;
        public string? Abbreviation { get; set; } = null;
        public string? PartyAddress { get; set; } = null;
        [Required] public string PartyType { get; set; } = String.Empty;
        [Required] public bool IsCoalition { get; set; } = false;
        public int? ListCommiteeNumber { get; set; } = null;
        public string? Website { get; set; } = null;
    }
}

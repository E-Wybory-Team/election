using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IPartyManagementService
    {
        Task<List<PartyViewModel>> Parties();
        string? GetPartyNameById(int partyId, List<PartyViewModel> parties);
    }
}

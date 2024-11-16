using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IPartyManagementService
    {
        Task<List<PartyViewModel>> Parties();
        string? GetPartyNameById(int partyId, List<PartyViewModel> parties);
        Task<List<PartyViewModel>> GetFilteredParties(int? electionTypeId);
        Task<bool> AddParty(PartyViewModel party);
        Task<bool> PutParty(PartyViewModel party);
        Task<bool> DeleteParty(int partyId);
        Task<bool> PartyExists(int partyId);
        Task<PartyViewModel> GetPartyById(int id);
    }
}

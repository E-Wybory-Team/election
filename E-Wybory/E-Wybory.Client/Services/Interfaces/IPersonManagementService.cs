using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IPersonManagementService
    {
        Task<List<PersonViewModel>> People();
        Task<int> GetPersonIdByPeselAsync(string pesel);
        Task<bool> AddPerson(PersonViewModel person);
        Task<bool> PutPerson(PersonViewModel person);
        Task<PersonViewModel> GetPersonById(int id);
        Task<String> GetPersonNameSurnameById(int id);
        int CountPersonAge(DateTime birthDate);
        Task<PersonViewModel> GetPersonIdByIdElectionUser(int electionUserId);
    }
}

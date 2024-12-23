﻿using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IVoterManagementService
    {
        Task<List<VoterViewModel>> Voters();
        Task<bool> AddVoter(VoterViewModel Voter);
        Task<bool> PutVoter(VoterViewModel Voter);
        Task<VoterViewModel> GetVoterById(int id);
        Task<bool> DeleteVoter(int VoterId);
        Task<bool> VoterExists(int voterId);
        Task<int> GetVoterIdByElectionUserId(int userId);
        Task<VoterViewModel> GetVoterByElectionUserId(int userId);
        Task<int> GetNumberVotersByDistrictId(int districtId);
        Task<bool> VoterOfUserExists(int userId);
    }
}

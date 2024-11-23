﻿using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IVoteManagementService
    {
        Task<List<VoteViewModel>> Votes();
        Task<bool> AddVote(VoteViewModel Vote);
        Task<VoteViewModel> GetVoteById(int id);
        Task<bool> DeleteVote(int VoteId);
        Task<List<VoteViewModel>> GetVotesIdByCandidateId(int candidateId);
        Task<List<VoteViewModel>> GetVotesByDistrictId(int districtId);
    }
}
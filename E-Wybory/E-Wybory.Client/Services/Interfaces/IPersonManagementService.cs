﻿using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IPersonManagementService
    {
        Task<List<PersonViewModel>> People();
        Task<int> GetPersonIdByPeselAsync(string pesel);
        Task<bool> AddPerson(PersonViewModel person);
        PersonViewModel? GetPersonById(int id, List<PersonViewModel> people);
        int CountPersonAge(DateTime birthDate);
    }
}

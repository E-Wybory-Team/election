﻿using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface ICountyManagementService
    {
        Task<List<CountyViewModel>> Counties();
        int? GetVoivodeshipIdFromCounty(int idCounty, List<CountyViewModel> counties);
    }
}

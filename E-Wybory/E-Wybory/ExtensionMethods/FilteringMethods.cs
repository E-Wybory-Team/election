using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using static E_Wybory.Client.Components.Pages.DetailedStats;

namespace E_Wybory.ExtensionMethods
{
    public static class FilteringMethods
    {
        public static Task<int> GetCandidateIdVoivodeship(ElectionDbContext _context, int idCandidate)
        {
            return _context.Candidates
                            .Where(candidate => candidate.IdCandidate == idCandidate)
                            .Include(p => p.IdDistrictNavigation.IdProvinceNavigation.IdCountyNavigation.IdVoivodeshipNavigation)
                            .Select(p => p.IdDistrictNavigation)
                            .Select(c => c.IdProvinceNavigation)
                            .Select(c => c.IdCountyNavigation)
                            .Select(c => c.IdVoivodeshipNavigation)
                            .Select(c => c.IdVoivodeship)
                            .FirstOrDefaultAsync();
        }

        public static Task<int> GetCandidateIdCounty(ElectionDbContext _context, int idCandidate)
        {
            return _context.Candidates
                            .Where(candidate => candidate.IdCandidate == idCandidate)
                            .Include(p => p.IdDistrictNavigation.IdProvinceNavigation.IdCountyNavigation)
                            .Select(p => p.IdDistrictNavigation)
                            .Select(c => c.IdProvinceNavigation)
                            .Select(c => c.IdCountyNavigation)
                            .Select(c => c.IdCounty)
                            .FirstOrDefaultAsync();
        }

        public static Task<int> GetCandidateIdProvince(ElectionDbContext _context, int idCandidate)
        {
            return _context.Candidates
                            .Where(candidate => candidate.IdCandidate == idCandidate)
                            .Include(p => p.IdDistrictNavigation.IdProvinceNavigation)
                            .Select(p => p.IdDistrictNavigation)
                            .Select(c => c.IdProvinceNavigation)
                            .Select(c => c.IdProvince)
                            .FirstOrDefaultAsync();
        }

        public static Task<int> GetCandidateIdElectionType(ElectionDbContext _context, int idCandidate)
        {
            return _context.Candidates
                            .Where(candidate => candidate.IdCandidate == idCandidate)
                            .Include(p => p.IdElectionNavigation.IdElectionTypeNavigation)
                            .Select(p => p.IdElectionNavigation)
                            .Select(c => c.IdElectionTypeNavigation)
                            .Select(c => c.IdElectionType)
                            .FirstOrDefaultAsync();
        }

        public static Task<int> GetDistrictIdVoivodeship(ElectionDbContext _context, int idDistrict)
        {
            return _context.Districts
                           .Where(district => district.IdDistrict == idDistrict)
                           .Include(p => p.IdProvinceNavigation.IdCountyNavigation.IdVoivodeshipNavigation)
                           .Select(c => c.IdProvinceNavigation)
                           .Select(c => c.IdCountyNavigation)
                           .Select(c => c.IdVoivodeshipNavigation)
                           .Select(c => c.IdVoivodeship)
                           .FirstOrDefaultAsync();
        }

        public static Task<int> GetDistrictIdCounty(ElectionDbContext _context, int idDistrict)
        {
            return _context.Districts
                            .Where(districts => districts.IdDistrict == idDistrict)
                            .Include(p => p.IdProvinceNavigation.IdCountyNavigation)
                            .Select(c => c.IdProvinceNavigation)
                            .Select(c => c.IdCountyNavigation)
                            .Select(c => c.IdCounty)
                            .FirstOrDefaultAsync();
        }

        public static Task<int> GetDistrictIdProvince(ElectionDbContext _context, int idDistrict)
        {
            return _context.Districts
                            .Where(district => district.IdDistrict == idDistrict)
                            .Include(p => p.IdProvinceNavigation)
                            .Select(c => c.IdProvinceNavigation)
                            .Select(c => c.IdProvince)
                            .FirstOrDefaultAsync();
        }

        public static Task<List<Voivodeship>> GetVoivodeshipsOfConstituency(ElectionDbContext _context, int idConstituency)
        {
            return _context.Districts
                .Where(district => district.IdConstituency == idConstituency)
                .SelectMany(district => _context.Provinces
                .Where(province => province.IdProvince == district.IdProvince)
                .SelectMany(province => _context.Counties
                .Where(county => county.IdCounty == province.IdCounty)
                .Select(county => county.IdVoivodeshipNavigation) 
                    )
                )
                .Distinct()
                .ToListAsync();
        }
    }
}

using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

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
    }
}

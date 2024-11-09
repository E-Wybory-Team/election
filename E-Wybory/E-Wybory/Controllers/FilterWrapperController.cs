using E_Wybory.Client.ViewModels;
using E_Wybory.Client.FilterData;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Matching;
using E_Wybory.ExtensionMethods;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterWrapperController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public FilterWrapperController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/FilterWrapper/Lists
        [HttpGet("Lists")]
        public async Task<ActionResult<FilterListWrapper>> GetFilteredLists(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId)
        {
            var filterListWrapper = new FilterListWrapper();

            if (filterListWrapper.VoivodeshipFilter.Count == 0) //when voivodeships are empty - at the beginning
            {
                filterListWrapper.VoivodeshipFilter = await _context.Voivodeships.Select(v => new VoivodeshipViewModel()
                {
                    idVoivodeship = v.IdVoivodeship,
                    voivodeshipName = v.VoivodeshipName
                }).ToListAsync();
            }

            if (voivodeshipId.HasValue)
            {
                filterListWrapper.CountyFilter = await _context.Counties
                    .Where(c => c.IdVoivodeship == voivodeshipId)
                    .Select(c => new CountyViewModel
                    {
                        IdCounty = c.IdCounty,
                        CountyName = c.CountyName,
                        IdVoivodeship = c.IdVoivodeship
                    })
                    .ToListAsync();
            }


            if (countyId.HasValue)
            {
                filterListWrapper.ProvinceFilter = await _context.Provinces
                    .Where(p => countyId == p.IdCounty)
                    .Select(p => new ProvinceViewModel
                    {
                        IdProvince = p.IdProvince,
                        IdCounty = p.IdCounty,
                        ProvinceName = p.ProvinceName
                    })
                    .ToListAsync();
            }


            if (provinceId.HasValue)
            {
                filterListWrapper.DistrictFilter = await _context.Districts
                    .Where(p => provinceId == p.IdProvince)
                    .Select(p => new DistrictViewModel
                    {
                        IdDistrict = p.IdDistrict,
                        DistrictName = p.DistrictName,
                        DistrictHeadquarters = p.DistrictHeadquarters,
                        IdProvince = p.IdProvince ?? 0
                    })
                    .ToListAsync();
            }
            return Ok(filterListWrapper);
        }

        // GET: api/FilterWrapper/Candidates
        [HttpGet("Candidates")]
        public async Task<ActionResult<List<CandidateViewModel>>> GetFilteredCandidates(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId,
            [FromQuery] int? districtId)
        {
            var candidates = await _context.Candidates
                .Include(candDist => candDist.IdDistrictNavigation)
                .ThenInclude(district => district.IdProvinceNavigation)
                .ThenInclude(province => province.IdCountyNavigation)
                .ThenInclude(county => county.IdVoivodeshipNavigation)
                .ToListAsync();

            var filteredCandidates = candidates.Where( candidate => 
                    ( voivodeshipId == null
                      || FilteringMethods.GetCandidateIdVoivodeship(_context, candidate.IdCandidate).Result == voivodeshipId)
                      &&
                      ( countyId == null 
                      || FilteringMethods.GetCandidateIdCounty(_context, candidate.IdCandidate).Result == countyId)
                      &&
                      ( provinceId == null
                      || FilteringMethods.GetCandidateIdProvince(_context, candidate.IdCandidate).Result == provinceId)
                      &&
                      (districtId == null
                      || candidate.IdDistrict == districtId)
                      )
                .Select(candidate => new CandidateViewModel
                {
                    IdCandidate = candidate.IdCandidate,
                    CampaignDescription = candidate.CampaignDescription,
                    EducationStatus = candidate.EducationStatus,
                    JobType = candidate.JobType,
                    PlaceOfResidence = candidate.PlaceOfResidence,
                    PositionNumber = candidate.PositionNumber,
                    Workplace = candidate.Workplace,
                    IdPerson = candidate.IdPerson,
                    IdDistrict = candidate.IdDistrict,
                    IdParty = candidate.IdParty,
                    IdElection = candidate.IdElection
                })
                .ToList();

            return Ok(filteredCandidates);
        }
    }
}

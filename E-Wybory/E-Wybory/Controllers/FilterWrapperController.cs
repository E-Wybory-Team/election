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
using System.Xml.Linq;

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

        [HttpGet("ElectionTypesList")]

        // GET: api/FilterWrapper/Lists
        [HttpGet("Lists")]
        public async Task<ActionResult<FilterListWrapperFull>> GetFilteredLists(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId)
        {
            var filterListWrapperFull = new FilterListWrapperFull();

            
            filterListWrapperFull.ElectionFilter = await _context.ElectionTypes.Select(v => new ElectionTypeViewModel()
                {
                    IdElectionType = v.IdElectionType,
                    ElectionTypeName = v.ElectionTypeName
                }).ToListAsync();
           

            if (filterListWrapperFull.FilterListWrapper.VoivodeshipFilter.Count == 0) 
            {
                filterListWrapperFull.FilterListWrapper.VoivodeshipFilter = await _context.Voivodeships.Select(v => new VoivodeshipViewModel()
                {
                    idVoivodeship = v.IdVoivodeship,
                    voivodeshipName = v.VoivodeshipName
                }).ToListAsync();
            }

            if (voivodeshipId.HasValue)
            {
                filterListWrapperFull.FilterListWrapper.CountyFilter = await _context.Counties
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
                filterListWrapperFull.FilterListWrapper.ProvinceFilter = await _context.Provinces
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
                filterListWrapperFull.FilterListWrapper.DistrictFilter = await _context.Districts
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
            return Ok(filterListWrapperFull);
        }

        // GET: api/FilterWrapper/Candidates
        [HttpGet("Candidates")]
        public async Task<ActionResult<List<CandidatePersonViewModel>>> GetFilteredCandidates(
            [FromQuery] int? electionTypeId,
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId,
            [FromQuery] int? districtId)
        {
            var candidates = await _context.Candidates
                .Include(candElection => candElection.IdElectionNavigation)
                .Include(candDist => candDist.IdDistrictNavigation)
                .ThenInclude(district => district.IdProvinceNavigation)
                .ThenInclude(province => province.IdCountyNavigation)
                .ThenInclude(county => county.IdVoivodeshipNavigation)
                .ToListAsync();

            var candidatePersonViewModels = new List<CandidatePersonViewModel>();

            foreach (var candidate in candidates)
            {
                if ((electionTypeId != null &&
                     await FilteringMethods.GetCandidateIdElectionType(_context, candidate.IdCandidate) != electionTypeId)
                    || (voivodeshipId != null &&
                     await FilteringMethods.GetCandidateIdVoivodeship(_context, candidate.IdCandidate) != voivodeshipId)
                    || (countyId != null &&
                     await FilteringMethods.GetCandidateIdCounty(_context, candidate.IdCandidate) != countyId)
                    || (provinceId != null &&
                     await FilteringMethods.GetCandidateIdProvince(_context, candidate.IdCandidate) != provinceId)
                    || (districtId != null && candidate.IdDistrict != districtId))
                {
                    // Skip this candidate if any of the filter conditions are not met
                    continue;
                }

                var personViewModel = await _context.People
                    .Where(p => p.IdPerson == candidate.IdPerson)
                    .Select(person => new PersonViewModel
                    {
                        IdPerson = person.IdPerson,
                        Name = person.Name,
                        Surname = person.Surname,
                        PESEL = person.Pesel,
                        BirthDate = person.BirthDate
                    })
                    .FirstOrDefaultAsync();

                candidatePersonViewModels.Add(new CandidatePersonViewModel
                {
                    candidateViewModel = new CandidateViewModel
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
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(candidatePersonViewModels);
        }
    }
}

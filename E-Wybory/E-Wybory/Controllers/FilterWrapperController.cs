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
using static E_Wybory.Client.Components.Pages.DetailedStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [AllowAnonymous]
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


        // GET: api/FilterWrapper/ListsWrapper
        [HttpGet("ListsWrapper")]
        [AllowAnonymous]//[Authorize(Roles = "Urzędnicy wyborczy, Administratorzy")]
        public async Task<ActionResult<FilterListWrapper>> GetFilteredListsWrapper(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId)
        {
            var filterListWrapper = new FilterListWrapper();


            filterListWrapper.VoivodeshipFilter = await _context.Voivodeships.Select(v => new VoivodeshipViewModel()
            {
                idVoivodeship = v.IdVoivodeship,
                voivodeshipName = v.VoivodeshipName
            }).ToListAsync();
            

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

        // GET: api/FilterWrapper/RegionLists
        [HttpGet("RegionLists")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]
        public async Task<ActionResult<FilterListWrapper>> GetFilteredLists(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId)
        {
            var filterListWrapper = new FilterListWrapper();


            if (filterListWrapper.VoivodeshipFilter.Count == 0)
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

            return Ok(filterListWrapper);
        }


        // GET: api/FilterWrapper/DistrictLists
        [HttpGet("DistrictLists")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<ActionResult<FilterListWrapper>> GetFilteredDistrictLists(
            [FromQuery] int? constituencyId,
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId)
        {
            var filterListWrapper = new FilterListWrapperDistrict();

            filterListWrapper.ConstituencyFilter = await _context.Constituences.Select(v => new ConstituencyViewModel()
            {
                idConstituency = v.IdConstituency,
                constituencyName= v.ConstituencyName
            }).ToListAsync();

            if (constituencyId.HasValue)
            {
                filterListWrapper.FilterListWrapper.VoivodeshipFilter = await _context.Districts
                .Where(district => district.IdConstituency == constituencyId)
                .SelectMany(district => _context.Provinces
                .Where(province => province.IdProvince == district.IdProvince)
                .SelectMany(province => _context.Counties
                .Where(county => county.IdCounty == province.IdCounty)
                .Select(county => county.IdVoivodeshipNavigation)
                )
                )
                .Distinct()
                .Select(voivodeship => new VoivodeshipViewModel
                {
                idVoivodeship= voivodeship.IdVoivodeship,
                voivodeshipName = voivodeship.VoivodeshipName
                })
                .ToListAsync();
            }

            if (voivodeshipId.HasValue)
            {
                filterListWrapper.FilterListWrapper.CountyFilter = await _context.Counties
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
                filterListWrapper.FilterListWrapper.ProvinceFilter = await _context.Provinces
                    .Where(p => countyId == p.IdCounty)
                    .Select(p => new ProvinceViewModel
                    {
                        IdProvince = p.IdProvince,
                        IdCounty = p.IdCounty,
                        ProvinceName = p.ProvinceName
                    })
                    .ToListAsync();
            }

            return Ok(filterListWrapper);
        }

        // GET: api/FilterWrapper/Candidates
        [HttpGet("Candidates")]
        [AllowAnonymous]
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
                var candidateElectionTypeId = candidate.IdElectionNavigation.IdElectionType;
                var candidateVoivodeshipId = candidate.IdDistrictNavigation?.IdProvinceNavigation?.IdCountyNavigation.IdVoivodeship;
                var candidateCountyId = candidate.IdDistrictNavigation?.IdProvinceNavigation?.IdCounty;
                var candidateProvinceId = candidate.IdDistrictNavigation?.IdProvince;

                // Filter conditions
                if ((electionTypeId != null && candidateElectionTypeId != electionTypeId) ||
                    (voivodeshipId != null && candidateVoivodeshipId != voivodeshipId) ||
                    (countyId != null && candidateCountyId != countyId) ||
                    (provinceId != null && candidateProvinceId != provinceId) ||
                    (districtId != null && candidate.IdDistrict != districtId))
                {
                    continue; // Skip this candidate if it doesn't match the filter conditions
                }

                // Retrieve person data for the current candidate
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

                // Add the candidate and their person data to the list
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
                        IdDistrict = candidate.IdDistrict.GetValueOrDefault(),
                        IdParty = candidate.IdParty,
                        IdElection = candidate.IdElection
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(candidatePersonViewModels);
        }



        // GET: api/FilterWrapper/Candidates
        [HttpGet("CandidatesElection")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy")]

        public async Task<ActionResult<List<CandidatePersonViewModel>>> GetFilteredCandidatesFromElection(
            [FromQuery] int? electionId,
            [FromQuery] int? districtId)
        {
            var candidatePersonViewModels = new List<CandidatePersonViewModel>();
            var candidates = await _context.Candidates.ToListAsync();

            foreach (var candidate in candidates)
            {
                // Filter conditions
                if ((electionId != null && candidate.IdElection != electionId) ||
                    (districtId != null && candidate.IdDistrict != districtId && candidate.IdDistrict != null))
                {
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
                        IdDistrict = candidate.IdDistrict.GetValueOrDefault(),
                        IdParty = candidate.IdParty,
                        IdElection = candidate.IdElection
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(candidatePersonViewModels);
        }


        // GET: api/FilterWrapper/Candidates
        [HttpGet("CandidatesElectionRegions")]
        //[Authorize(Roles = "Komisja wyborcza, Administratorzy")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CandidatePersonViewModel>>> GetFilteredCandidatesFromElectionRegions(
            [FromQuery] int? electionId,
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId,
            [FromQuery] int? districtId)
        {
            var candidatePersonViewModels = new List<CandidatePersonViewModel>();
            var candidates = await _context.Candidates
                .Include(candElection => candElection.IdElectionNavigation)
                .Include(candDist => candDist.IdDistrictNavigation)
                .ThenInclude(district => district.IdProvinceNavigation)
                .ThenInclude(province => province.IdCountyNavigation)
                .ThenInclude(county => county.IdVoivodeshipNavigation)
                .ToListAsync();

            foreach (var candidate in candidates)
            {
                var candidateElectionId = candidate.IdElection;
                var candidateVoivodeshipId = candidate.IdDistrictNavigation?.IdProvinceNavigation?.IdCountyNavigation.IdVoivodeship;
                var candidateCountyId = candidate.IdDistrictNavigation?.IdProvinceNavigation?.IdCounty;
                var candidateProvinceId = candidate.IdDistrictNavigation?.IdProvince;

                if (candidate.IdDistrict == null)
                {
                    if (electionId != null && candidateElectionId != electionId)
                    {
                        continue; // Skip this candidate only if electionId doesn't match
                    }
                }
                else
                {
                    if ((electionId != null && candidateElectionId != electionId) ||
                        (voivodeshipId != null && candidateVoivodeshipId != voivodeshipId) ||
                        (countyId != null && candidateCountyId != countyId) ||
                        (provinceId != null && candidateProvinceId != provinceId) ||
                        (districtId != null && candidate.IdDistrict != districtId))
                    {
                        continue; // Skip this candidate if it doesn't match the filter conditions
                    }
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
                        IdDistrict = candidate.IdDistrict.GetValueOrDefault(),
                        IdParty = candidate.IdParty,
                        IdElection = candidate.IdElection
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(candidatePersonViewModels);
        }



        // GET: api/FilterWrapper/Candidates
        [HttpGet("CandidatesWithoutRegions/{electionId}")]
        //[Authorize(Roles = "Komisja wyborcza, Administratorzy")]
        [AllowAnonymous]

        public async Task<ActionResult<List<CandidatePersonViewModel>>> GetFilteredCandidatesWithoutRegions(
            int electionId)
        {
            var candidatePersonViewModels = new List<CandidatePersonViewModel>();
            var candidates = await _context.Candidates.Where(candidate => candidate.IdElection == electionId && candidate.IdDistrict == null).ToListAsync();

            foreach (var candidate in candidates)
            {

                // Retrieve person data for the current candidate
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

                // Add the candidate and their person data to the list
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
                        IdDistrict = candidate.IdDistrict.GetValueOrDefault(),
                        IdParty = candidate.IdParty,
                        IdElection = candidate.IdElection
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(candidatePersonViewModels);
        }


        // GET: api/FilterWrapper/Districts
        [HttpGet("Districts")]
        [AllowAnonymous]//[Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<ActionResult<List<DistrictViewModel>>> GetFilteredDistricts(
        [FromQuery] int? constituencyId,
        [FromQuery] int? voivodeshipId,
        [FromQuery] int? countyId,
        [FromQuery] int? provinceId)
        {
            var districts = await _context.Districts
                .Include(district => district.IdConstituencyNavigation)
                .Include(district => district.IdProvinceNavigation)
                .ThenInclude(province => province.IdCountyNavigation)
                .ThenInclude(county => county.IdVoivodeshipNavigation)
                .ToListAsync();

            var filteredDistricts = new List<DistrictViewModel>();

            foreach (var district in districts)
            {
                // Check for null values in navigation properties before accessing them
                var districtConstituencyId = district.IdConstituency;
                var districtVoivodeshipId = district.IdProvinceNavigation?.IdCountyNavigation?.IdVoivodeshipNavigation?.IdVoivodeship;
                var districtCountyId = district.IdProvinceNavigation?.IdCountyNavigation?.IdCounty;
                var districtProvinceId = district.IdProvince;

                if ((constituencyId != null && districtConstituencyId != constituencyId) ||
                    (voivodeshipId != null && districtVoivodeshipId != voivodeshipId) ||
                    (countyId != null && districtCountyId != countyId) ||
                    (provinceId != null && districtProvinceId != provinceId))
                {
                    continue; // Skip this district if it doesn't match the filter
                }

                // Add to the filtered list if it meets the criteria
                filteredDistricts.Add(new DistrictViewModel
                {
                    IdDistrict = district.IdDistrict,
                    DistrictName = district.DistrictName,
                    DisabledFacilities = district.DisabledFacilities,
                    DistrictHeadquarters = district.DistrictHeadquarters,
                    IdConstituency = district.IdConstituency.GetValueOrDefault(),
                    IdProvince = district.IdProvince
                });
            }

            return Ok(filteredDistricts);
        }


        // GET: api/FilterWrapper/Users
        [HttpGet("Users")]
        [Authorize(Roles = "Urzędnicy wyborczy, Administratorzy")]
        public async Task<ActionResult<List<UserPersonViewModel>>> GetFilteredUsers(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId,
            [FromQuery] int? provinceId,
            [FromQuery] int? districtId)
        {
            var users = await _context.ElectionUsers
                .Include(user => user.Voter)
                .Include(user => user.IdDistrictNavigation)
                .ThenInclude(district => district.IdProvinceNavigation)
                .ThenInclude(province => province.IdCountyNavigation)
                .ThenInclude(county => county.IdVoivodeshipNavigation)
                .ToListAsync();

            var userPersonViewModels = new List<UserPersonViewModel>();

            foreach (var user in users)
            {
                var userVoivodeshipId = user.Voter?.IdDistrictNavigation?.IdProvinceNavigation?.IdCountyNavigation.IdVoivodeship;
                var userCountyId = user.Voter?.IdDistrictNavigation?.IdProvinceNavigation?.IdCounty;
                var userProvinceId = user.Voter?.IdDistrictNavigation?.IdProvince;
                var voterDistrictId = user.Voter?.IdDistrict;

                // Filter conditions
                if ( (user.Voter == null) ||
                    (voivodeshipId != null && userVoivodeshipId != voivodeshipId) ||
                    (countyId != null && userCountyId != countyId) ||
                    (provinceId != null && userProvinceId != provinceId) ||
                    (districtId != null && voterDistrictId != districtId))
                {
                    continue; // Skip this candidate if it doesn't match the filter conditions
                }

                // Retrieve person data for the current candidate
                var personViewModel = await _context.People
                    .Where(p => p.IdPerson == user.IdPerson)
                    .Select(person => new PersonViewModel
                    {
                        IdPerson = person.IdPerson,
                        Name = person.Name,
                        Surname = person.Surname,
                        PESEL = person.Pesel,
                        BirthDate = person.BirthDate
                    })
                    .FirstOrDefaultAsync();

                // Add the user and their person data to the list
                userPersonViewModels.Add(new UserPersonViewModel
                {
                    userViewModel = new ElectionUserViewModel
                    {
                        IdElectionUser = user.IdElectionUser,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Password = user.Password,
                        IdPerson = user.IdPerson,
                        IdDistrict = voterDistrictId!.GetValueOrDefault(),
                        UserSecret = user.UserSecret
                    },
                    personViewModel = personViewModel
                });
            }

            return Ok(userPersonViewModels);
        }



        // GET: api/FilterWrapper/PartiesCandidates
        [HttpGet("PartiesCandidates")]
        [AllowAnonymous] //?
        public async Task<ActionResult<List<CandidateViewModel>>> GetFilteredCandidatesFromParties(
            [FromQuery] int? partyId,
            [FromQuery] int? electionId)
        {
            var candidates = await _context.Candidates
                .Where(candidate => candidate.IdParty == partyId && candidate.IdElection == electionId)
                .ToListAsync();

            var candidateViewModels = new List<CandidateViewModel>();

            foreach (var candidate in candidates)
            {
                candidateViewModels.Add(new CandidateViewModel
                {
                    IdCandidate = candidate.IdCandidate,
                    CampaignDescription = candidate.CampaignDescription,
                    JobType = candidate.JobType,
                    PlaceOfResidence = candidate.PlaceOfResidence,
                    PositionNumber = candidate.PositionNumber,
                    EducationStatus = candidate.EducationStatus,
                    Workplace = candidate.Workplace,
                    IdPerson = candidate.IdPerson,
                    IdDistrict = candidate.IdDistrict,
                    IdParty = candidate.IdParty,
                    IdElection = candidate.IdElection
                });
            }
            return candidateViewModels;
        }

        [HttpGet("RegionsOfDistrict/{districtId}")]
        public async Task<ActionResult<List<string>>> GetRegionsOfDistrict(int districtId)
        {
            var districts = await _context.Districts
                            .Include(district => district.IdProvinceNavigation)
                            .ThenInclude(province => province.IdCountyNavigation)
                            .ThenInclude(county => county.IdVoivodeshipNavigation)
                            .ToListAsync();

            var regionList = new List<string>();

            if(!(await _context.Districts.AnyAsync(district => district.IdDistrict == districtId)))
            {
                return NotFound();
            }

            var district = districts.Where(district => district.IdDistrict == districtId).FirstOrDefault();
            var districtName = district.DistrictName;
            regionList.Add(districtName);

            if (district.IdProvince != null)
            {
                var provinceName = district.IdProvinceNavigation.ProvinceName;
                var countyName = district.IdProvinceNavigation.IdCountyNavigation.CountyName;
                var voivodeshipname = district.IdProvinceNavigation.IdCountyNavigation.IdVoivodeshipNavigation.VoivodeshipName;
                regionList.Add(provinceName);
                regionList.Add(countyName);
                regionList.Add(voivodeshipname);
                
            }
            else
            {
                regionList.Add(String.Empty);
                regionList.Add(String.Empty);
                regionList.Add(String.Empty);
            }
            return regionList;
        }
    }
}

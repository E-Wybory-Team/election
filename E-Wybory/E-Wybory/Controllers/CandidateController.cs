using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.ViewModels;
using System.Xml.Linq;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public CandidateController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Candidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates()
        {

            //Use Mapster or map manually 
            // _context.Candidates.Select(c => new CandidateViewModel() { IdCandidate = c.IdCandidate /*...*/ });



            return await _context.Candidates.ToListAsync();
        }

        // GET: api/Candidates/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<ActionResult<Candidate>> GetCandidate(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);

            if (candidate == null)
            {
                return NotFound();
            }

            return candidate;
        }

        // PUT: api/Candidates/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<IActionResult> PutCandidate(int id, [FromBody] CandidateViewModel candidateModel)
        {
            if (!EnteredRequiredData(candidateModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != candidateModel.IdCandidate || !CandidateExists(id))
            {
                return Conflict("Incorrect id");
            }

            var candidate = await _context.Candidates.FindAsync(id);

            if (candidate == null)
            {
                return NotFound();
            }

            candidate.CampaignDescription = candidateModel.CampaignDescription;
            candidate.PlaceOfResidence = candidateModel.PlaceOfResidence;
            candidate.JobType = candidateModel.JobType;
            candidate.PositionNumber = candidateModel.PositionNumber;
            candidate.EducationStatus = candidateModel.EducationStatus;
            candidate.Workplace = candidateModel.Workplace;
            candidate.IdDistrict = candidateModel.IdDistrict;
            candidate.IdPerson = candidateModel.IdPerson;
            candidate.IdParty = candidateModel.IdParty;
            candidate.IdElection = candidateModel.IdElection;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Impossible to execute in database");
            }
            return Ok();
        }

        // POST: api/candidates
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<ActionResult<Candidate>> PostCandidate([FromBody] CandidateViewModel candidateModel)
        {
            if (!EnteredRequiredData(candidateModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (_context.Candidates.Any(candidate => candidate.IdPerson == candidateModel.IdPerson) &&
               _context.Candidates.Any(candidate => candidate.IdElection == candidateModel.IdElection))
            {
                return Conflict("These data exists in database");
            }

            var candidate = new Candidate();

            candidate.CampaignDescription = candidateModel.CampaignDescription;
            candidate.PlaceOfResidence = candidateModel.PlaceOfResidence;
            candidate.JobType = candidateModel.JobType;
            candidate.PositionNumber = candidateModel.PositionNumber;
            candidate.EducationStatus = candidateModel.EducationStatus;
            candidate.Workplace = candidateModel.Workplace;
            candidate.IdDistrict = candidateModel.IdDistrict;
            candidate.IdPerson = candidateModel.IdPerson;
            candidate.IdParty = candidateModel.IdParty;
            candidate.IdElection = candidateModel.IdElection;

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCandidate", new { id = candidate.IdCandidate }, candidate);
        }

        // DELETE: api/candidates/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CandidateExists(int id)
        {
            return _context.Candidates.Any(e => e.IdCandidate == id);
        }

        private bool EnteredRequiredData(CandidateViewModel model)
        {
            if (model.JobType == String.Empty || model.PlaceOfResidence == String.Empty || model.PositionNumber == 0
                || model.IdPerson == 0 || model.IdElection == 0)
            {
                return false;
            }

            return true;
        }

        // GET: api/Candidate/exist/5
        [HttpGet("exist/{id}")]
        [Authorize(Roles = "Pracownicy PKW, Administratorzy")]

        public async Task<IActionResult> IfCandidateExists(int id)
        {
            if (CandidateExists(id))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet("ElectionDistrictCandidates/{electionId}/{districtId}")]
        [Authorize]
        public async Task<ActionResult<List<CandidateViewModel>>> GetCandidatesByElectionIdAndDistrict(int electionId, int districtId)
        {
            var Candidates = await _context.Candidates.Where(candidate => candidate.IdElection == electionId && (candidate.IdDistrict == districtId || candidate.IdDistrict == null)).ToListAsync<Candidate>();
            var CandidatesViewModels = new List<CandidateViewModel>();

            foreach (var Candidate in Candidates)
            {
                var CandidateModel = new CandidateViewModel()
                {
                    IdCandidate = Candidate.IdCandidate,
                    CampaignDescription = Candidate.CampaignDescription,
                    JobType = Candidate.JobType,
                    PlaceOfResidence = Candidate.PlaceOfResidence,
                    PositionNumber = Candidate.PositionNumber,
                    EducationStatus = Candidate.EducationStatus,
                    Workplace = Candidate.Workplace,
                    IdPerson = Candidate.IdPerson,
                    IdDistrict = Candidate.IdDistrict,
                    IdParty = Candidate.IdParty,
                    IdElection = Candidate.IdElection
                };

                CandidatesViewModels.Add(CandidateModel);

            }

            return CandidatesViewModels;
        }

    }
}


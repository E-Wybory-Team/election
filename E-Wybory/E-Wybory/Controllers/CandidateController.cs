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
        public async Task<IActionResult> PutCandidate(int id, Candidate candidate)
        {
            if (id != candidate.IdCandidate)
            {
                return Conflict();
            }

            _context.Entry(candidate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandidateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/candidates
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Candidate>> PostCandidate(CandidateViewModel candidateModel)
        {
            if(_context.Candidates.Any(candidate => candidate.IdPerson == candidateModel.IdPerson) &&
               _context.Candidates.Any(candidate => candidate.IdElection == candidateModel.IdElection))
            {
                return Conflict();
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
    }
}

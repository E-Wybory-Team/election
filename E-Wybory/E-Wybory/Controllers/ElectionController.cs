using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ElectionController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Election
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Election>>> GetElection()
        {
            return await _context.Elections.ToListAsync();
        }

        // GET: api/Election/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Election>> GetElection(int id)
        {
            var election = await _context.Elections.FindAsync(id);

            if (election == null)
            {
                return NotFound();
            }

            return election;
        }

        // PUT: api/Election/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElection(int id, ElectionViewModel electionModel)
        {
            if (!EnteredRequiredData(electionModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != electionModel.IdElection || !ElectionExists(id))
            {
                return Conflict();
            }

            var election = await _context.Elections.FindAsync(id);

            if (election == null)
            {
                return NotFound();
            }

            election.ElectionStartDate = electionModel.ElectionStartDate;
            election.ElectionEndDate = electionModel.ElectionEndDate;
            election.ElectionTour = electionModel.ElectionTour;
            election.IdElectionType = electionModel.IdElectionType;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Impossible to execute that in database");
            }
            return Ok();
        }

        // POST: api/Election
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Election>> PostElection([FromBody] ElectionViewModel electionModel)
        {
            if (!EnteredRequiredData(electionModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            var election = new Election();
            election.ElectionStartDate = electionModel.ElectionStartDate;
            election.ElectionEndDate = electionModel.ElectionEndDate;
            election.ElectionTour = electionModel.ElectionTour;
            election.IdElectionType = electionModel.IdElectionType;
            
            _context.Elections.Add(election);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetElection", new { id = election.IdElection }, election);
        }

        // DELETE: api/Election/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElection(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return NotFound();
            }

            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ElectionExists(int id)
        {
            return _context.Elections.Any(e => e.IdElection == id);
        }

        private bool EnteredRequiredData(ElectionViewModel electionModel)
        {
            if (electionModel.ElectionStartDate == DateTime.MinValue || electionModel.ElectionEndDate == DateTime.MinValue
                || electionModel.IdElectionType == 0)
            {
                return false;
            }

            return true;
        }
    }
}

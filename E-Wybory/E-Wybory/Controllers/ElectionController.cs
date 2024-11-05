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
        public async Task<IActionResult> PutElection(int id, Election election)
        {
            if (id != election.IdElection)
            {
                return Conflict();
            }

            _context.Entry(election).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ElectionExists(id))
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

        // POST: api/Election
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Election>> PostElection(ElectionViewModel electionModel)
        {
            var election = new Election();
            election.ElectionStartDate = electionModel.ElectionStart;
            election.ElectionEndDate = electionModel.ElectionEnd;
            election.ElectionTour = electionModel.ElectionTour;
            
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
    }
}

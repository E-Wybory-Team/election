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
    public class ConstituencyController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ConstituencyController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Constituences
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Constituence>>> GetConstituency()
        {
            return await _context.Constituences.ToListAsync();
        }

        // GET: api/Constituency/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Constituence>> GetConstituency(int id)
        {
            var constituency = await _context.Constituences.FindAsync(id);

            if (constituency == null)
            {
                return NotFound();
            }

            return constituency;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConstituency(int id, [FromBody] ConstituencyViewModel constituencyModel)
        {
            if (constituencyModel.constituencyName == String.Empty)
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != constituencyModel.idConstituency || !ConstituencyExists(id))
            {
                return Conflict("Incorrect id");
            }

            var constituency = await _context.Constituences.FindAsync(id);

            if (constituency == null)
            {
                return NotFound();
            }

            constituency.ConstituencyName = constituencyModel.constituencyName;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Impossible to execute it in database");
            }
            return Ok();
        }

        // POST: api/Constituency
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Constituence>> PostConstituency([FromBody] ConstituencyViewModel constituencyModel)
        {
            if (constituencyModel.constituencyName == String.Empty)
            {
                return BadRequest("Not entered data to all required fields");
            }

            var constituency = new Constituence();

            constituency.ConstituencyName = constituencyModel.constituencyName;
            
            _context.Constituences.Add(constituency);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getconstituency", new { id = constituency.IdConstituency }, constituency);
        }

        // DELETE: api/Constituency/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConstituency(int id)
        {
            var constituency = await _context.Constituences.FindAsync(id);
            if (constituency == null)
            {
                return NotFound();
            }

            _context.Constituences.Remove(constituency);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConstituencyExists(int id)
        {
            return _context.Constituences.Any(e => e.IdConstituency == id);
        }

    }
}

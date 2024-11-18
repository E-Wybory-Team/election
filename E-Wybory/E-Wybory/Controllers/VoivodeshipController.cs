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

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoivodeshipController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public VoivodeshipController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Voivodeships
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voivodeship>>> GetVoivodeships()
        {
            return await _context.Voivodeships.ToListAsync();
        }

        // GET: api/Voivodeships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voivodeship>> GetVoivodeship(int id)
        {
            var voivodeship = await _context.Voivodeships.FindAsync(id);

            if (voivodeship == null)
            {
                return NotFound();
            }

            return voivodeship;
        }

        // PUT: api/Voivodeships/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoivodeship(int id, Voivodeship voivodeship)
        {
            if (id != voivodeship.IdVoivodeship)
            {
                return Conflict();
                return Conflict();
            }

            _context.Entry(voivodeship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoivodeshipExists(id))
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

        // POST: api/Voivodeships
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Voivodeship>> PostVoivodeship(Voivodeship voivodeship)
        {
            _context.Voivodeships.Add(voivodeship);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVoivodeship", new { id = voivodeship.IdVoivodeship }, voivodeship);
        }

        // DELETE: api/Voivodeships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoivodeship(int id)
        {
            var voivodeship = await _context.Voivodeships.FindAsync(id);
            if (voivodeship == null)
            {
                return NotFound();
            }

            _context.Voivodeships.Remove(voivodeship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoivodeshipExists(int id)
        {
            return _context.Voivodeships.Any(e => e.IdVoivodeship == id);
        }
    }
}

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
    [Authorize(Roles = "Pracownicy PKW, Administratorzy, Komisja wyborcza")]
    public class ElectionTypeController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ElectionTypeController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/ElectionTypes
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ElectionType>>> GetElectionTypes()
        {
            return await _context.ElectionTypes.ToListAsync();
        }

        // GET: api/ElectionTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ElectionType>> GetElectionType(int id)
        {
            var ElectionType = await _context.ElectionTypes.FindAsync(id);

            if (ElectionType == null)
            {
                return NotFound();
            }

            return ElectionType;
        }


        // GET: api/ElectionTypes/name/5
        [HttpGet("name/{electionTypeId}")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetElectionTypeName(int electionTypeId)
        {
            var ElectionType = await _context.ElectionTypes.Where(type => type.IdElectionType == electionTypeId).FirstOrDefaultAsync();

            if (ElectionType == null)
            {
                return NotFound();
            }

            return Ok(ElectionType.ElectionTypeName);
        }

        // PUT: api/ElectionTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElectionType(int id, string name, ElectionType ElectionType)
        {
            if (id != ElectionType.IdElectionType || name == ElectionType.ElectionTypeName)
            {
                return Conflict();
            }

            _context.Entry(ElectionType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ElectionTypeExists(id, name))
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

        // POST: api/ElectionTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ElectionType>> PostElectionType(ElectionType ElectionType)
        {

            if(ElectionTypeExists(ElectionType.IdElectionType, ElectionType.ElectionTypeName))
            {
                return Conflict();
            }

            _context.ElectionTypes.Add(ElectionType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetElectionType", new { id = ElectionType.IdElectionType }, ElectionType);
        }

        // DELETE: api/ElectionTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElectionType(int id)
        {
            var ElectionType = await _context.ElectionTypes.FindAsync(id);
            if (ElectionType == null)
            {
                return NotFound();
            }

            _context.ElectionTypes.Remove(ElectionType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ElectionTypeExists(int id, string name)
        {
            return _context.ElectionTypes.Any(e => e.IdElectionType == id || e.ElectionTypeName == name);
        }
    }
}

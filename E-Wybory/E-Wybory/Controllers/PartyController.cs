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

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public PartyController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Parties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Party>>> GetParties()
        {
            return await _context.Parties.ToListAsync();
        }

        // GET: api/Parties/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Party>> GetParty(int id)
        {
            var party = await _context.Parties.FindAsync(id);

            if (party == null)
            {
                return NotFound();
            }

            return party;
        }

        // PUT: api/Parties/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParty(int id, string name, Party party)
        {
            if (id != party.IdParty || name == party.PartyName)
            {
                return Conflict();
            }

            _context.Entry(party).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartyExists(id, name))
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

        // POST: api/parties
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Party>> PostParty(Party party)
        {

            if(PartyExists(party.IdParty, party.PartyName))
            {
                return Conflict();
            }

            _context.Parties.Add(party);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParty", new { id = party.IdParty }, party);
        }

        // DELETE: api/parties/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParty(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null)
            {
                return NotFound();
            }

            _context.Parties.Remove(party);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PartyExists(int id, string name)
        {
            return _context.Parties.Any(e => e.IdParty == id || e.PartyName == name);
        }

        // POST: api/parties/name/2
        [HttpGet("name/{partyId}")]
        public string? GetPartyNameById(int partyId)
        {
            var partyName = _context.Parties
                .Where(p => p.IdParty == partyId)
                .Select(p => p.PartyName)
                .FirstOrDefault();

            return partyName;
        }
    }
}

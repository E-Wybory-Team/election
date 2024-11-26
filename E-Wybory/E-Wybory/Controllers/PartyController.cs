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
using System.Diagnostics.Eventing.Reader;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW")]

    public class PartyController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public PartyController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Parties
        [HttpGet]
        [AllowAnonymous]
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
        public async Task<IActionResult> PutParty(int id, [FromBody] PartyViewModel partyModel)
        {
            if (!EnteredRequiredData(partyModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != partyModel.IdParty || !PartyExists(id))
            {
                return Conflict("Incorrect id");
            }

            var partiesWithThisName = await _context.Parties.Where(party => party.PartyName.Equals(partyModel.PartyName) && party.IdParty != id).ToListAsync();
            Console.WriteLine($"Parties with this name: {partiesWithThisName.Count}");

            if (partiesWithThisName.Count > 0)
            {
                return Conflict("The party with this party already exists!");
            }

            var party = await _context.Parties.FindAsync(id);

            if (party == null)
            {
                return NotFound();
            }

            party.IdParty = partyModel.IdParty;
            party.PartyName = partyModel.PartyName;
            party.Abbreviation = partyModel.Abbreviation;
            party.PartyAddress = partyModel.PartyAddress;
            party.PartyType = partyModel.PartyType;
            party.IsCoalition = partyModel.IsCoalition;
            party.ListCommiteeNumber = partyModel.ListCommiteeNumber;
            party.Website = partyModel.Website;

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

        // POST: api/parties
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Party>> PostParty([FromBody] PartyViewModel partyModel)
        {

            if (!EnteredRequiredData(partyModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (PartyExists(partyModel.IdParty, partyModel.PartyName))
            {
                return Conflict("These data exists in database");
            }

            var party = new Party();

            party.IdParty = partyModel.IdParty;
            party.PartyName = partyModel.PartyName;
            party.Abbreviation = partyModel.Abbreviation;
            party.PartyAddress = partyModel.PartyAddress;
            party.PartyType = partyModel.PartyType;
            party.IsCoalition = partyModel.IsCoalition;
            party.ListCommiteeNumber = partyModel.ListCommiteeNumber;
            party.Website = partyModel.Website;

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

        private bool PartyExists(int id)
        {
            return _context.Parties.Any(e => e.IdParty == id);
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

        [HttpGet("Filter")]
        public async Task<ActionResult<List<PartyViewModel>>> GetFilteredParties([FromQuery] int? electionTypeId)
        {
            if(! _context.ElectionTypes.Any(e => e.IdElectionType == electionTypeId))
            {
                return NotFound("There is no election type with this ID!");
            }

            var query = await _context.Parties
                .Where(party => party.Candidates
                .Any(candidate => candidate.IdElectionNavigation.IdElectionTypeNavigation.IdElectionType == electionTypeId))
                .Select(party => new PartyViewModel
                {
                IdParty = party.IdParty,
                PartyName = party.PartyName,
                Abbreviation = party.Abbreviation,
                PartyAddress = party.PartyAddress,
                PartyType = party.PartyType,
                IsCoalition = party.IsCoalition,
                ListCommiteeNumber = party.ListCommiteeNumber,
                Website = party.Website
                })
                .OrderBy(partyViewModel => partyViewModel.IdParty)
                .ToListAsync();

            return Ok(query);

        }

        private bool EnteredRequiredData(PartyViewModel model)
        {
            if (model.PartyName == String.Empty || model.PartyType == String.Empty)
            {
                return false;
            }

            return true;
        }

        // GET: api/Party/exist/5
        [HttpGet("exist/{id}")]
        public async Task<IActionResult> IfPartyExists(int id)
        {
            if (PartyExists(id))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }
    }
}

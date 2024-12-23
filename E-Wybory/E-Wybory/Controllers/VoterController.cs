﻿using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VoterController : Controller
    {
        private readonly ElectionDbContext _context;

        public VoterController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Voters
        [HttpGet]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Urzędnicy wyborczy")]
        public async Task<ActionResult<IEnumerable<Voter>>> GetVoters()
        {
            return await _context.Voters.ToListAsync();
        }

        // GET: api/Voters/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Urzędnicy wyborczy")]
        public async Task<ActionResult<Voter>> GetVoter(int id)
        {
            var Voter = await _context.Voters.FindAsync(id);

            if (Voter == null)
            {
                return NotFound();
            }

            return Voter;
        }


        // PUT: api/Voters/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administratorzy, Urzędnicy wyborczy")]

        public async Task<IActionResult> PutVoter(int id, [FromBody] VoterViewModel VoterModel)
        {
            if (!EnteredRequiredData(VoterModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != VoterModel.IdVoter || !VoterExists(id))
            {
                return Conflict("Incorrect Voter's id");
            }

            var votersWithThisUser = await _context.Voters.Where(voter => voter.IdElectionUser == VoterModel.IdElectionUser && voter.IdVoter != VoterModel.IdVoter).ToListAsync();
            Console.WriteLine($"Voters with this user: {votersWithThisUser.Count}");

            if (votersWithThisUser.Count > 0)
            {
                return Conflict("The voter with this user already exists!");
            }

            var Voter = await _context.Voters.FindAsync(id);

            if (Voter == null)
            {
                return NotFound();
            }

            Voter.IdElectionUser = VoterModel.IdElectionUser;
            Voter.IdDistrict = VoterModel.IdDistrict;

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

        // POST: api/Voters
        [HttpPost]
        [Authorize(Roles = "Administratorzy, Urzędnicy wyborczy")]

        public async Task<ActionResult<Voter>> PostVoter([FromBody] VoterViewModel VoterModel)
        {
            if (!EnteredRequiredData(VoterModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (UserIsVoter(VoterModel.IdElectionUser))
            {
                return Conflict("This user is already saved as voter!");
            }

            var Voter = new Voter();
            Voter.IdElectionUser = VoterModel.IdElectionUser;
            Voter.IdDistrict = VoterModel.IdDistrict;

            _context.Voters.Add(Voter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVoter", new { id = Voter.IdVoter }, Voter);
        }

        // DELETE: api/Voters/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administratorzy, Urzędnicy wyborczy")]

        public async Task<IActionResult> DeleteVoter(int id)
        {
            var Voter = await _context.Voters.FindAsync(id);
            if (Voter == null)
            {
                return NotFound();
            }

            _context.Voters.Remove(Voter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoterExists(int id)
        {
            return _context.Voters.Any(e => e.IdVoter == id);
        }

        private bool UserIsVoter(int userId)
        {
            return _context.Voters.Any(e => e.IdElectionUser == userId);
        }

        private bool EnteredRequiredData(VoterViewModel voterModel)
        {
            if (voterModel.IdElectionUser == 0)
            {
                return false;
            }

            return true;
        }

        // GET: api/Voter/exist/5
        [HttpGet("exist/{id}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Urzędnicy wyborczy")]

        public async Task<IActionResult> IfVoterExists(int id)
        {
            if (VoterExists(id))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet("voter/{userId}")]
        [Authorize(Roles = "Administratorzy, Urzędnicy wyborczy")]

        public async Task<ActionResult<int>> GetVoterId(int userId)
        {
            var voter = await _context.Voters.Where(user => user.IdElectionUser == userId).FirstOrDefaultAsync();

            if (voter == null)
            {
                return 0;
            }


            return voter.IdVoter;
        }

        [HttpGet("voterRecord/{userId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy")]

        public async Task<ActionResult<VoterViewModel>> GetVoterByElectionUserId(int userId)
        {
            var voter = await _context.Voters.Where(user => user.IdElectionUser == userId).FirstOrDefaultAsync();

            if (voter != null)
            {
                var voterModel = new VoterViewModel()
                {
                    IdVoter = voter.IdVoter,
                    IdElectionUser = userId,
                    IdDistrict = voter.IdDistrict
                };

                return voterModel;
            }
            else
            {
                return NotFound("Not found that voter");
            }
        }

        [HttpGet("userOfVoterExist/{userId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy")]
        public async Task<ActionResult<bool>> VoterOfElectionUserExists(int userId)
        {
            if(await _context.Voters.AnyAsync(voter => voter.IdElectionUser == userId))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet("votersDistrict/{districtId}")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetNumberVotersByDistrictId(int districtId)
        {
            var voters = await _context.Voters.Where(voter => voter.IdDistrict == districtId).ToListAsync();

            return voters.Count();
        }

    }
}

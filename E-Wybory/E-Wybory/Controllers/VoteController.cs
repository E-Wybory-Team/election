using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static E_Wybory.Client.Components.Pages.DetailedStats;
using static E_Wybory.Client.Components.Pages.Statistics;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VoteController : Controller
    {
        private readonly ElectionDbContext _context;

        public VoteController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Votes
        [HttpGet]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]
        public async Task<ActionResult<IEnumerable<Domain.Entities.Vote>>> GetVotes()
        {
            return await _context.Votes.ToListAsync();
        }

        // GET: api/Votes/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]

        public async Task<ActionResult<Domain.Entities.Vote>> GetVote(int id)
        {
            var Vote = await _context.Votes.FindAsync(id);

            if (Vote == null)
            {
                return NotFound();
            }

            return Vote;
        }

        // POST: api/Votes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "2FAveryfiedUser", Policy="2FAenabled")]
        public async Task<ActionResult<Domain.Entities.Vote>> PostVote([FromBody] VoteViewModel VoteModel)
        {
            Console.WriteLine($"Value of candidate: {VoteModel.IdCandidate}");
            Console.WriteLine($"Value of election: {VoteModel.IdElection}");
            Console.WriteLine($"Value of District: {VoteModel.IdDistrict}");
            if (!EnteredRequiredData(VoteModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            var Vote = new Domain.Entities.Vote();
            Vote.IdVote = VoteModel.IdVote;
            Vote.IsValid = VoteModel.IsValid;
            Vote.IdCandidate = VoteModel.IdCandidate;
            Vote.IdElection = VoteModel.IdElection;
            Vote.IdDistrict = VoteModel.IdDistrict;

            _context.Votes.Add(Vote);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVote", new { id = Vote.IdVote }, Vote);
        }

        // DELETE: api/Votes/5
        //[HttpDelete("{id}")]
        //[Authorize(Roles = "Administratorzy")]
        //public async Task<IActionResult> DeleteVote(int id)
        //{
        //    var Vote = await _context.Votes.FindAsync(id);
        //    if (Vote == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Votes.Remove(Vote);
        //    await _context.SaveChangesAsync();

        //    //_context.Votes.Remove(Vote);
        //    //await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool VoteExists(int id)
        {
            return _context.Votes.Any(e => e.IdVote == id);
        }

        private bool EnteredRequiredData(VoteViewModel VoteModel)
        {
            if (VoteModel.IdCandidate == 0 || VoteModel.IdElection == 0 || VoteModel.IdDistrict == 0)
            {
                return false;
            }

            return true;
        }


        [HttpGet("VotesCandidate/{candidateId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]

        public async Task<ActionResult<List<VoteViewModel>>> GetVotesByCandidateId(int candidateId)
        {
            var Votes = await _context.Votes.Where(candidate => candidate.IdCandidate == candidateId).ToListAsync<Domain.Entities.Vote>();
            var VotesViewModels = new List<VoteViewModel>();

            foreach (var Vote in Votes)
            {
                    var VoteModel = new VoteViewModel()
                    {
                        IdVote = Vote.IdVote,
                        IsValid = true,
                        IdCandidate = candidateId,
                        IdElection = Vote.IdElection,
                        IdDistrict = Vote.IdDistrict
                    };

                    VotesViewModels.Add(VoteModel);
                
            }

            return VotesViewModels;
        }


        [HttpGet("VotesDistrict/{districtId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]

        public async Task<ActionResult<List<VoteViewModel>>> GetVoteByDistrictId(int districtId)
        {
            var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId).ToListAsync<Domain.Entities.Vote>();
            var VotesViewModels = new List<VoteViewModel>();

            foreach (var Vote in Votes)
            {
                var VoteModel = new VoteViewModel()
                {
                    IdVote = Vote.IdVote,
                    IsValid = true,
                    IdCandidate = Vote.IdCandidate,
                    IdElection = Vote.IdElection,
                    IdDistrict = districtId
                };

                VotesViewModels.Add(VoteModel);

            }

            return VotesViewModels;
        }
    }
}

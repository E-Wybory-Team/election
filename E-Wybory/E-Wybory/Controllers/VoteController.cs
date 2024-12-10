using E_Wybory.Application.Wrappers;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
        [HttpPost]
        [Authorize(Roles = "2FAveryfiedUser", Policy="2FAenabled")]
        public async Task<ActionResult<Domain.Entities.Vote>> PostVote([FromBody] VoteViewModel VoteModel)
        {
            if (!EnteredRequiredData(VoteModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            UserWrapper user = new UserWrapper(User);
            var voterId = 0;
            var voter = await _context.Voters.Where(voter => voter.IdElectionUser == user.Id).FirstOrDefaultAsync();

            if (voter == null)
            {
                return BadRequest("this user does not have set a voter row!");
            }

            var electionVoter = await _context.ElectionVoters.Where(elVoter => elVoter.IdVoter == voter.IdVoter && elVoter.IdElection == VoteModel.IdElection).FirstOrDefaultAsync();
            if (electionVoter != null)
            {
                return Conflict("This user already voted in this election!");
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


        [HttpGet("VotesCandidate/{candidateId}/{electionId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]
        public async Task<ActionResult<List<VoteViewModel>>> GetVotesByCandidateId(int candidateId, int electionId)
        {
            var Votes = await _context.Votes.Where(candidate => candidate.IdCandidate == candidateId && candidate.IdElection == electionId).ToListAsync<Domain.Entities.Vote>();
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


        [HttpGet("VotesDistrict/{districtId}/{electionId}")] //Szczegółowe wyniki dla obwodu
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]
        public async Task<ActionResult<List<VoteViewModel>>> GetVoteByDistrictId(int districtId, int electionId)
        {
            var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId).ToListAsync<Domain.Entities.Vote>();
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


        [HttpGet("VotesNumberDistrict/{districtId}/{electionId}")]
        [AllowAnonymous]

        public async Task<ActionResult<int>> GetVotesNumberByDistrictId(int districtId, int electionId)
        {
            var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId).ToListAsync<Domain.Entities.Vote>();

            return Votes.Count();
        }


        [HttpGet("ValidVotesNumberDistrict/{districtId}/{electionId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy")]

        public async Task<ActionResult<int>> GetValidVotesNumberByDistrictId(int districtId, int electionId)
        {
            var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId && vote.IsValid).ToListAsync<Domain.Entities.Vote>();
            return Votes.Count();
        }


        [HttpGet("InvalidVotesNumberDistrict/{districtId}/{electionId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy")]

        public async Task<ActionResult<int>> GetInvalidVotesNumberByDistrictId(int districtId, int electionId)
        {
            var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId && !vote.IsValid).ToListAsync<Domain.Entities.Vote>();
            return Votes.Count();
        }


        [HttpGet("VotesNumberDistrictCandidate/{districtId}/{electionId}/{candidateId}")]
        [AllowAnonymous]

        public async Task<ActionResult<int>> GetVotesNumberByDistrictCandidate(int districtId, int electionId, int candidateId)
        {
            if (await _context.Candidates.Where(candidate => candidate.IdCandidate == candidateId && candidate.IdElection == electionId && (candidate.IdDistrict == districtId || candidate.IdDistrict == null)).AnyAsync())
            {
                if (await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId && vote.IdCandidate == candidateId).AnyAsync())
                {
                    var Votes = await _context.Votes.Where(vote => vote.IdDistrict == districtId && vote.IdElection == electionId && vote.IdCandidate == candidateId && vote.IsValid).ToListAsync<Domain.Entities.Vote>();
                    return Votes.Count();
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return NotFound("Not found votes related to this candidate in this election");
            }
        }



        [HttpGet("frequency/{districtId}/{electionId}/{hourMax}")]
        [AllowAnonymous]
        public async Task<ActionResult<double>> GetFrequencyByDistrictIdToHour(int districtId, int electionId, int hourMax)
        {
            if (hourMax >= -1 && hourMax <= 24)
            {
                if (_context.Districts.Where(district => district.IdDistrict == districtId).Any() &&
                    _context.Elections.Where(election => election.IdElection == electionId).Any())
                {
                    var voters = await _context.Voters.Where(voter => voter.IdDistrict == districtId).ToListAsync();

                    if (voters.Count() == 0)
                    {
                        return 0.0;
                    }

                    var election = await _context.Elections.Where(election => election.IdElection == electionId).FirstOrDefaultAsync();
                    var electionMaxDate = new DateTime();
                    if (hourMax != -1)
                    {
                        var electionDate = election.ElectionStartDate;
                        electionMaxDate = new DateTime(electionDate.Year, electionDate.Month, electionDate.Day, hourMax, 0, 0);
                    }

                    var electionAttendants = 0.0;

                    foreach (var voterDistrict in voters)
                    {
                        var electionVoter = await _context.ElectionVoters.Where(voter => voterDistrict.IdVoter == voter.IdVoter && voter.IdElection == electionId).FirstOrDefaultAsync();
                        if (electionVoter != null)
                        {
                            if (hourMax != -1)
                            { 
                                var electionVoterDate = electionVoter.VoteTime;
                                if (electionVoter != null && electionVoterDate < electionMaxDate)
                                {
                                    electionAttendants++;
                                }
                            }
                            else
                            {
                                electionAttendants++;
                            }
                        }
                    }

                    return Math.Truncate((electionAttendants / (double)voters.Count()) * 100);
                }
                else
                {
                    return NotFound("Not found district or election");
                }
            }
            else
            {
                return BadRequest("Entered incorrect hour");
            }
        }

    }
}

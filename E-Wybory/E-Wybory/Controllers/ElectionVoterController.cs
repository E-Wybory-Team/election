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
    public class ElectionVoterController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ElectionVoterController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/ElectionVoters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElectionVoter>>> GetElectionVoters()
        {
            return await _context.ElectionVoters.ToListAsync();
        }

        // GET: api/ElectionVoters/5/2
        [HttpGet("{idVoter}/{idElection}")]
        public async Task<ActionResult<ElectionVoterViewModel>> GetElectionVoter(int idVoter, int idElection)
        {
            var ElectionVoter = _context.ElectionVoters.Where(elVoter => elVoter.IdVoter == idVoter && elVoter.IdElection == idElection).FirstOrDefault();

            if (ElectionVoter == null)
            {
                return NotFound("Not found this voter voting in this election");
            }

            ElectionVoterViewModel electionVoterViewModel = new ElectionVoterViewModel()
            {
                IdElectionVoter = ElectionVoter.IdElectionVoter,
                IdElection = ElectionVoter.IdElection,
                IdVoter = ElectionVoter.IdVoter,
                VoteTime = ElectionVoter.VoteTime
            };

            return electionVoterViewModel;
        }


        // POST: api/ElectionVoters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ElectionVoter>> PostElectionVoter(ElectionVoterViewModel electionVoterViewModel)
        {

            if(ElectionVoterExists(electionVoterViewModel.IdVoter, electionVoterViewModel.IdElection))
            {
                return Conflict();
            }

            if(!ElectionAndVoterExists(electionVoterViewModel.IdVoter, electionVoterViewModel.IdElection))
            {
                return NotFound("Not found that voter or election!");
            }

            ElectionVoter electionVoter = new()
            {
                IdElection = electionVoterViewModel.IdElection,
                IdVoter = electionVoterViewModel.IdVoter,
                VoteTime = electionVoterViewModel.VoteTime
            };

            _context.ElectionVoters.Add(electionVoter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetElectionVoter", new { idVoter = electionVoterViewModel.IdVoter, idElection = electionVoterViewModel.IdElection }, electionVoterViewModel);
        }

        private bool ElectionVoterExists(int idVoter, int idElection)
        {
            return _context.ElectionVoters.Any(e => e.IdVoter == idVoter && e.IdElection == idElection);
        }

        private bool ElectionAndVoterExists(int idVoter, int idElection)
        {
            return _context.Voters.Any(e => e.IdVoter == idVoter) 
                && _context.Elections.Any(e => e.IdElection == idElection);
        }

        [HttpGet("exist/{idVoter}/{idElection}")]
        public async Task<ActionResult<bool>> ElectionVoterAlreadyExists(int idVoter, int idElection)
        {
            return ElectionVoterExists(idVoter, idElection);
        }
    }
}

using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Election>>> GetElections()
        {
            return await _context.Elections.ToListAsync();
        }

        // GET: api/Election/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]
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
        [HttpPut("{id}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]
        public async Task<IActionResult> PutElection(int id, ElectionViewModel electionModel)
        {
            if (!EnteredRequiredData(electionModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != electionModel.IdElection || !ElectionExists(id))
            {
                return Conflict();
            }

            var election = await _context.Elections.FindAsync(id);

            if (election == null)
            {
                return NotFound();
            }

            election.ElectionStartDate = electionModel.ElectionStartDate;
            election.ElectionEndDate = electionModel.ElectionEndDate;
            election.ElectionTour = electionModel.ElectionTour;
            election.IdElectionType = electionModel.IdElectionType;

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

        // POST: api/Election
        [HttpPost]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]
        public async Task<ActionResult<Election>> PostElection([FromBody] ElectionViewModel electionModel)
        {
            if (!EnteredRequiredData(electionModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if(await ElectionOfTypeAtTimeExists(electionModel.IdElection, electionModel.IdElectionType, electionModel.ElectionStartDate, electionModel.ElectionEndDate))
            {
                return Conflict("Election of this type in this time already exists!");
            }

            var election = new Election();
            election.ElectionStartDate = electionModel.ElectionStartDate;
            election.ElectionEndDate = electionModel.ElectionEndDate;
            election.ElectionTour = electionModel.ElectionTour;
            election.IdElectionType = electionModel.IdElectionType;
            
            _context.Elections.Add(election);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetElection", new { id = election.IdElection }, election);
        }

        // DELETE: api/Election/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]
        public async Task<IActionResult> DeleteElection(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return NotFound();
            }

            if(await ElectionHasCandidate(id))
            {
                return Conflict("Cannot delete this election because at least one candidate is set to it!");
            }

            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/Election/active
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ElectionViewModel>>> GetActiveElections()
        {
            var currentDate = DateTime.UtcNow;

            var activeElections = await _context.Elections
                .Where(record => record.ElectionStartDate <= currentDate && record.ElectionEndDate >= currentDate)
                .Select(record => new ElectionViewModel
                {
                    IdElection = record.IdElection,
                    ElectionStartDate = record.ElectionStartDate,
                    ElectionEndDate = record.ElectionEndDate,
                    ElectionTour = record.ElectionTour.GetValueOrDefault(),
                    IdElectionType = record.IdElectionType
                })
                .ToListAsync();

            if (activeElections == null || activeElections.Count() == 0)
            {
                return NotFound("No active elections found.");
            }

            return Ok(activeElections);
        }

        // GET: api/Election/type
        [HttpGet("type/{electionTypeId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW")]

        public async Task<ActionResult<List<ElectionViewModel>>> GetElectionsOfType(int electionTypeId)
        {

            var typeElections = await _context.Elections
                .Where(record => record.IdElectionType == electionTypeId)
                .Select(record => new ElectionViewModel
                {
                    IdElection = record.IdElection,
                    ElectionStartDate = record.ElectionStartDate,
                    ElectionEndDate = record.ElectionEndDate,
                    ElectionTour = record.ElectionTour.GetValueOrDefault(),
                    IdElectionType = electionTypeId
                })
                .ToListAsync();

            if (typeElections == null || typeElections.Count() == 0)
            {
                return NotFound("No elections of this type found.");
            }

            return Ok(typeElections);
        }


        private bool ElectionExists(int id)
        {
            return _context.Elections.Any(e => e.IdElection == id);
        }

        private async Task<bool> ElectionHasCandidate(int electionId)
        {
            if(await  _context.Candidates.AnyAsync(candidate => candidate.IdElection == electionId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet("candidateNotSet/{electionId}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]
        public async Task<ActionResult> ElectionIsNotSetToCandidate(int electionId)
        {
            if(await ElectionHasCandidate(electionId))
            {
                return Conflict();
            }
            else
            {
                return Ok();
            }
        }

        private async Task<bool> ElectionOfTypeAtTimeExists(int electionId, int electionTypeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Elections.AnyAsync(e =>
                        e.IdElection != electionId &&
                        e.IdElectionType == electionTypeId && 
                        ((startDate >= e.ElectionStartDate && startDate <= e.ElectionEndDate) || 
                         (endDate >= e.ElectionStartDate && endDate <= e.ElectionEndDate) || 
                         (startDate <= e.ElectionStartDate && endDate >= e.ElectionEndDate)));
        }

        [HttpPut("typeTime")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW")]
        public async Task<ActionResult<bool>> ElectionOfTypeAtTimeAlreadyExists(ElectionViewModel election)
        {
            if(!(await ElectionOfTypeAtTimeExists(election.IdElection, election.IdElectionType, election.ElectionStartDate, election.ElectionEndDate)))
            {
                return Ok();
            }
            else
            {
                return NotFound("Not found election in this time of this type");
            }
        }

        private bool EnteredRequiredData(ElectionViewModel electionModel)
        {
            if (electionModel.ElectionStartDate == DateTime.MinValue || electionModel.ElectionEndDate == DateTime.MinValue
                || electionModel.IdElectionType == 0)
            {
                return false;
            }

            return true;
        }


        [HttpGet("newest")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ElectionViewModel>>> GetNewestElections()
        {
            var now = DateTime.Now;
            List<DateTime> dates = _context.Elections
                                    .Where(x => x.ElectionStartDate <= now )
                                    .Select(x => x.ElectionStartDate)
                                    .ToList();

            DateTime newestDate = dates.Max();
            Console.WriteLine($"Max date: {newestDate}");


            var activeElections = await _context.Elections
                .Where(record => record.ElectionStartDate == newestDate)
                .Select(record => new ElectionViewModel
                {
                    IdElection = record.IdElection,
                    ElectionStartDate = record.ElectionStartDate,
                    ElectionEndDate = record.ElectionEndDate,
                    ElectionTour = record.ElectionTour.GetValueOrDefault(),
                    IdElectionType = record.IdElectionType
                })
                .ToListAsync();

            return Ok(activeElections);
        }


        [HttpGet("newestAllTypes")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ElectionViewModel>>> GetNewestElectionsOfAllTypes()
        {
            var now = DateTime.Now;
            var electionTypes = await _context.ElectionTypes.ToListAsync();
            var elections = new List<ElectionViewModel>();

            foreach (var electionType in electionTypes)
            {
                var filteredElections = await _context.Elections
                    .Where(x => x.IdElectionType == electionType.IdElectionType && x.ElectionStartDate <= now)
                    .ToListAsync();

                var newestDate = filteredElections.MaxBy(e => e.ElectionStartDate)?.ElectionStartDate;

                if (newestDate != null)
                {
                    var newestElection = filteredElections
                        .Where(e => e.ElectionStartDate == newestDate)
                        .Select(record => new ElectionViewModel
                        {
                            IdElection = record.IdElection,
                            ElectionStartDate = record.ElectionStartDate,
                            ElectionEndDate = record.ElectionEndDate,
                            ElectionTour = record.ElectionTour.GetValueOrDefault(),
                            IdElectionType = record.IdElectionType
                        })
                        .FirstOrDefault();

                    if (newestElection != null)
                    {
                        elections.Add(newestElection);
                    }
                }
            }

            return Ok(elections);
        }


        [HttpGet("newest/{electionTypeId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ElectionViewModel>> GetNewestElectionOfType(int electionTypeId)
        {
            var now = DateTime.Now;
            List<DateTime> dates = _context.Elections
                                    .Where(x => x.IdElectionType == electionTypeId && x.ElectionEndDate <= now)
                                    .Select(x => x.ElectionEndDate)
                                    .ToList();

            if (dates.Count() > 0)
            {
                DateTime newestDate = dates.Max();


                var activeElections = await _context.Elections
                    .Where(record => record.IdElectionType == electionTypeId && record.ElectionEndDate == newestDate)
                    .Select(record => new ElectionViewModel
                    {
                        IdElection = record.IdElection,
                        ElectionStartDate = record.ElectionStartDate,
                        ElectionEndDate = record.ElectionEndDate,
                        ElectionTour = record.ElectionTour.GetValueOrDefault(),
                        IdElectionType = record.IdElectionType
                    })
                    .FirstOrDefaultAsync();

                return Ok(activeElections);
            }
            else
            {
                return NotFound("Not found elections of this type which were started");
            }
        }

    }
}

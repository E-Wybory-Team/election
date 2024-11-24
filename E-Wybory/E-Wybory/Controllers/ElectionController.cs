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
    public class ElectionController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ElectionController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Election
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Election>>> GetElection()
        {
            return await _context.Elections.ToListAsync();
        }

        // GET: api/Election/5
        [HttpGet("{id}")]
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Election>> PostElection([FromBody] ElectionViewModel electionModel)
        {
            if (!EnteredRequiredData(electionModel))
            {
                return BadRequest("Not entered data to all required fields");
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
        public async Task<IActionResult> DeleteElection(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return NotFound();
            }

            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/Election/active
        [HttpGet("active")]
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
        public async Task<ActionResult<List<ElectionViewModel>>> GetNewestElectionsOfAllTypes()
        {
            var now = DateTime.Now;
            var electionTypes = await _context.ElectionTypes.ToListAsync();
            var elections = new List<ElectionViewModel>();

            foreach (var electionType in electionTypes)
            {
                // Get all elections for this type that have started
                var filteredElections = await _context.Elections
                    .Where(x => x.IdElectionType == electionType.IdElectionType && x.ElectionStartDate <= now)
                    .ToListAsync();

                // Find the newest date if there are any matching elections
                var newestDate = filteredElections.MaxBy(e => e.ElectionStartDate)?.ElectionStartDate;

                if (newestDate != null)
                {
                    // Retrieve the election with the newest date
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
        public async Task<ActionResult<ElectionViewModel>> GetNewestElectionOfType(int electionTypeId)
        {
            var now = DateTime.Now;
            List<DateTime> dates = _context.Elections
                                    .Where(x => x.IdElectionType == electionTypeId && x.ElectionStartDate <= now)
                                    .Select(x => x.ElectionStartDate)
                                    .ToList();

            if (dates.Count() > 0)
            {
                DateTime newestDate = dates.Max();


                var activeElections = await _context.Elections
                    .Where(record => record.IdElectionType == electionTypeId && record.ElectionStartDate == newestDate)
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

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
    //[Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]
    [Authorize]
    public class PersonController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public PersonController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]


        public async Task<ActionResult<IEnumerable<Person>>> GetPeople()
        {
            return await _context.People.ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        [AllowAnonymous] //    [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]


        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]

        public async Task<IActionResult> PutPerson(int id, [FromBody] PersonViewModel personModel)
        {
            if (!EnteredRequiredData(personModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != personModel.IdPerson || !PersonExists(id))
            {
                return Conflict("Incorrect person's id");
            }

            var person = await _context.People.FindAsync(id);

            //when user tries to add existing PESEL
            if (person.Pesel != personModel.PESEL && _context.People.Any(e => e.Pesel == personModel.PESEL))
            {
                return Conflict("Person with entered PESEL already exists");
            }

            if (person == null)
            {
                return NotFound();
            }

            person.Name = personModel.Name;
            person.Surname = personModel.Surname;
            person.Pesel = personModel.PESEL;
            person.BirthDate = personModel.BirthDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            return Ok();
        }

        // POST: api/People
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]

        public async Task<ActionResult<Person>> PostPerson([FromBody] PersonViewModel personModel)
        {
            if (!EnteredRequiredData(personModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            var person = new Person();

            person.Name = personModel.Name;
            person.Surname = personModel.Surname;
            person.Pesel = personModel.PESEL;
            person.BirthDate = personModel.BirthDate;
            
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.IdPerson }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administratorzy, Pracownicy PKW")]

        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/People/idFromPesel/3
        [HttpGet("idFromPesel/{pesel}")]
        [AllowAnonymous] // [Authorize(Roles = "Komisja wyborcza, Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]

        public async Task<ActionResult<int>> GetPersonIdByPeselAsync(string pesel)
        {
            var person = await _context.People.Where(p => p.Pesel == pesel).FirstOrDefaultAsync();
            return Ok(person?.IdPerson ?? 0);
        }

        // GET: api/People/fromUser/3
        [HttpGet("fromUser/{electionUserId}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Urzędnicy wyborczy")]

        public async Task<ActionResult<PersonViewModel>> GetPersonViewModelByPeselAsync(int electionUserId)
        {
            var electionUser = await _context.ElectionUsers.Where(u => u.IdElectionUser == electionUserId).FirstOrDefaultAsync();
            if(electionUser == null)
            {
                return NotFound("Not found user with that ID");
            }
            var person = await _context.People.Where(p => p.IdPerson == electionUser.IdPerson).FirstOrDefaultAsync();
            if(person == null)
            {
                return NotFound("Not found that person");
            }

            var personModel = new PersonViewModel()
            {
                IdPerson = person.IdPerson,
                Name = person.Name,
                Surname = person.Surname,
                PESEL = person.Pesel,
                BirthDate = person.BirthDate
            };

            return Ok(personModel);
        }


        // GET: api/People/dataFromId/3
        [HttpGet("dataFromId/{idPerson}")]
        [Authorize(Roles = "Komisja wyborcza, Administratorzy, Urzędnicy wyborczy")]

        public async Task<ActionResult<string>> GetPersonDataByIdAsync(int idPerson)
        {
            var person = await _context.People.Where(p => p.IdPerson == idPerson).FirstOrDefaultAsync();
            var resultString = $"{person.Name} {person.Surname}";
            return Ok(resultString);
        }

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.IdPerson == id);
        }

        private bool EnteredRequiredData(PersonViewModel personModel)
        {
            if(personModel.Name == String.Empty || personModel.Surname == String.Empty 
                || personModel.PESEL == String.Empty || personModel.BirthDate == DateTime.MinValue)
            {
                return false;
            }

            return true;
        }
    }
}

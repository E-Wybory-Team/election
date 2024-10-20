using Microsoft.AspNetCore.Mvc;
using System.Threading;
using E_Wybory.Application;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionUserController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ElectionUserController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/ElectionUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElectionUser>>> GetElectionUsers()
        {
            return await _context.ElectionUsers.ToListAsync();
        }

        // GET: api/ElectionUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ElectionUser>> GetElectionUser(int id)
        {
            var electionUser = await _context.ElectionUsers.FindAsync(id);

            if (electionUser == null)
            {
                return NotFound();
            }

            return electionUser;
        }

        // PUT: api/ElectionUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElectionUser(int id, ElectionUser user)
        {
            if (id != user.IdElectionUser)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ElectionUserExists(id))
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

        // POST: api/ElectionUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ElectionUser>> PostElectionUser(ElectionUser user)
        {
            _context.ElectionUsers.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetElectionUser", new { id = user.IdElectionUser }, user);
        }

        // DELETE: api/ElectionUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElectionUser(int id)
        {
            var user = await _context.ElectionUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.ElectionUsers.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ElectionUserExists(int id)
        {
            return _context.ElectionUsers.Any(e => e.IdElectionUser == id);
        }
    }
}

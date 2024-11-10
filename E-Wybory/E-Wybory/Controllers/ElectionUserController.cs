using Microsoft.AspNetCore.Mvc;
using System.Threading;
using E_Wybory.Application;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionUserController : ControllerBase
    {
        private static UserTypeViewModel DefaultUserTypeViewModel { get => new UserTypeViewModel() { IdUserType = 0, UserTypeName = "Wyborca" }; }
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

        // GET: api/ElectionUsers/user-info
        [HttpGet("user-info")]
        [Authorize]
        public async Task<ActionResult<UserInfoViewModel>> GetUserInfo()
        {
            var claimIdElectionUser = User.FindFirst(c => c.Type.Equals("IdElectionUser"));
            

            if (claimIdElectionUser is null || string.IsNullOrEmpty(claimIdElectionUser.Value)) return Unauthorized("Identity claim missing.");


            int idElectionUser = Convert.ToInt32(claimIdElectionUser.Value);

            var userUserTypesCollection = await _context.UserTypeSets
                .Include(us => us.IdElectionUserNavigation.IdPersonNavigation)
                .Include(us => us.IdUserTypeNavigation)
                .Where(e => e.IdElectionUser == idElectionUser)
                .ToListAsync();

            if(userUserTypesCollection is null || userUserTypesCollection.Count == 0) return await GetUserInfoByDeafult(idElectionUser);

            var electionUser = userUserTypesCollection.Select(us => us.IdElectionUserNavigation).FirstOrDefault();

            if (electionUser == null)
            {
                return NotFound();
            }

            int idUserType = Convert.ToInt32(User.FindFirst(c => c.Type.Equals("IdUserType"))?.Value);
            var userTypeViewModels = userUserTypesCollection.Select(us => new UserTypeViewModel()
            {
                IdUserType = us.IdUserType,
                UserTypeName = us.IdUserTypeNavigation.UserTypeName
            });

            var currentUserType = userTypeViewModels.Where(u => u.IdUserType == idUserType).FirstOrDefault() ??
                DefaultUserTypeViewModel;

            return new UserInfoViewModel()
            {
                Name = electionUser.IdPersonNavigation.Name,
                Surname = electionUser.IdPersonNavigation.Surname,
                CurrentUserType = currentUserType,
                AvailableUserTypes = userTypeViewModels.Append(DefaultUserTypeViewModel).OrderBy(utv => utv.UserTypeName).ToList(),
                Username = electionUser.Email
            };
        }
        private async Task<ActionResult<UserInfoViewModel>> GetUserInfoByDeafult(int idElectionUser)
        {

            var electionUser = await _context.ElectionUsers.Include(e => e.IdPersonNavigation)
               .Where(e => e.IdElectionUser == idElectionUser)
               .FirstOrDefaultAsync();

            if (electionUser is null) return NotFound();



            return new UserInfoViewModel()
            {
                Name = electionUser.IdPersonNavigation.Name,
                Surname = electionUser.IdPersonNavigation.Surname,
                CurrentUserType = DefaultUserTypeViewModel,
                AvailableUserTypes = new List<UserTypeViewModel> { DefaultUserTypeViewModel},
                Username = electionUser.Email
            };

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

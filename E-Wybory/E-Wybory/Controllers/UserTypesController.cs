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
    public class UserTypeController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public UserTypeController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/UserTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTypeViewModel>>> GetUserTypes()
        {
            var viewModels = new List<UserTypeViewModel>();

            foreach(var UserType in await _context.UserTypes.ToListAsync())
            {
                viewModels.Add(new UserTypeViewModel()
                {
                    IdUserType = UserType.IdUserType,
                    UserTypeName = UserType.UserTypeName,
                    UserTypeInfo = UserType.UserTypeInfo,
                    IdUserTypesGroup = UserType.IdUserTypesGroup
                });
            }

            return viewModels;
        }

        // GET: api/UserTypesOfGroup
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<UserTypeViewModel>>> GetUserTypesOfGroup(int groupId)
        {
            var viewModels = new List<UserTypeViewModel>();

            foreach (var UserType in await _context.UserTypes.Where(userType => userType.IdUserTypesGroup == groupId).ToListAsync())
            {
                viewModels.Add(new UserTypeViewModel()
                {
                    IdUserType = UserType.IdUserType,
                    UserTypeName = UserType.UserTypeName,
                    UserTypeInfo = UserType.UserTypeInfo,
                    IdUserTypesGroup = UserType.IdUserTypesGroup
                });
            }

            return viewModels;
        }

        // GET: api/UserTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTypeViewModel>> GetUserType(int id)
        {
            var UserType = await _context.UserTypes.FindAsync(id);

            if (UserType == null)
            {
                return NotFound();
            }

            var UserTypeViewModel = new UserTypeViewModel()
            {
                IdUserType = UserType.IdUserType,
                UserTypeName = UserType.UserTypeName,
                UserTypeInfo = UserType.UserTypeInfo,
                IdUserTypesGroup = UserType.IdUserTypesGroup
            };

            return UserTypeViewModel;
        }

       
        // POST: api/UserTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserType>> PostUserType(UserTypeViewModel UserTypeViewModel)
        {
            if(UserTypeWithNameOfGroupExists(UserTypeViewModel.UserTypeName, UserTypeViewModel.IdUserTypesGroup))
            {
                return Conflict("This user type is already defined to this type's group!");
            }

            var UserType = new UserType()
            {
                IdUserType = UserTypeViewModel.IdUserType,
                UserTypeName = UserTypeViewModel.UserTypeName,
                UserTypeInfo = UserTypeViewModel.UserTypeInfo,
                IdUserTypesGroup = UserTypeViewModel.IdUserTypesGroup
            };

            _context.UserTypes.Add(UserType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserType", new { id = UserType.IdUserType }, UserType);
        }

        // DELETE: api/UserTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            var UserType = await _context.UserTypes.FindAsync(id);
            if (UserType == null)
            {
                return NotFound();
            }

            _context.UserTypes.Remove(UserType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTypeExists(int id)
        {
            return _context.UserTypes.Any(e => e.IdUserType == id);
        }

        private bool UserTypeWithNameOfGroupExists(string userTypeName, int userTypeGroupId)
        {
            return _context.UserTypes.Any(e => e.UserTypeName == userTypeName && e.IdUserTypesGroup == userTypeGroupId);
        }
    }
}

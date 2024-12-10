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
using Microsoft.AspNetCore.Routing.Matching;
using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administratorzy, Pracownicy PKW, Urzędnicy wyborczy")]

    public class UserTypesSetsController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public UserTypesSetsController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/UserTypesSets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTypeSetViewModel>>> GetUserTypesSets()
        {
            var viewModels = new List<UserTypeSetViewModel>();

            foreach(var userTypeSet in await _context.UserTypeSets.ToListAsync())
            {
                viewModels.Add(new UserTypeSetViewModel()
                {
                    IdUserTypeSet = userTypeSet.IdUserTypeSet,
                    IdElectionUser = userTypeSet.IdElectionUser,
                    IdUserType = userTypeSet.IdUserType
                });
            }

            return viewModels;
        }

        // GET: api/UserTypesSets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTypeSet>> GetUserTypeSet(int id)
        {
            var userTypeSet = await _context.UserTypeSets.FindAsync(id);

            if (userTypeSet == null)
            {
                return NotFound();
            }

            return userTypeSet;
        }


        // GET: api/UserTypesSets/5
        [HttpGet("commissioners/{districtId}")]
        public async Task<ActionResult<IEnumerable<CommissionerViewModel>>> GetUserTypeSetsOfCommisionersOfDistrict(int districtId)
        {
            var commissionersGroupName = "Komisja wyborcza";

            if(!(await _context.Districts.AnyAsync(district => district.IdDistrict == districtId)))
            {
                return NotFound("Not found that district");
            }

            if(!(await _context.UserTypesGroups.AnyAsync(group => group.UserTypesGroupName == commissionersGroupName)))
            {
                return NotFound("Not found that group with this name");
            }

            var userTypeSets = await _context.UserTypeSets
                .Include(set => set.IdElectionUserNavigation)
                .ThenInclude(user => user.IdPersonNavigation)
                .Include(set => set.IdUserTypeNavigation)
                .ThenInclude(type => type.IdUserTypesGroupNavigation)
                .ToListAsync();

            var commissionersViewModels = await _context.UserTypeSets
                .Include(set => set.IdElectionUserNavigation)
                    .ThenInclude(user => user.IdPersonNavigation)
                .Include(set => set.IdUserTypeNavigation)
                    .ThenInclude(type => type.IdUserTypesGroupNavigation)
                .Where(typeSet => typeSet.IdElectionUserNavigation.IdDistrict == districtId &&
                                    typeSet.IdUserTypeNavigation.IdUserTypesGroup == 2)
                .Select(typeSet => new CommissionerViewModel
                {
                    userType = new UserTypeViewModel
                    {
                        IdUserType = typeSet.IdUserTypeNavigation.IdUserType,
                        UserTypeName = typeSet.IdUserTypeNavigation.UserTypeName,
                        UserTypeInfo = typeSet.IdUserTypeNavigation.UserTypeInfo,
                        IdUserTypesGroup = typeSet.IdUserTypeNavigation.IdUserTypesGroup
                    },
                    userViewModel = new ElectionUserViewModel
                    {
                        IdElectionUser = typeSet.IdElectionUserNavigation.IdElectionUser,
                        Email = typeSet.IdElectionUserNavigation.Email,
                        PhoneNumber = typeSet.IdElectionUserNavigation.PhoneNumber,
                        Password = typeSet.IdElectionUserNavigation.Password,
                        IdPerson = typeSet.IdElectionUserNavigation.IdPerson,
                        IdDistrict = typeSet.IdElectionUserNavigation.IdDistrict,
                        UserSecret = typeSet.IdElectionUserNavigation.UserSecret
                    },
                    personViewModel = new PersonViewModel
                    {
                        IdPerson = typeSet.IdElectionUserNavigation.IdPersonNavigation.IdPerson,
                        Name = typeSet.IdElectionUserNavigation.IdPersonNavigation.Name,
                        Surname = typeSet.IdElectionUserNavigation.IdPersonNavigation.Surname,
                        PESEL = typeSet.IdElectionUserNavigation.IdPersonNavigation.Pesel,
                        BirthDate = typeSet.IdElectionUserNavigation.IdPersonNavigation.BirthDate
                    }
                })
                .ToListAsync();

            return commissionersViewModels;
        }


        // POST: api/UserTypeSets
        [HttpPost]
        public async Task<ActionResult<UserTypeSet>> PostUserTypeSet(UserTypeSetViewModel userTypeSetViewModel)
        {
            var user = await _context.ElectionUsers.FindAsync(userTypeSetViewModel.IdElectionUser);
            if(user == null || user.IdDistrict == 0)
            {
                return NotFound();
            }

            if(UserTypeGroupOfElectionUserExists(userTypeSetViewModel.IdUserType, userTypeSetViewModel.IdElectionUser))
            {
                return Conflict("This user is already set to this user type group!");
            }

            var userTypeSet = new UserTypeSet()
            {
                IdUserTypeSet = userTypeSetViewModel.IdUserTypeSet,
                IdElectionUser = userTypeSetViewModel.IdElectionUser,
                IdUserType = userTypeSetViewModel.IdUserType
            };

            _context.UserTypeSets.Add(userTypeSet);
            await _context.SaveChangesAsync();

            return Ok(); //there was problems with CreatedAtAction result
        }


        // PUT: api/UserTypeSets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTypeSet(int id, [FromBody] UserTypeSetViewModel userTypeSetViewModel)
        {

            if (id != userTypeSetViewModel.IdUserTypeSet || !UserTypeSetExists(id))
            {
                return Conflict("Incorrect id");
            }

            var UserTypeSet = await _context.UserTypeSets.FindAsync(id);

            if (UserTypeSet == null)
            {
                return NotFound();
            }


            UserTypeSet.IdUserTypeSet = userTypeSetViewModel.IdUserTypeSet;
            UserTypeSet.IdElectionUser = userTypeSetViewModel.IdElectionUser;
            UserTypeSet.IdUserType = userTypeSetViewModel.IdUserType;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Impossible to execute in database");
            }
            return Ok();
        }


        // DELETE: api/UserTypeSets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTypeSet(int id)
        {
            var userTypeSet = await _context.UserTypeSets.FindAsync(id);
            if (userTypeSet == null)
            {
                return NotFound();
            }

            _context.UserTypeSets.Remove(userTypeSet);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool UserTypeSetExists(int id)
        {
            return _context.UserTypeSets.Any(e => e.IdUserTypeSet == id);
        }

        private bool UserTypeGroupInSetExists(int typeGroupId)
        {
            return _context.UserTypeSets.Any(e => e.IdUserTypeNavigation.IdUserTypesGroup == typeGroupId);
        }

        private bool UserTypeGroupOfElectionUserExists(int userTypeId, int electionUserId)
        {
            var userType = _context.UserTypes.Where(type => type.IdUserType == userTypeId).FirstOrDefault();
            if (userType != null)
            {
            var userTypeGroup = userType.IdUserTypesGroup;
            return _context.UserTypeSets.Any(e => e.IdUserTypeNavigation.IdUserTypesGroup == userTypeGroup && e.IdElectionUser == electionUserId);
            }
            return false;
        }

        private bool TypeGroupOfElectionUserExists(int userTypeGroupId, int electionUserId)
        {
        return _context.UserTypeSets.Any(e => e.IdUserTypeNavigation.IdUserTypesGroup == userTypeGroupId && e.IdElectionUser == electionUserId);

        }

        [HttpGet("typeGroupUser/{userTypeId}/{electionUserId}")]
        public async Task<ActionResult<bool>> UserWithTypeGroupExists(int userTypeId, int electionUserId)
        {
            return UserTypeGroupOfElectionUserExists(userTypeId, electionUserId);
      
        }

        [HttpGet("setGroupUser/{typeGroupId}/{electionUserId}")]
        public async Task<ActionResult<UserTypeSetViewModel>> GetUserWithTypeGroupSet(int typeGroupId, int electionUserId)
        {
            if(!TypeGroupOfElectionUserExists(typeGroupId, electionUserId))
            {
                return new UserTypeSetViewModel(); //empty object
            }

            var set = await _context.UserTypeSets.Where(e => e.IdUserTypeNavigation.IdUserTypesGroup == typeGroupId 
                                            && e.IdElectionUser == electionUserId).FirstOrDefaultAsync();

            var userTypeSetViewModel = new UserTypeSetViewModel()
            {
                IdUserTypeSet = set.IdUserTypeSet,
                IdElectionUser = set.IdElectionUser,
                IdUserType = set.IdUserType
            };

            return userTypeSetViewModel;
        }

    }
}

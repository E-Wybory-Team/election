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
    public class DistrictController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public DistrictController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Districts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<District>>> GetDistricts()
        {
            return await _context.Districts.ToListAsync();
        }

        // GET: api/Districts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<District>> GetDistrict(int id)
        {
            var district = await _context.Districts.FindAsync(id);

            if (district == null)
            {
                return NotFound();
            }

            return district;
        }

        // PUT: api/Districts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, [FromBody] DistrictViewModel districtModel)
        {
            if (!EnteredRequiredData(districtModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            if (id != districtModel.IdDistrict || !DistrictExists(id))
            {
                return Conflict("Incorrect district's id");
            }

            var district = await _context.Districts.FindAsync(id);

            if (district == null)
            {
                return NotFound();
            }

            district.DistrictName = districtModel.DistrictName;
            district.DisabledFacilities = districtModel.DisabledFacilities;
            district.DistrictHeadquarters = districtModel.DistrictHeadquarters;
            district.IdConstituency = districtModel.IdConstituency;
            district.IdProvince = districtModel.IdProvince;

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

        // POST: api/Districts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<District>> PostDistrict([FromBody] DistrictViewModel districtModel)
        {
            if (!EnteredRequiredData(districtModel))
            {
                return BadRequest("Not entered data to all required fields");
            }

            var district = new District();

            district.DistrictName = districtModel.DistrictName;
            district.DisabledFacilities = districtModel.DisabledFacilities;
            district.DistrictHeadquarters = districtModel.DistrictHeadquarters;
            district.IdConstituency = districtModel.IdConstituency;
            district.IdProvince = districtModel.IdProvince;

            _context.Districts.Add(district);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDistrict", new { id = district.IdDistrict }, district);
        }

        // DELETE: api/Districts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDistrict(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                return NotFound();
            }

            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DistrictExists(int id)
        {
            return _context.Districts.Any(e => e.IdDistrict == id);
        }

        private bool EnteredRequiredData(DistrictViewModel districtModel)
        {
            if (districtModel.DistrictName == String.Empty || districtModel.DistrictHeadquarters == String.Empty
                 || districtModel.IdConstituency == 0 )
            {
                return false;
            }

            return true;
        }
    }
}

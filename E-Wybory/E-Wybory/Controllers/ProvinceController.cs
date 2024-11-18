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

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvinceController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public ProvinceController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/Provinces
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Province>>> GetProvinces()
        {
            return await _context.Provinces.ToListAsync();
        }

        // GET: api/Provinces/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Province>> GetProvince(int id)
        {
            var Province = await _context.Provinces.FindAsync(id);

            if (Province == null)
            {
                return NotFound();
            }

            return Province;
        }

        // PUT: api/Provinces/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProvince(int id, Province Province)
        {
            if (id != Province.IdProvince)
            {
                return BadRequest();
            }

            _context.Entry(Province).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvinceExists(id))
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

        // POST: api/Provinces
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Province>> PostProvince(Province Province)
        {
            _context.Provinces.Add(Province);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProvince", new { id = Province.IdProvince }, Province);
        }

        // DELETE: api/Provinces/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            var Province = await _context.Provinces.FindAsync(id);
            if (Province == null)
            {
                return NotFound();
            }

            _context.Provinces.Remove(Province);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProvinceExists(int id)
        {
            return _context.Provinces.Any(e => e.IdProvince == id);
        }
    }
}

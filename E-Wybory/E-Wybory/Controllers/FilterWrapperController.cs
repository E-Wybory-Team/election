using E_Wybory.Client.ViewModels;
using E_Wybory.Client.FilterData;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Wybory.Controllers
{
    [Route("api/get-filter")]
    [ApiController]
    public class FilterWrapperController : ControllerBase
    {
        private readonly ElectionDbContext _context;

        public FilterWrapperController(ElectionDbContext context)
        {
            _context = context;
        }

        // GET: api/get-filter
        [HttpGet]
        public async Task<ActionResult<FilterListWrapper>> GetFilteredLists(
            [FromQuery] int? voivodeshipId,
            [FromQuery] int? countyId)
        {
            var filterListWrapper = new FilterListWrapper();


            filterListWrapper.VoivodeshipFilter = await _context.Voivodeships.Select(v => new VoivodeshipViewModel()
            {
                idVoivodeship = v.IdVoivodeship,
                voivodeshipName = v.VoivodeshipName
            }).ToListAsync();


            if (!voivodeshipId.HasValue && !countyId.HasValue)
            {
               
                //Test to include navigator - u can use embedded navigation throughout Linq
                var testProvince = _context.Provinces
                   // .Include(p => p.IdCountyNavigation)
                    .Include(p => p.IdCountyNavigation.IdVoivodeshipNavigation)
                    .Select(p => p.IdCountyNavigation)
                    .Select(c => c.IdVoivodeshipNavigation)
                    .FirstOrDefault();

                //_context.Provinces.Join()
                Console.WriteLine(testProvince);

                return Ok(filterListWrapper);
            }

            if (voivodeshipId.HasValue)
            {

                filterListWrapper.CountyFilter = await _context.Counties
                    .Where(c => c.IdVoivodeship == voivodeshipId.Value)
                    .Select(c => new CountyViewModel
                    {
                        IdCounty = c.IdCounty,
                        CountyName = c.CountyName
                    })
                    .ToListAsync();

            }
           
          
            if (countyId.HasValue)
            {
                filterListWrapper.ProvinceFilter = await _context.Provinces
                    .Where(p => countyId == p.IdCounty)
                    .Select(p => new ProvinceViewModel
                    {
                        IdProvince = p.IdProvince,
                        IdCounty = p.IdCounty,
                        ProvinceName = p.ProvinceName
                    })
                    .ToListAsync();
            }
            

            return Ok(filterListWrapper);
        }
    }
}

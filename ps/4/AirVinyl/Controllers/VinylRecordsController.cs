using AirVinyl.API.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    public class VinylRecordsController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbcontext;

        public VinylRecordsController(AirVinylDbContext airVinylDbcontext)
        {
            _airVinylDbcontext = airVinylDbcontext
                ?? throw new ArgumentNullException(nameof(airVinylDbcontext));
        }

        [HttpGet("VinylRecords")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _airVinylDbcontext.VinylRecords.ToListAsync());
        }

        [HttpGet("odata/VinylRecords({key})")]
        public async Task<IActionResult> GetOneVinylRecord([FromRoute] int key)
        {
            var vinylRecord = await _airVinylDbcontext.VinylRecords
                .FirstOrDefaultAsync(v => v.VinylRecordId == key);

            if (vinylRecord == null)
            {
                return NotFound();
            }

            return Ok(vinylRecord);
        }
    }
}

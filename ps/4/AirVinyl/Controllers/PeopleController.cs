using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using AirVinyl.Entities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{

    public class PeopleController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbcontext;

        public PeopleController(AirVinylDbContext airVinylDbContext)
        {
            _airVinylDbcontext = airVinylDbContext ??
                throw new ArgumentNullException(nameof(airVinylDbContext));
        }

        public async Task<IActionResult> Get()
        {
            return Ok(await _airVinylDbcontext.People.ToListAsync());
        }

        //People(1)
        public async Task<IActionResult> Get(int key)
        {
            var person = await _airVinylDbcontext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [HttpGet("/odata/People({key})/Email")]
        [HttpGet("/odata/People({key})/FirstName")]
        [HttpGet("/odata/People({key})/LastName")]
        [HttpGet("/odata/People({key})/DateOfBirth")]
        [HttpGet("/odata/People({key})/Gender")]
        public async Task<IActionResult> GetPersonProperty(int key)
        {
            var person = await _airVinylDbcontext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = new Uri(HttpContext.Request.GetEncodedUrl()).Segments.Last();

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }

            return Ok(propertyValue);
        }

        [HttpGet("/odata/People({key})/Email/$value")]
        [HttpGet("/odata/People({key})/FirstName/$value")]
        [HttpGet("/odata/People({key})/LastName/$value")]
        [HttpGet("/odata/People({key})/DateOfBirth/$value")]
        [HttpGet("/odata/People({key})/Gender/$value")]
        public async Task<IActionResult> GetPersonPropertyRawValue(int key)
        {
            var person = await _airVinylDbcontext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var url = HttpContext.Request.GetEncodedUrl();

            var propertyToGet = new Uri(url).Segments[^2].TrimEnd('/');

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }

            return Ok(propertyValue.ToString());
        }

        //odata/People(key)/VinylRecords
        [HttpGet("odata/People({key})/VinylRecords")]
        //[HttpGet("odata/People({key})/Friends")]
        //[HttpGet("odata/People({key})/Addresses")]
        public async Task<IActionResult> GetPersonCollectionProperty(int key)
        {

            var collectionPropertyToGet = new Uri(HttpContext.Request.GetEncodedUrl())
                .Segments.Last();

            var person = await _airVinylDbcontext.People
                .Include(collectionPropertyToGet)
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            if (!person.HasProperty(collectionPropertyToGet))
            {
                return NotFound();
            }

            return Ok(person.GetValue(collectionPropertyToGet));
        }

        [HttpPost("odata/People")]
        public async Task<IActionResult> CreatePerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // add the person to the People Collection
            _airVinylDbcontext.People.Add(person);
            await _airVinylDbcontext.SaveChangesAsync();

            // return the created person
            return Created(person);
        }

        //Standard says that ID from Body will be ignored. The ID from URL will win.
        [HttpPut("odata/People({key})")]
        public async Task<IActionResult> UpdatePerson(int key, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = await _airVinylDbcontext.People
                                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();

                //Alternative: If the person is not found: Upsert. This must only
                //be used if the responsibility for creating the key isn't at
                //server-level. In our case, we're using auto-increment fields,
                //so this isn't allowed - code is for illustration purposes only!
                //if (currentPerson == null)
                //{
                // // the key from the URI is the hey we should use
                // person.PersonId = key;
                // _airVinylDbcontext.People.Add(person);
                // await _airVinylDbcontext.SaveChangesAsync();
                // return Created(person);
            }

            person.PersonId = currentPerson.PersonId;
            _airVinylDbcontext.Entry(currentPerson).CurrentValues.SetValues(person);
            await _airVinylDbcontext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("odata/People({key})")]
        public async Task<IActionResult> PartiallyUpdatePerson(int key,
            [FromBody] Delta<Person> patch)
        {
            var currentPerson = await _airVinylDbcontext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            patch.Patch(currentPerson);
            await _airVinylDbcontext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("odata/People({key})")]
        public async Task<IActionResult> DeletePerson(int key)
        {
            var person = await _airVinylDbcontext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            _airVinylDbcontext.People.Remove(person);
            await _airVinylDbcontext.SaveChangesAsync();

            return NoContent();
        }

    }
}

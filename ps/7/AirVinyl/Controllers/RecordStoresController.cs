using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using AirVinyl.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    [Route("odata")]
    public class RecordStoresController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbContext;

        public RecordStoresController(AirVinylDbContext airVinylDbContext)
        {
            _airVinylDbContext = airVinylDbContext
                ?? throw new ArgumentNullException(nameof(airVinylDbContext));
        }

        [EnableQuery]
        [HttpGet("RecordStores")]
        public IActionResult GetAllRecordStores()
        {
            return Ok(_airVinylDbContext.RecordStores);
        }

        [EnableQuery]

        public IActionResult GetOneRecordStore(int key)
        {
            var recordStores = _airVinylDbContext.RecordStores
                .Where(p => p.RecordStoreId == key);

            if (!recordStores.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(recordStores));
        }

        [HttpGet("RecordStores({key})/Tags")]
        [EnableQuery]
        public IActionResult GetRecordStoreTagsProperty(int key)
        {
            //no include necessary for EF Core - "Tags" isn't a navigation property
            //in the entity model.

            var recordStore = _airVinylDbContext.RecordStores
                .FirstOrDefault(p => p.RecordStoreId == key);

            if (recordStore == null)
            {
                return NotFound();
            }

            var collectionPropertyToGet = new Uri(HttpContext.Request.GetEncodedUrl())
                .Segments.Last();

            var collectionPropertyValue = recordStore.GetValue(collectionPropertyToGet); ;

            //return the collection of tags
            return Ok(collectionPropertyValue);
        }

        [HttpGet("RecordStores({key})/AiryVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
        public async Task<bool> IsHighRated(int key, int minimumRating)
        {
            //get the RecordStore
            var recordStore = await _airVinylDbContext.RecordStores
                .FirstOrDefaultAsync(p => p.RecordStoreId == key
                && p.Ratings.Any()
                && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);


            return recordStore != null;
        }

        [HttpGet("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={people})")]
        public async Task<IActionResult> AreRatedBy([FromODataUri] IEnumerable<int> people)
        {
            var recordStores = await _airVinylDbContext.RecordStores
                .Where(p => p.Ratings.Any(r => people.Contains(r.RatedBy.PersonId)))
                .ToListAsync();

            return Ok(recordStores);
        }

        [HttpGet("GetHighRatedRecordStores(minimumRating={minimumRating})")]
        public async Task<IActionResult> GetHighRatedRecordStores(int minimumRating)
        {
            var recordStores = await _airVinylDbContext.RecordStores
                .Where(p => p.Ratings.Any()
                && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating)
                .ToListAsync();

            return Ok(recordStores);
        }

        [HttpPost("RecordStores({id})/AirVinyl.Actions.Rate")]
        public async Task<IActionResult> Rate(int id, ODataActionParameters parameters)
        {
            //get the RecordStore
            var recordStore = await _airVinylDbContext.RecordStores
                .FirstOrDefaultAsync(p => p.RecordStoreId == id);

            if (recordStore == null)
            {
                return NotFound();
            }

            if (!parameters.TryGetValue("rating", out object outputFromDictionary))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out int rating))
            {
                return NotFound();
            }

            if (!parameters.TryGetValue("personId", out outputFromDictionary))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out int personId))
            {
                return NotFound();
            }

            // the peron must exist
            var person = await _airVinylDbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            // everything checks out, add the rating
            recordStore.Ratings.Add(new Rating()
            {
                RatedBy = person,
                Value = rating
            });

            // save changes
            if (await _airVinylDbContext.SaveChangesAsync() > 0)
            {
                //return true
                return Ok(true);
            }
            else
            {
                // Something went wrong - we expect our
                // action to return false in that case.
                // The request is still successful, false
                // is a valid response
                return Ok(false);
            }
        }

        [HttpPost("RecordStores/AirVinyl.Actions.RemoveRatings")]
        public async Task<IActionResult> RemoveRatings(ODataActionParameters parameters)
        {
            // from the param dictionary, get the personId
            if (!parameters.TryGetValue("personId", out object outputFromDictionary))
            {
                return BadRequest();
            }

            if (!int.TryParse(outputFromDictionary.ToString() , out int personId))
            {
                return BadRequest();
            }

            //get the RecordStore that were rated by the person with personId
            var recordStoresRatedByCurrentPerson = await _airVinylDbContext.RecordStores
                .Include("Ratings").Include("Ratings.RatedBy")
                .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId))
                .ToListAsync();

            //remove those ratings
            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                //get the rating by the current person
                var ratingsByCurrentPerson = store.Ratings
                    .Where(r => r.RatedBy.PersonId == personId)
                    .ToList();

                for (int i = 0; i < ratingsByCurrentPerson.Count(); i++) 
                {
                    store.Ratings.Remove(ratingsByCurrentPerson[i]);
                }
            }

            //save changes
            if (await _airVinylDbContext.SaveChangesAsync() > 0)
            {
                //return true
                return Ok(true);
            }
            else
            {
                //something went wrong - we expect our
                //action to return false in that case.
                //The request is still successful, false
                //is a valid response
                return Ok(false);
            }
        }

        [HttpPost("RemoveRecordStoreRatings")]
        public async Task<IActionResult> RemoveRecordSoreRatings(ODataActionParameters parameters)
        {
            // from the param dictionary, get the personId
            if (!parameters.TryGetValue("personId", out object outputFromDictionary))
            {
                return BadRequest();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out int personId))
            {
                return BadRequest();
            }

            //get the RecordStores that were rated by the person with personId
            var recordStoresRatedByCurrentPerson = await _airVinylDbContext.RecordStores
                .Include("Ratings").Include("Ratings.RatedBy")
                .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId))
                .ToListAsync();

            //remove those ratings
            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                //get the rating by the current person
                var ratingsByCurrentPerson = store.Ratings
                    .Where(r => r.RatedBy.PersonId == personId)
                    .ToList();

                for (int i = 0; i < ratingsByCurrentPerson.Count(); i++)
                {
                    store.Ratings.Remove(ratingsByCurrentPerson[i]);
                }
            }

            //save changes
            if (await _airVinylDbContext.SaveChangesAsync() > 0)
            {

                return NoContent();
            }
            else
            {
                //something went wrong               
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        //[HttpGet("RecordStores/AirVinyl.SpecializedRecordStore")]
        //public async Task<IActionResult> GetSpecializedRecordStores()
        //{
        //    var specializedRecordStores = await _airVinylDbContext.RecordStores
        //        .Where(r => r is SpecializedRecordStore)
        //        .ToListAsync();

        //    return Ok(specializedRecordStores);
        //}

        [EnableQuery]
        [HttpGet("RecordStores/AirVinyl.SpecializedRecordStore")]
        public IActionResult GetSpecializedRecordStores()
        {
            var specializedRecordStores = _airVinylDbContext.RecordStores
                .Where(r => r is SpecializedRecordStore);
                
            return Ok(specializedRecordStores.Select(s => s as SpecializedRecordStore));
        }

        //[HttpGet("RecordStores({id})/AirVinyl.SpecializedRecordStore")]
        //public async Task<IActionResult> GetSpecializedRecordStore(int id)
        //{
        //    var specializedRecordStore = await _airVinylDbContext.RecordStores
        //        .Where(r => r.RecordStoreId == id && r is SpecializedRecordStore)
        //        .ToListAsync();

        //    if (!specializedRecordStore.Any())
        //    {
        //        return NotFound();
        //    }

        //    return Ok(specializedRecordStore.Single());
        //}

        [EnableQuery]
        [HttpGet("RecordStores({id})/AirVinyl.SpecializedRecordStore")]
        public IActionResult GetSpecializedRecordStore(int id)
        {
            var specializedRecordStore = _airVinylDbContext.RecordStores
                .Where(r => r.RecordStoreId == id && r is SpecializedRecordStore);

            if (!specializedRecordStore.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(specializedRecordStore.Select(s => s as SpecializedRecordStore)));
        }

        [HttpPost("RecordStores")]
        public async Task<IActionResult> CreateRecordStore([FromBody] RecordStore recordStore)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // add the record store
            _airVinylDbContext.RecordStores.Add(recordStore);
            await _airVinylDbContext.SaveChangesAsync();

            //return the created RecordStore
            return Created(recordStore);
        }

        [HttpPatch("RecordStores({key})")]
        [HttpPatch("RecordStores({key})/AiryVinyl.SpecializedRecordStore")]
        public async Task<IActionResult> UpdateRecordStorePartially(int key, 
            [FromBody] Delta<RecordStore> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //find a matching store
            var currentRecordStore = await _airVinylDbContext.RecordStores
                .FirstOrDefaultAsync(p => p.RecordStoreId == key);

            //if the record store isn't found, return NotFound
            if (currentRecordStore == null)
            {
                return NotFound();
            }

            patch.Patch(currentRecordStore);
            await _airVinylDbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("RecordStores({key})")]
        [HttpDelete("RecordStores({key})/AirVinyl.SpecializedRecordStore")]
        public async Task<IActionResult> DeleteRecordStore(int key)
        {
            var currentRecordStore = await _airVinylDbContext.RecordStores
                .Include("Ratings")
                .FirstOrDefaultAsync(p => p.RecordStoreId == key);

            if (currentRecordStore == null)
            {
                return NotFound();
            }

            currentRecordStore.Ratings.Clear();
            _airVinylDbContext.RecordStores.Remove(currentRecordStore);
            await _airVinylDbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

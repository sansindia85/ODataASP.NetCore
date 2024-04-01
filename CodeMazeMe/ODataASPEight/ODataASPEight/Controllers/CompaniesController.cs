using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Extensions.Options;
using ODataASPEight.Database;
using ODataASPEight.Model;

namespace ODataASPEight.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepo _companyRepo;
        public CompaniesController(ICompanyRepo companyRepo)
        {
            _companyRepo = companyRepo;
        }

        //The[EnableQuery] attribute enables clients to send queries,
        //by using query options such as $filter, $sort, and $page.
        //By using IQueryable, OData will be able to query the list in
        //various ways.Note that we can also define a default page size (3)
        //for each request.
        [EnableQuery(PageSize = 3)]
        [HttpGet]
        public IQueryable<Company> Get()
        {
            return _companyRepo.GetAll();
        }

        [EnableQuery]
        [HttpGet("{id}")]
        public SingleResult<Company> Get([FromODataUri] int key)
        {
            return SingleResult.Create(_companyRepo.GetById(key));
        }

        [HttpPost]
        public IActionResult Post([FromBody] Company company)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _companyRepo.Create(company);

            return Created("Companies", company);
        }

        [HttpPut]
        public IActionResult Put([FromODataUri] int key, [FromBody] Company company)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != company.ID)
            {
                return BadRequest();
            }

            _companyRepo.Update(company);

            return NoContent();
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key)
        {
            var company = _companyRepo.GetById(key);

            if (company is null)
            {
                return BadRequest();
            }

            _companyRepo.Delete(company.First());

            return NoContent();
        }
    }
}

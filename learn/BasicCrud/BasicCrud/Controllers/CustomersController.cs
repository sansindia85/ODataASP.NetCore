using BasicCrud.Data;
using BasicCrud.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace BasicCrud.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly BasicCrudDbContext db;

        public CustomersController(BasicCrudDbContext db)
        {
            this.db = db;
        }

        public ActionResult<IQueryable<Customer>> Get()
        {
            return Ok(db.Customers);
        }
    }
}

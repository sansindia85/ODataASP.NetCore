using BasicCrud.Models;
using Microsoft.EntityFrameworkCore;

namespace BasicCrud.Data
{
    public class BasicCrudDbContext : DbContext
    {
        public BasicCrudDbContext(DbContextOptions<BasicCrudDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}

using BasicCrud.Models;

namespace BasicCrud.Data
{
    public class BasicCrudDbHelper
    {
        public static void SeedDb(BasicCrudDbContext db)
        {
            if (!db.Customers.Any())
            {
                db.Add(new Customer
                {
                    Id = 1,
                    Name = "Sue",
                    CustomerType = CustomerType.Retail,
                    CreditLimit = 3700,
                    CustomerSince = new DateTime(2022, 7, 4)
                });

                db.Add(new Customer
                {
                    Id = 2,
                    Name = "Joe",
                    CustomerType = CustomerType.Wholesale,
                    CreditLimit = 5100,
                    CustomerSince = new DateTime(2022, 12, 12)
                });

                db.SaveChanges();
            }

        }
    }
}

using System.Data.Entity;

namespace sportmaster_parser
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }

//        protected override void OnModelCreating(DbModelBuilder ModelBuilder)
//        {
//            
//        }
    }
}
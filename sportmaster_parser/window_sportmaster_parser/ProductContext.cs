using System.Data.Entity;

namespace window_sportmaster_parser
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
    }
}
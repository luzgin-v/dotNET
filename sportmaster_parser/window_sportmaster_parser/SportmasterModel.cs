using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Entity;

namespace window_sportmaster_parser
{
    public partial class SportmasterModel
    {
        public class ProductOfSearch
        {
            public string SportmasterId { get; set; }
            public string Name { get; set; }

        }

        public class Product
        {
            [Key]
            public string SportmasterId { get; set; }
            public string ProductName { get; set; }
            public string ProductPrice { get; set; }
            public virtual Manufacturer Manufacturer { get; set; }
        }

        public class Manufacturer
        {
            [Key]
            public string ManufacturerName { get; set; }
            public virtual ICollection<Product> Products { get; set; }
        }

        public class ProductContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Manufacturer> Manufacturers { get; set; }
        }
    }
}
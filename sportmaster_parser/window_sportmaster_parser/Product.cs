using System.ComponentModel.DataAnnotations;

namespace window_sportmaster_parser
{
    public class Product
    {
        [Key]
        public string SportmasterId { get; set; }
        public string ProductName { get; set; }
        public string ProductPrice { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
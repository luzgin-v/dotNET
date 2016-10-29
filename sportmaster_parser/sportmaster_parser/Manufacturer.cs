using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sportmaster_parser
{
    public class Manufacturer
    {
        [Key]
        public string ManufacturerName { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
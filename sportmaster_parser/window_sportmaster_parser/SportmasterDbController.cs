using System.Collections.ObjectModel;
using System.Linq;
using static window_sportmaster_parser.SportmasterModel;

namespace window_sportmaster_parser
{
    public static class SportmasterDbController
    {
        public static bool AddFieldToDb(Product displayedProduct)
        {
            bool manufacturerHasAdd = false;
            using (var db = new ProductContext())
            {
                var newManufacturer = db.Manufacturers.Find(displayedProduct.Manufacturer.ManufacturerName);
                if (newManufacturer == null)
                {
                    manufacturerHasAdd = true;
                    newManufacturer = new Manufacturer
                    {
                        ManufacturerName = displayedProduct.Manufacturer.ManufacturerName
                    };
                    db.Manufacturers.Add(newManufacturer);
                    db.SaveChanges();
                }
                var newProduct = displayedProduct;
                newProduct.Manufacturer = newManufacturer;
                if (
                    !db.Products.Where(p => p.SportmasterId == displayedProduct.SportmasterId)
                        .Select(c => c)
                        .Any())
                {
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                }
            }
            return manufacturerHasAdd;
        }

        public static ObservableCollection<Product> FillProductsOfManufacturer(Manufacturer selectedManufacturer)
        {
            using (var db = new ProductContext())
            {
                return new ObservableCollection<Product>(db.Products.Where(p => p.Manufacturer.ManufacturerName == selectedManufacturer.ManufacturerName).Select(c => c).ToList());
            }
        }

        public static ObservableCollection<Manufacturer> FillManufacturers()
        {
            using (var db = new ProductContext())
            {
                return new ObservableCollection<Manufacturer>(db.Manufacturers.ToList());
            }
        }
    }
}
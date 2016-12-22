using LiteDB;

namespace webAPI_sportmaster_parser.Models
{
    public class SportmasterDbController
    {
        private static string _pathToDb = @"C:\Users\Victor\Documents\dotNET\sportmaster_parser\webAPI_sportmaster_parser\cache.db";
        public static CachedProduct GetCachedProduct(string id)
        {
            using (var db = new LiteDatabase(_pathToDb))
            {
                var products = db.GetCollection<CachedProduct>("products");
                var result = products.FindOne(x => x.SportmasterId.Equals(id));
                return result;
            }
        }

        public static void AddNewProduct(CachedProduct newCachedProduct)
        {
            using (var db = new LiteDatabase(_pathToDb))
            {
                var products = db.GetCollection<CachedProduct>("products");
                products.Insert(newCachedProduct);
            }
        }
    }
}
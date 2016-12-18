using LiteDB;

namespace WcfSportmaster
{
    public static class SportmasterDbController
    {
        private static string _pathToDb = @"C:\Users\Victor\Documents\dotNET\WcfSportmaster\WcfSportmaster\cache.db";
        public static SportmasterModel.CachedProduct GetCachedProduct(string id)
        {
            using (var db = new LiteDatabase(_pathToDb))
            {
                var products = db.GetCollection<SportmasterModel.CachedProduct>("products");
                var result = products.FindOne(x => x.SportmasterId.Equals(id));
                return result;
            }
        }

        public static void AddNewProduct(SportmasterModel.CachedProduct newCachedProduct)
        {
            using (var db = new LiteDatabase(_pathToDb))
            {
                var products = db.GetCollection<SportmasterModel.CachedProduct>("products");
                products.Insert(newCachedProduct);
            }
        }
    }
}
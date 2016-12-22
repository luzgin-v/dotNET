namespace WcfSportmaster
{
    public class SportmasterService : ISportmasterService
    {
        public SportmasterModel.ProductOfSearch[] GetProductsByQuery(string query)
        {
            return SportmasterParseController.GetProductsWithSearch(query).ToArray();
        }

        public SportmasterModel.CachedProduct GetProductById(string id)
        {
            return SportmasterDbController.GetCachedProduct(id);
        }
    }
}

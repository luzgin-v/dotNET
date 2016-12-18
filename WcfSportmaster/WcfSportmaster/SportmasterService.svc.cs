using System.Collections.Generic;

namespace WcfSportmaster
{
    public class SportmasterService : ISportmasterService
    {
        public List<SportmasterModel.ProductOfSearch> GetProductsByQuery(string query)
        {
            return SportmasterParseController.GetProductsWithSearch(query);
        }

        public SportmasterModel.CachedProduct GetProductById(string id)
        {
            var newProduct = SportmasterDbController.GetCachedProduct(id);
            if (newProduct == null)
            {
                newProduct = SportmasterParseController.GetProductFromId(id);
                SportmasterDbController.AddNewProduct(newProduct);
            }
            return newProduct;
        }
    }
}

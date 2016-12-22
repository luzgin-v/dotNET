using System.Net;
using System.Web.Http;
using webAPI_sportmaster_parser.Models;

namespace webAPI_sportmaster_parser.Controllers
{
    public class SportmasterController : ApiController
    {
        public ProductOfSearch[]  GetProductsByQuery(string query)
        {
            var resultSearch = SportmasterParseController.GetProductsWithSearch(query);
            if (resultSearch == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return resultSearch.ToArray();
        }

        public CachedProduct GetProductById(string id)
        {
            var newProduct = SportmasterParseController.GetProductFromId(id);
            if (newProduct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return newProduct;
        }
    }
}
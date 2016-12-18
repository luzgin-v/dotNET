using System.Collections.Generic;
using System.ServiceModel;

namespace WcfSportmaster
{
    [ServiceContract]
    public interface ISportmasterService
    {
        [OperationContract]
        List<SportmasterModel.ProductOfSearch> GetProductsByQuery(string query);

        [OperationContract]
        SportmasterModel.CachedProduct GetProductById(string id);
    }
}

using System.ServiceModel;

namespace WcfSportmaster
{
    [ServiceContract]
    public interface ISportmasterService
    {
        [OperationContract]
        SportmasterModel.ProductOfSearch [] GetProductsByQuery(string query);

        [OperationContract]
        SportmasterModel.CachedProduct GetProductById(string id);
    }
}

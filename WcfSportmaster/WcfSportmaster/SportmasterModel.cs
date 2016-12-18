using System.Runtime.Serialization;

namespace WcfSportmaster
{
    public partial class SportmasterModel
    {
        [DataContract]
        public class ProductOfSearch
        {
            [DataMember]
            public string SportmasterId { get; set; }
            [DataMember]
            public string Name { get; set; }
        }

        [DataContract]
        public class CachedProduct
        {
            [DataMember]
            public string SportmasterId { get; set; }
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Manufacturer { get; set; }
            [DataMember]
            public string Price { get; set; }
        }
    }
}
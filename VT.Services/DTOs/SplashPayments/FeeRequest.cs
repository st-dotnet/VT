using Newtonsoft.Json;

namespace VT.Services.DTOs.SplashPayments
{
    public class CreateFee
    {
        [JsonProperty(PropertyName = "entity")]
        public string ReferrerId { get; set; }

        [JsonProperty(PropertyName = "forentity")]
        public string ForEntityId { get; set; }

        [JsonProperty(PropertyName = "start")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "um")]
        public string Um { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "schedule")]
        public string Schedule { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string FeeName { get; set; }
    }

    public class CreateFeeRequest
    {
        [JsonProperty(PropertyName = "entity")]
        public string ReferrerId { get; set; }

        [JsonProperty(PropertyName = "forentity")]
        public string ForEntityId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "start")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "um")]
        public string Um { get; set; }

        [JsonProperty(PropertyName = "scheduleFactor")]
        public string ScheduleFactor { get; set; }

        //[JsonProperty(PropertyName = "collectionFactor")]
        //public string CollectionFactor { get; set; }
 
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "schedule")]
        public string Schedule { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string FeeName { get; set; }

        //[JsonProperty(PropertyName = "collectionOffset")]
        //public string CollectionOffset { get; set; }

        [JsonProperty(PropertyName = "inactive")]
        public string Inactive { get; set; }

        [JsonProperty(PropertyName = "frozen")]
        public string Frozen { get; set; }
    } 
}

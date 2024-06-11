using System.Collections.Generic;
using Newtonsoft.Json;
namespace Webhooks.Models.DTO
{
    public class EntitiesModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("operation")]
        public string Operation { get; set; }
        [JsonProperty("lastUpdated")]
        public string LastUpdated { get; set; } //validate 
    }

    public class DataChangeEventsModel
    {
        [JsonProperty("entities")]
        public List<EntitiesModel> Entities { get; set; }
    }
    public class EventNotificationsModel
    {
        [JsonProperty("realmId")]
        public string RealmId { get; set; }

        [JsonProperty("dataChangeEvent")]
        public DataChangeEventsModel DataEvents { get; set; }
    }
    public class WebhooksDataModel
    {
        [JsonProperty("eventNotifications")]
        public List<EventNotificationsModel> EventNotifications { get; set; }
    }
}
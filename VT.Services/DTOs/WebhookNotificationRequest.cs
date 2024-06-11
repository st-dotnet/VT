using Newtonsoft.Json;
using System.Collections.Generic;

namespace VT.Services.DTOs
{
    public class EntitiesRequest
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

    public class DataChangeEventsRequest
    {
        [JsonProperty("entities")]
        public List<EntitiesRequest> Entities { get; set; }
    }
    public class EventNotificationsRequest
    {
        [JsonProperty("realmId")]
        public string RealmId { get; set; }

        [JsonProperty("dataChangeEvent")]
        public DataChangeEventsRequest DataEvents { get; set; }
    }
    public class WebhookNotificationRequest
    {
        [JsonProperty("eventNotifications")]
        public List<EventNotificationsRequest> EventNotifications { get; set; }
    }
}
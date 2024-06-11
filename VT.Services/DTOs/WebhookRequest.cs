using System.Collections.Generic;
using VT.Data.Entities;
using VT.QuickBooks.DTOs.Customers;

namespace VT.Services.DTOs
{
    public class WebhookRequest
    {
        public List<EntitiesRequest> Customers { get; set; }
        public AllCustomerResponse QbCustomers { get; set; }
        public QuickbookSettings QbSettings { get; set; }
        public List<Data.Entities.Customer> DbCustomers { get; set; }
    }
}

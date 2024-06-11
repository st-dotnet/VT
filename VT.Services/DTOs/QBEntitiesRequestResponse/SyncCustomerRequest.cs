using System.Collections.Generic;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class SyncCustomerRequest
    {
        public SyncCustomerRequest()
        {
            UnLinkActualLinkedCustomerIds = new List<int>();
            CustomersEdited = new List<SystemCustomerModel>();
            LinkedSystemCustomers = new List<int>();
            LinkedQBCustomers = new List<int>();
        }
        public List<int> UnLinkActualLinkedCustomerIds { get; set; }
        public List<SystemCustomerModel> CustomersEdited { get; set; }
        public List<int> LinkedSystemCustomers { get; set; }
        public List<int> LinkedQBCustomers { get; set; }
        public int CompanyId { get; set; }
    }
}

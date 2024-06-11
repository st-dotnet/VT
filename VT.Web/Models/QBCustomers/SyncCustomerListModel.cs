using System.Collections.Generic;

namespace VT.Web.Models.QBCustomers
{
    public class SyncCustomerListModel
    {
        public SyncCustomerListModel()
        {
            UnLinkActualLinkedCustomerIds = new List<int>();
            CustomersEdited = new List<Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel>();
            LinkedSystemCustomers = new List<int>();
            LinkedQBCustomers = new List<int>();
        }
        public List<int> UnLinkActualLinkedCustomerIds { get; set; }
        public List<Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel> CustomersEdited { get; set; }
        public List<int> LinkedSystemCustomers { get; set; }
        public List<int> LinkedQBCustomers { get; set; }
        public int CompanyId { get; set; }
    }
    public class UnlinkCustomersModel
    {
        public int? EntityId { get; set; }
        public int? QBEntityId { get; set; }
    }
}
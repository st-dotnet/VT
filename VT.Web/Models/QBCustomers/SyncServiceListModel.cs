using System.Collections.Generic;
using VT.Services.DTOs.QBEntitiesRequestResponse;

namespace VT.Web.Models.QBCustomers
{
    public class SyncServiceListModel
    {
        public SyncServiceListModel()
        {
            UnLinkActualLinkedServicesIds = new List<int>();
            ServicesEdited = new List<SystemServiceModel>();
            LinkedSystemServices = new List<int>();
            LinkedQBServices = new List<int>();
        }

        public List<int> UnLinkActualLinkedServicesIds { get; set; }
        public List<SystemServiceModel> ServicesEdited { get; set; }
        public List<int> LinkedSystemServices { get; set; }
        public List<int> LinkedQBServices { get; set; }
        public int CompanyId { get; set; }
    }
    public class UnlinkServiceModel
    {
        public int? EntityId { get; set; }
        public int? QBEntityId { get; set; }
    }
    public class ServiceIdsModel
    {
        public string ServiceId { get; set; }
        public string QBServiceId { get; set; }
    }
}
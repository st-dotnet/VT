using System.Collections.Generic;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class SyncServicesRequest
    {
        public SyncServicesRequest()
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

    public class SystemServiceModel
    {
        public SystemServiceModel()
        {
            IsLinked = true;
        }
        public int CompanyId { get; set; }
        public string QBServiceId { get; set; }
        public int? ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsMatch { get; set; }
        public bool IsLinked { get; set; }
    }

    public class SystemServiceModel1
    {
        public SystemServiceModel1()
        {
            IsLinked1 = true;
        }
        public int CompanyId1 { get; set; }
        public string QBServiceId1 { get; set; }
        public int? ServiceId1 { get; set; }
        public string Name1 { get; set; }
        public string Description1 { get; set; }

        public bool IsActive1 { get; set; }
        public bool IsMatch1 { get; set; }
        public bool IsLinked1 { get; set; }
    }
    public class QBServiceModel : SystemServiceModel
    {
    }
}

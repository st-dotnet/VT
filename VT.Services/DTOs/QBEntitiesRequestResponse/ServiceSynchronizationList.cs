using System.Collections.Generic;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class UnlinkedService
    {
        public UnlinkedService()
        {
            UnlinkedSystemServices = new List<SystemServiceModel>();
            unlinkedQBServices = new List<QBServiceModel>();
        }
        public List<SystemServiceModel> UnlinkedSystemServices { get; set; }
        public List<QBServiceModel> unlinkedQBServices { get; set; }
    }

    public class LinkedService
    {
        public LinkedService()
        {
            LinkedSystemService = new SystemServiceModel();
            LinkedQBService = new QBServiceModel();
        }
        public SystemServiceModel LinkedSystemService { get; set; }
        public QBServiceModel LinkedQBService { get; set; }
    }
    public class ServiceSynchronizationList : BaseResponse
    {
        public ServiceSynchronizationList()
        {
            LinkedServices = new List<LinkedService>();
            UnlinkedSystemServices = new List<SystemServiceModel>();
            UnlinkedQBServices = new List<QBServiceModel>();
        }

        public List<LinkedService> LinkedServices { get; set; }

        public List<SystemServiceModel> UnlinkedSystemServices { get; set; }

        public List<QBServiceModel> UnlinkedQBServices { get; set; }

    }
}

using System.Collections.Generic;
using VT.Data.Entities;
using VT.Services.DTOs;
using VT.Services.DTOs.QBEntitiesRequestResponse;

namespace VT.Services.Interfaces
{
    public interface ICompanyServiceService
    {
        CompanyService GetCompanyService(int companyServiceId);
        IList<CompanyService> GetAllCompanyServices(int? companyId = null);
        SaveCompanyServiceResponse SaveCompanyService(SaveCompanyServiceRequest request);
        BaseResponse DeleteCompanyServices(List<int> ids);
        IList<CompanyService> GetFilteredCompanyServices(int companyId, int customerId);
        BaseResponse UndeleteCompanyService(int companyServiceId);
        BaseResponse ActivateService(int id);
        BaseResponse DeactivateService(int id);

        // FOR QUICK BOOKS
        ServiceSynchronizationList ServiceSynchronizationList(int? companyId);
        BaseResponse UpdateSynList(SyncServicesRequest request);
    }
}

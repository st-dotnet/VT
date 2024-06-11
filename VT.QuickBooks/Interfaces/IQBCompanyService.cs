using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.CompanyServices;

namespace VT.QuickBooks.Interfaces
{
    public interface IQBCompanyService
    {
        QuickbookBaseResponse CreateQBCompanyService(string jsonService, string authorizationTokenHeader);
        QuickbookBaseResponse DeleteQBCompanyService(string serviceId);
        GetQBServiceResponse GetQBService(string serviceId, string authorizationTokenHeader);
        GetAllServices GetAllServices(string authorizationTokenHeader);
        QuickbookBaseResponse UpdateService(string jsonService, string authorizationTokenHeader);
    }
}

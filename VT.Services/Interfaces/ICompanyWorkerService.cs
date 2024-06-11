using System.Collections.Generic;
using VT.Data.Entities;
using VT.QuickBooks.DTOs.Employee;
using VT.Services.DTOs;

namespace VT.Services.Interfaces
{
    public interface ICompanyWorkerService
    {
        LoginResponse AuthenticateUser(LoginRequest request);
        IList<CompanyWorker> GetAllUsers(int? companyId);
        IEnumerable<QuickBooks.DTOs.Employee.SyncEmployeeRequest> GetAllEmployees(int? companyId);
        BaseResponse DeleteUsers(List<int> ids);
        CompanyWorkerSaveResponse AddUser(CompanyWorkerSaveRequest request);
        CompanyWorkerResponse GetCompanyWorker(int companyWorkerId);
        BaseResponse IsEmailAlreadyExist(string email);
        BaseResponse ResetUserPassword(ResetPasswordRequest request);
        List<string> GetAllActiveUsersEmail();
        BaseResponse ActivateWorker(int id);
        BaseResponse DeactivateWorker(int id);

        // synchronization methods
        EmployeeSynchronizationList EmployeeSynchronizationList(int? companyId);
        BaseResponse UpdateSyncList(DTOs.QBEntitiesRequestResponse.SyncEmployeeRequest request);
    }
}

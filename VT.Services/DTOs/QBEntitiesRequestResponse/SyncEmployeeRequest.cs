using System.Collections.Generic;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class SyncEmployeeRequest
    {
        public SyncEmployeeRequest()
        {
            UnLinkActualLinkedEmployeeIds = new List<int>();
            EmployeesEdited = new List<SystemEmployeeModel>();
            LinkedSystemEmployees = new List<int>();
            LinkedQBEmployees = new List<int>();
        }
        public List<int> UnLinkActualLinkedEmployeeIds { get; set; }
        public List<SystemEmployeeModel> EmployeesEdited { get; set; }
        public List<int> LinkedSystemEmployees { get; set; }
        public List<int> LinkedQBEmployees { get; set; }
        public int CompanyId { get; set; }
    }
}

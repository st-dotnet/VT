using System.Collections.Generic;
using VT.Services.DTOs;

namespace VT.Web.Models.QBCustomers
{
    public class SyncEmployeeListModel
    {
        public SyncEmployeeListModel()
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
    public class UnlinkEmployeeModel
    {
        public int? CustomerId { get; set; }
        public int? QBCustomerId { get; set; }
    }
}
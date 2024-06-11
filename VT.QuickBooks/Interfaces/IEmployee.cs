using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Employee;
using System.Collections.Generic;

namespace VT.QuickBooks.Interfaces
{
    public interface IEmployee
    {
        QuickbookBaseResponse CreateEmployee(string jsonEmployee, string authorizationTokenHeader);
        QuickbookBaseResponse UpdateEmployee(string jsonEmployee, string authorizationTokenHeader);
        QuickbookBaseResponse DeleteEmployee(int employeeId);
        GetAllEmployees GetAllEmployees(string authorizationTokenHeader);
        GetEmployeeResponse GetEmployee(string employeeId, string authorizationTokenHeader);
    }
}

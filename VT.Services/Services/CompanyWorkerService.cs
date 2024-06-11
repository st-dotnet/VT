using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using VT.Common.Utils;
using VT.Data.Context;
using VT.Data.Entities;
using VT.QuickBooks.DTOs.Employee;
using VT.QuickBooks.Interfaces;
using VT.Services.DTOs;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.Services.Interfaces;

namespace VT.Services.Services
{
    public class CompanyWorkerService : ICompanyWorkerService
    {
        #region Field(s)

        private readonly IVerifyTechContext _context;
        private readonly IEmployee _qbEmployeeService;

        #endregion

        #region Constructor

        public CompanyWorkerService(IVerifyTechContext context, IEmployee qbEmployeeService)
        {
            _context = context;
            _qbEmployeeService = qbEmployeeService;
        }

        #endregion

        #region Interface implementation

        //Login
        public LoginResponse AuthenticateUser(LoginRequest request)
        {
            var response = new LoginResponse();

            var companyWorker = _context.CompanyWorkers
                .Include(x => x.Company)
                .FirstOrDefault(x => x.Email == request.Email);

            if (companyWorker == null)
            {
                response.Success = false;
                response.Message = "The username you provided does not exists.";
                return response;
            }
            if (companyWorker.IsDeleted)
            {
                response.Success = false;
                response.Message = "The username you provided is inactive.";
                return response;
            }
            else
            {
                response.Success = true;
                response.CompanyWorker = companyWorker;
            }
            return response;
        }

        // deactivate employee
        public BaseResponse DeactivateWorker(int id)
        {
            var response = new BaseResponse();
            try
            {
                var employee = _context.CompanyWorkers.FirstOrDefault(x => x.CompanyWorkerId == id);
                if (employee == null)
                {
                    response.Success = false;
                    response.Message = "Employee doesn't exists.";
                }
                // deactivate employee
                employee.IsDeleted = true;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Employee has been deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        // activate employee
        public BaseResponse ActivateWorker(int id)
        {
            var response = new BaseResponse();
            try
            {
                var employee = _context.CompanyWorkers.FirstOrDefault(x => x.CompanyWorkerId == id);
                if (employee == null)
                {
                    response.Success = false;
                    response.Message = "Employee doesn't exists.";
                    return response;
                }
                // activate employee
                employee.IsDeleted = false;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Employee has been activated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public IList<CompanyWorker> GetAllUsers(int? companyId)
        {
            var query = _context.CompanyWorkers;
            return (companyId == null) ? query.ToList() : query.Where(x => x.CompanyId == companyId).ToList();
        }

        // combo method of employee fetching
        public IEnumerable<QuickBooks.DTOs.Employee.SyncEmployeeRequest> GetAllEmployees(int? companyId)
        {
            var list = new List<QuickBooks.DTOs.Employee.SyncEmployeeRequest>();

            var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == companyId.Value);

            // fetch quickbooks employees
            var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
            var qbEmployeesList = _qbEmployeeService.GetAllEmployees(authorizationToken);

            // fetch employees from database
            var dbEmployees = _context.CompanyWorkers.Where(x => x.CompanyId == companyId.Value).ToList();

            foreach (var employee in qbEmployeesList.QueryResponse.Employees.ToList())
            {
                var empExist = dbEmployees.FirstOrDefault(x => x.QBEmployeeId == employee.Id);

                if (empExist == null)
                {
                    // new record from quickbooks
                    var newEmp = new CompanyWorker();
                    newEmp.CompanyId = companyId.Value;
                    newEmp.Email = employee.PrimaryEmailAddr != null ? employee.PrimaryEmailAddr.Address : "";
                    newEmp.FirstName = employee.GivenName;
                    newEmp.LastName = employee.FamilyName;
                    newEmp.PasswordSalt = PasswordUtil.GenerateSalt();
                    newEmp.HashedPassword = PasswordUtil.CreatedHashedPassword(qbSettings.DefaultPassword, newEmp.PasswordSalt);
                    newEmp.QBEmployeeId = employee.Id;

                    _context.CompanyWorkers.Add(newEmp);
                    _context.SaveChanges();

                    // add obj in list
                    //list.Add(GetEmployeeObject(newEmp, false));
                }
                else
                {
                    // current employee from quickbooks already exists in database

                    bool isMatch = empExist != null && employee.PrimaryEmailAddr != null
                       &&
                       (employee.PrimaryEmailAddr.Address == empExist.Email)
                       &&
                       (employee.GivenName == empExist.FirstName)
                       &&
                       (employee.FamilyName == empExist.LastName);

                    if (!isMatch)
                    {
                        // update fields
                        empExist.Email = employee.PrimaryEmailAddr != null ? employee.PrimaryEmailAddr.Address : "";
                        empExist.FirstName = employee.GivenName;
                        empExist.LastName = employee.FamilyName;

                        _context.SaveChanges();
                    }
                    //list.Add(GetEmployeeObject(empExist, isMatch));
                }
            }

            // database employees
            foreach (var dbEmployee in dbEmployees)
            {
                var qbEmp = qbEmployeesList.QueryResponse.Employees
                    .FirstOrDefault(x => x.Id == dbEmployee.QBEmployeeId);

                if (qbEmp == null)
                {
                    // create new employee on quickbooks from database

                    var request = new CompanyWorkerSaveRequest();
                    request.CompanyId = companyId;
                    request.Email = dbEmployee.Email;
                    request.FirstName = dbEmployee.FirstName;
                    request.LastName = dbEmployee.LastName;

                    var response = SaveEmployeeOnQB(request);
                    if (response.Success) dbEmployee.QBEmployeeId = response.QBEntityId;
                    _context.SaveChanges();
                }
                else
                {
                    // employee existing on quick books but should be updated on there
                    // make employee update request

                    bool isMatch = qbEmp != null && qbEmp.PrimaryEmailAddr != null
                        &&
                        (qbEmp.PrimaryEmailAddr.Address == dbEmployee.Email)
                        &&
                        (qbEmp.GivenName == dbEmployee.FirstName)
                        &&
                        (qbEmp.FamilyName == dbEmployee.LastName);

                    if (!isMatch)
                    {
                        if (qbSettings.IsQuickbooksIntegrated && qbSettings.EmployeeSettings)
                        {
                            // prepare update employee json object
                            var employee = new DTOs.CreateEmployeeRequest
                            {
                                Id = dbEmployee.QBEmployeeId.ToString(),
                                GivenName = dbEmployee.FirstName,
                                FamilyName = dbEmployee.LastName,
                                DisplayName = dbEmployee.FirstName + " " + dbEmployee.LastName,
                                PrimaryEmailAddr = new DTOs.PrimaryEmailAddr
                                {
                                    Address = dbEmployee.Email
                                },
                            };
                            var json = JsonConvert.SerializeObject(employee);

                            // generate encoded authorization token header
                            var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                            var employeeIpdationResponse = _qbEmployeeService.UpdateEmployee(json, authorizationTokenHeader);
                            if (!employeeIpdationResponse.Success)
                            {
                            }
                            var qbEmployee = XmlUtil.Deserialize<EmployeeCreationResponse>(employeeIpdationResponse.ResponseValue);
                            var X = employeeIpdationResponse.ResponseValue;
                        }
                    }
                }
            }
            return list;
        }

        #region Employee Synchronization Methods

        public EmployeeSynchronizationList EmployeeSynchronizationList(int? companyId)
        {
            var syncList = new EmployeeSynchronizationList();
            var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == companyId.Value);
            try
            {
                // fetch records form database
                var dbEmployees = _context.CompanyWorkers.Where(x => x.CompanyId == companyId.Value).ToList();

                // generate aujthorization token header
                var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();

                // prepare request to fetch all employees
                var qbEmployees = _qbEmployeeService.GetAllEmployees(authorizationTokenHeader);

                if (!qbEmployees.Success)
                {
                    syncList.Success = false;
                    syncList.Message = qbEmployees.Message;
                    return syncList;
                }
                foreach (var qbEmployee in qbEmployees.QueryResponse.Employees.ToList())
                {
                    var linkedEmployees = new LinkedEmployee();
                    var unlinkedEmployee = new UnlinkedEmployee();

                    var employeeExists = dbEmployees.FirstOrDefault(x => x.QBEmployeeId == qbEmployee.Id);

                    if (employeeExists != null)
                    {
                        var systemEmployee = GetSystemEmployee(employeeExists, companyId.Value);
                        var qbEmployeeObj = GetQBEmployee(qbEmployee, companyId, employeeExists.CompanyWorkerId);

                        bool isMatch = IsEmployeeMatched(systemEmployee, qbEmployeeObj);
                        systemEmployee.IsMatch = isMatch;
                        qbEmployeeObj.IsMatch = isMatch;

                        linkedEmployees.LinkedSystemEmployee = systemEmployee;
                        linkedEmployees.LinkedQBEmployee = qbEmployeeObj;
                        syncList.LinkedEmployees.Add(linkedEmployees);
                    }
                    else
                    {
                        var unlinkedQBEmployee = GetQBEmployee(qbEmployee, companyId.Value, null);
                        syncList.UnlinkedQBEmployees.Add(unlinkedQBEmployee);
                    }
                }

                // make list of unliniked employees list
                foreach (var dbEmployee in dbEmployees)
                {
                    // qbEmployeeId=null mean employee is unlinked
                    if (dbEmployee.QBEmployeeId == null || dbEmployee.QBEmployeeId == "null")
                    {
                        var systemEmployee = GetSystemEmployee(dbEmployee, companyId);
                        syncList.UnlinkedSystemEmployees.Add(systemEmployee);
                    }
                }
                syncList.Success = true;
            }
            catch (Exception ex)
            {
                syncList.Success = false;
                syncList.Message = ex.Message.ToString();
            }
            return syncList;
        }

        public BaseResponse UpdateSyncList(DTOs.QBEntitiesRequestResponse.SyncEmployeeRequest request)
        {
            var response = new BaseResponse();
            var updateResponse = new BaseResponse();
            StringBuilder sb = new StringBuilder();
            try
            {
                var dbEmployees = _context.CompanyWorkers.ToList();
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                #region Unlink Actual Linked Employees

                if (request.UnLinkActualLinkedEmployeeIds.Count != 0)
                {
                    response = UnLinkActualLinkedEmployee(dbEmployees, request.UnLinkActualLinkedEmployeeIds);
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                }

                #endregion

                #region Linked System Employees

                //linked system employees (employees to be saved on quick books server)
                if (request.LinkedSystemEmployees.Count != 0)
                {
                    // make request     
                    response = LinkedSystemEmployees(dbEmployees, request.LinkedSystemEmployees, request.EmployeesEdited);
                    if (!response.Success)
                    {
                        sb.Append("Couldn't save this employee on Quikcbooks " + response.Message + " ");
                    }
                }

                #endregion

                #region Linked Quick books Employees

                // linked qb customers
                if (request.LinkedQBEmployees.Count != 0)
                {
                    response = LinkedQBEmployees(request.LinkedQBEmployees, qbSettings, request);
                    dbEmployees = _context.CompanyWorkers.ToList();
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                    updateResponse = UpdateEmployeeForQuickbooks(request.EmployeesEdited, qbSettings, request, dbEmployees);
                    if (!updateResponse.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                }

                #endregion

                #region Update Employee on Quikcbooks              

                if (request.EmployeesEdited.Count != 0 && request.LinkedQBEmployees.Count == 0)
                {
                    updateResponse = UpdateEmployeeOnQB(request.EmployeesEdited, qbSettings, request, dbEmployees);
                    var dbEmp = _context.CompanyWorkers.ToList();
                    if (updateResponse.Success)
                        // update employee on local database
                        foreach (var employee in request.EmployeesEdited)
                        {
                            var dbEmployee = dbEmp.FirstOrDefault(x => x.CompanyWorkerId == employee.EmployeeId);

                            dbEmployee.Email = employee.Email;
                            dbEmployee.FirstName = employee.GivenName;
                            dbEmployee.LastName = employee.FamilyName;
                            _context.SaveChanges();
                        }
                }

                #endregion

                response.Success = true;
                response.Message = "Employees have been synchronized successfully." + sb.ToString();
                if (!qbSettings.EmployeeSettings)
                {
                    qbSettings.EmployeeSettings = true;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        #endregion

        public BaseResponse DeleteUsers(List<int> ids)
        {
            var response = new BaseResponse();
            try
            {
                var companyWorkers = _context.CompanyWorkers.Where(x => ids.Contains(x.CompanyWorkerId)).ToList();

                foreach (var companyWorker in companyWorkers)
                {
                    companyWorker.IsDeleted = true;
                    //companyWorker.Email = EmailDeleted(companyWorker.Email);
                }
                _context.SaveChanges();
                response.Success = true;
            }
            catch (Exception exception)
            {
                response.Message = exception.ToString();
            }
            return response;
        }

        public CompanyWorkerResponse GetCompanyWorker(int companyWorkerId)
        {
            var user = _context.CompanyWorkers
                    .FirstOrDefault(x => x.CompanyWorkerId == companyWorkerId);

            var customers =
                _context.CompanyWorkerCustomers.Where(x => x.CompanyWorkerId == companyWorkerId)
                    .OrderBy(x => x.CustomerOrder)
                    .Select(x => x.CustomerId)
                    .ToList();

            if (user == null)
            {
                throw new Exception("User does not exist in the database");
            }

            return new CompanyWorkerResponse
            {
                CompanyId = user.CompanyId,
                CompanyWorkerId = user.CompanyWorkerId,
                CompanyName = user.Company != null ? user.Company.Name : "N/A",
                Username = user.Email,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                IsAdmin = user.IsAdmin,
                LastName = user.LastName,
                IsDeleted = user.IsDeleted,
                AuthKey = user.HashedPassword,
                Role = user.IsAdmin && user.Company == null ? "Super Admin" : (user.IsAdmin && user.Company != null ? "Company Admin" : "Company Worker"),
                To = customers,
                IsCompanyActive = user.Company == null || (user.Company != null && !user.Company.IsDeleted)
            };
        }

        public BaseResponse IsEmailAlreadyExist(string email)
        {
            var response = new BaseResponse();
            var user = _context.CompanyWorkers.FirstOrDefault(x => x.Email == email);
            response.Success = user != null;
            return response;
        }

        public BaseResponse ResetUserPassword(ResetPasswordRequest request)
        {
            var response = new BaseResponse();
            var companyWorker = _context.CompanyWorkers.FirstOrDefault(x => x.CompanyWorkerId == request.UserId);

            if (companyWorker == null)
            {
                response.Message = "User record does not exist in the database.";
                return response;
            }

            companyWorker.PasswordSalt = PasswordUtil.GenerateSalt();
            companyWorker.HashedPassword = PasswordUtil.CreatedHashedPassword(request.NewPassword, companyWorker.PasswordSalt);
            _context.SaveChanges();

            response.Success = true;
            return response;
        }

        public List<string> GetAllActiveUsersEmail()
        {
            return _context.CompanyWorkers.Where(x => !x.IsDeleted).Select(x => x.Email).ToList();
        }

        public CompanyWorkerSaveResponse AddUser(CompanyWorkerSaveRequest request)
        {
            var resposne = new CompanyWorkerSaveResponse();
            var baseResponse = new BaseResponse();
            try
            {
                var companyWorker = request.CompanyWorkerId > 0
                ? _context.CompanyWorkers.Include(x => x.AccessibleCustomers).FirstOrDefault(x => x.CompanyWorkerId == request.CompanyWorkerId)
                : new CompanyWorker();

                if (companyWorker == null)
                {
                    resposne.Message = "User record does not exist in the database.";
                    return resposne;
                }

                companyWorker.Email = request.Email;
                companyWorker.CompanyId = request.CompanyId;
                companyWorker.FirstName = request.FirstName;
                companyWorker.MiddleName = request.MiddleName;
                companyWorker.LastName = request.LastName;
                companyWorker.IsAdmin = request.IsAdmin;

                if (companyWorker.CompanyWorkerId == 0)
                {
                    companyWorker.PasswordSalt = PasswordUtil.GenerateSalt();
                    companyWorker.HashedPassword = PasswordUtil.CreatedHashedPassword(request.Password,
                        companyWorker.PasswordSalt);

                    _context.CompanyWorkers.Add(companyWorker);
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.Password))
                    {
                        var hashedPassword = PasswordUtil.CreatedHashedPassword(request.Password,
                          companyWorker.PasswordSalt);

                        // ReSharper disable once RedundantCheckBeforeAssignment
                        if (companyWorker.HashedPassword != hashedPassword) //update the password
                        {
                            companyWorker.HashedPassword = hashedPassword;
                        }

                    }
                    if (companyWorker.AccessibleCustomers != null)
                    {
                        // remove existing access if any
                        foreach (var access in companyWorker.AccessibleCustomers.ToList())
                        {
                            _context.CompanyWorkerCustomers.Remove(access);
                        }
                    }
                    var count = 0;
                    // add new customer access
                    if (request.AccessibleCustomers != null)
                    {
                        foreach (var customerId in request.AccessibleCustomers)
                        {
                            _context.CompanyWorkerCustomers.Add(new CompanyWorkerCustomer
                            {
                                CompanyWorkerId = companyWorker.CompanyWorkerId,
                                CustomerId = customerId,
                                CustomerOrder = count++
                            });
                        }
                    }
                    resposne.CompanyWorker = companyWorker;
                }
                // save employee on quickbooks
                if (string.IsNullOrEmpty(companyWorker.QBEmployeeId))
                {
                    baseResponse = SaveEmployeeOnQB(request);
                    if (baseResponse.Success) companyWorker.QBEmployeeId = baseResponse.QBEntityId;
                }
                _context.SaveChanges();
                resposne.Success = true;
            }
            catch (Exception exception)
            {
                resposne.Message = exception.ToString();
            }
            return resposne;
        }

        #endregion

        #region Private Method(s)

        private BaseResponse SaveEmployeeOnQB(CompanyWorkerSaveRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var qbSettings = _context.QuickbookSettings
                    .FirstOrDefault(x => x.CompanyId == request.CompanyId);
                if (qbSettings == null) return response;
                if (qbSettings.IsQuickbooksIntegrated && qbSettings.EmployeeSettings)
                {
                    // prepare create employee json object
                    var employee = new DTOs.CreateEmployeeRequest
                    {
                        GivenName = request.FirstName,
                        FamilyName = request.LastName,
                        DisplayName = request.FirstName + " " + request.LastName,
                        PrimaryEmailAddr = new DTOs.PrimaryEmailAddr
                        {
                            Address = request.Email
                        }
                    };
                    var json = JsonConvert.SerializeObject(employee);

                    // generate encoded authorization token header
                    var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                    var employeeCreationResponse = _qbEmployeeService.CreateEmployee(json, authorizationTokenHeader);
                    if (!employeeCreationResponse.Success)
                    {
                        response.Success = false;
                        response.Message = employeeCreationResponse.Message;
                        return response;
                    }
                    var qbEmployee = XmlUtil.Deserialize<EmployeeCreationResponse>(employeeCreationResponse.ResponseValue);
                    response.Success = employeeCreationResponse.Success;
                    response.Message = employeeCreationResponse.Message;
                    response.QBEntityId = qbEmployee.Employee.Id;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could't process the request, Please make sure that employee settings under Quikcbooks Settings Tab in ON.";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse LinkedQBEmployees(List<int> linkedQBEmployees, QuickbookSettings qbSettings, DTOs.QBEntitiesRequestResponse.SyncEmployeeRequest req)
        {
            var response = new BaseResponse();
            try
            {
                var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                var qbEmployees = _qbEmployeeService.GetAllEmployees(authorizationTokenHeader);

                foreach (var qbEmployeeId in linkedQBEmployees)
                {
                    var qbEmployee = qbEmployees.QueryResponse.Employees.FirstOrDefault(x => x.Id == qbEmployeeId.ToString());
                    var empExists = req.EmployeesEdited.FirstOrDefault(x => x.QBEmployeeId == qbEmployeeId.ToString());

                    if (qbEmployee != null)
                    {
                        var employee = new CompanyWorker();
                        var name = qbEmployee.FamilyName != null ? qbEmployee.GivenName + " " + qbEmployee.FamilyName : qbEmployee.GivenName + "";

                        employee = new CompanyWorker();
                        employee.FirstName = empExists != null ? empExists.GivenName : name;
                        employee.CompanyId = qbSettings.CompanyId;
                        employee.LastName = empExists != null ? empExists.FamilyName : qbEmployee.FamilyName;
                        employee.Email = empExists != null ? empExists.Email : qbEmployee.PrimaryEmailAddr != null ? qbEmployee.PrimaryEmailAddr.Address : "";
                        employee.PasswordSalt = PasswordUtil.GenerateSalt();
                        employee.HashedPassword = PasswordUtil.CreatedHashedPassword(qbSettings.DefaultPassword, employee.PasswordSalt);
                        employee.QBEmployeeId = qbEmployee.Id;

                        _context.CompanyWorkers.Add(employee);
                        _context.SaveChanges();
                    }
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse UpdateEmployeeForQuickbooks(List<SystemEmployeeModel> request, QuickbookSettings qbSettings, DTOs.QBEntitiesRequestResponse.SyncEmployeeRequest request1, List<CompanyWorker> dbemployees)
        {
            var response = new BaseResponse();
            var response1 = new GetEmployeeResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestEmployee in request)
                {
                    var dbEmployee = dbemployees.FirstOrDefault(x => x.QBEmployeeId == requestEmployee.QBEmployeeId);
                    var employee = new UpdateEmployeeRequest();
                    // generate authorization header toekn
                    var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();

                    //get employee from quickbooks
                    var qbEmployee = _qbEmployeeService.GetEmployee(requestEmployee.QBEmployeeId != null ? requestEmployee.QBEmployeeId : requestEmployee.EmployeeId.ToString(), authorizationToken);
                    employee.sparse = true;
                    employee.Id = requestEmployee.QBEmployeeId;
                    employee.SyncToken = qbEmployee.Employee.SyncToken;
                    employee.GivenName = requestEmployee.GivenName;
                    employee.FamilyName = requestEmployee.FamilyName;
                    employee.DisplayName = employee.GivenName + " " + employee.FamilyName;
                    employee.PrimaryEmailAddr = new QuickBooks.DTOs.Employee.PrimaryEmailAddr
                    {
                        Address = requestEmployee.Email
                    };
                    var jsonEmployee = JsonConvert.SerializeObject(employee);
                    var employeeUpdationResponse = _qbEmployeeService.UpdateEmployee(jsonEmployee, authorizationToken);

                    if (employeeUpdationResponse.Success)
                        response1 = XmlUtil.Deserialize<GetEmployeeResponse>(employeeUpdationResponse.ResponseValue);
                    dbEmployee.QBEmployeeId = response1.Employee.Id;
                    _context.SaveChanges();

                    if (!employeeUpdationResponse.Success)
                    {
                        response.Success = false;
                        response.Message = employeeUpdationResponse.Message;
                        return response;
                    }
                    response.Success = true;
                }
                //}
                //else
                //{
                //    response.Success = false;
                //    response.Message = "Please make sure that customer settings under Settings TAB is ON.";
                //}
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse UpdateEmployeeOnQB(List<SystemEmployeeModel> request, QuickbookSettings qbSettings, DTOs.QBEntitiesRequestResponse.SyncEmployeeRequest request1, List<CompanyWorker> dbemployees)
        {
            var response = new BaseResponse();
            var response1 = new GetEmployeeResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestEmployee in request)
                {
                    var dbEmployee = dbemployees.FirstOrDefault(x => x.CompanyWorkerId == requestEmployee.EmployeeId);
                    if (request1.LinkedSystemEmployees != null || request1.LinkedSystemEmployees.Count != 0)
                        if (!request1.LinkedSystemEmployees.Contains(requestEmployee.EmployeeId.Value))
                        {
                            var employee = new UpdateEmployeeRequest();

                            // generate authorization header toekn
                            var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();

                            //get customer from quickbooks
                            var qbEmployee = _qbEmployeeService.GetEmployee(requestEmployee.QBEmployeeId != null ? requestEmployee.QBEmployeeId : requestEmployee.EmployeeId.ToString(), authorizationToken);

                            employee.sparse = true;
                            employee.Id = requestEmployee.QBEmployeeId;
                            employee.SyncToken = qbEmployee.Employee.SyncToken;
                            employee.GivenName = requestEmployee.GivenName;
                            employee.FamilyName = requestEmployee.FamilyName;
                            employee.DisplayName = requestEmployee.GivenName + " " + requestEmployee.FamilyName;
                            employee.PrimaryEmailAddr = new QuickBooks.DTOs.Employee.PrimaryEmailAddr
                            {
                                Address = requestEmployee.Email
                            };
                            var jsonEmployee = JsonConvert.SerializeObject(employee);
                            var employeeUpdationResponse = _qbEmployeeService.UpdateEmployee(jsonEmployee, authorizationToken);

                            if (employeeUpdationResponse.Success)
                                response1 = XmlUtil.Deserialize<GetEmployeeResponse>(employeeUpdationResponse.ResponseValue);
                            dbEmployee.QBEmployeeId = response1.Employee.Id;
                            _context.SaveChanges();

                            if (!employeeUpdationResponse.Success)
                            {
                                response.Success = false;
                                response.Message = employeeUpdationResponse.Message;
                                return response;
                            }
                        }
                    response.Success = true;
                }
                //}
                //else
                //{
                //    response.Success = false;
                //    response.Message = "Please make sure that customer settings under Settings TAB is ON.";
                //}
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private string EmailDeleted(string email)
        {
            var arr = email.Split('@');
            return arr.Length == 2 ? string.Format("{0}_deleted_{1}@{2}", arr[0], DateTime.UtcNow.ToString("MMddyyyyHHmmss"), arr[1]) : string.Empty;
        }

        private BaseResponse LinkedSystemEmployees(List<CompanyWorker> dbEmployees, List<int> employeesToBeLinkedIds, List<SystemEmployeeModel> employeeEdited)
        {
            var response = new BaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                // save customers on quickbooks
                foreach (var employeeId in employeesToBeLinkedIds)
                {
                    var dbEmployee = dbEmployees.FirstOrDefault(x => x.CompanyWorkerId == employeeId);

                    var checkEmployee = employeeEdited.FirstOrDefault(x => x.EmployeeId == employeeId);

                    if (dbEmployee != null)
                    {
                        var index = checkEmployee != null ? checkEmployee.DisplayName.IndexOf(' ') : 0;
                        var first = checkEmployee != null ? checkEmployee.GivenName.Substring(0, index) : dbEmployee.FirstName;
                        var last = checkEmployee != null ? checkEmployee.FamilyName.Substring(index + 1) : dbEmployee.LastName;

                        var request = new DTOs.CompanyWorkerSaveRequest
                        {
                            FirstName = checkEmployee != null ? checkEmployee.GivenName : first,
                            LastName = checkEmployee != null ? checkEmployee.FamilyName : last,
                            Email = checkEmployee != null ? checkEmployee.Email : dbEmployee.Email,
                            CompanyId = dbEmployee.CompanyId
                        };
                        response = SaveEmployeeOnQB(request);
                        if (response.Success)
                        {
                            dbEmployee.QBEmployeeId = response.QBEntityId;
                            _context.SaveChanges();
                        }
                        else
                        {
                            sb.Append("<b>" + request.FirstName + " " + request.LastName + "</b> " + response.Message + "<br/> ");
                        }
                    }
                }
                response.Success = response.Success;
                response.Message = sb.ToString();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse UnLinkActualLinkedEmployee(List<CompanyWorker> dbEmployees, List<int> unlinkedIds)
        {
            var response = new BaseResponse();
            try
            {
                foreach (var employeeId in unlinkedIds)
                {
                    var employee = dbEmployees.FirstOrDefault(x => x.CompanyWorkerId == employeeId);
                    if (employee != null)
                    {
                        employee.QBEmployeeId = null;
                        _context.SaveChanges();
                    }
                }
                response.Success = true;
                response.Message = "Employees hasve been unlinked successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private SystemEmployeeModel GetSystemEmployee(CompanyWorker dbEmployee, int? companyId)
        {
            //linked  system employees list
            return new SystemEmployeeModel
            {
                GivenName = dbEmployee.FirstName,
                FamilyName = dbEmployee.LastName,
                EmployeeId = dbEmployee.CompanyWorkerId,
                Email = dbEmployee.Email,
                QBEmployeeId = dbEmployee.QBEmployeeId,
                CompanyId = companyId.Value
            };
        }

        private QBEmployeeModel GetQBEmployee(QuickBooks.DTOs.Employee.Employee qbEmployee, int? companyId, int? employeeId)
        {
            return new QBEmployeeModel
            {
                GivenName = qbEmployee.GivenName,
                FamilyName = qbEmployee.FamilyName,
                QBEmployeeId = qbEmployee.Id,
                CompanyId = companyId.Value,
                Email = qbEmployee.PrimaryEmailAddr != null ? qbEmployee.PrimaryEmailAddr.Address : ""
            };
        }

        private bool IsEmployeeMatched(SystemEmployeeModel systemEmployee, QBEmployeeModel qbEmployee)
        {
            bool isMatch =
                systemEmployee.GivenName == qbEmployee.GivenName &&
                systemEmployee.FamilyName == qbEmployee.FamilyName &&
                 systemEmployee.Email == qbEmployee.Email;
            return isMatch;
        }

        #endregion
    }
}

using System.Web.Mvc;
using AutoMapper;
using VT.Web.Models;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.Interfaces;
using System;
using System.Text;
using System.Collections.Generic;
using VT.Web.Models.QBCustomers;
using VT.Services.Interfaces;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using System.Linq;
using VT.Services.DTOs;

namespace VT.Web.Controllers
{
    public class QuickbooksController : BaseController
    {
        #region Fields

        private readonly IQuickbookSettings _qbSettings;
        private readonly ICompanyWorkerService _companyWorkerServices;
        private readonly ICustomerService _customerServices;
        private readonly ICompanyServiceService _companyServiceService;

        #endregion

        #region Constructor

        public QuickbooksController(ICompanyServiceService companyServiceService, IQuickbookSettings qbSettings, ICompanyWorkerService companyWorkerService, ICustomerService customerServices)
        {
            _companyServiceService = companyServiceService;
            _qbSettings = qbSettings;
            _companyWorkerServices = companyWorkerService;
            _customerServices = customerServices;
        }

        #endregion

        #region Public Method(s)  

        //For Company Admin
        [Authorize(Roles = "CompanyAdmin")]
        [HttpGet, Route("~/Quickbooks")]
        public ActionResult Quickbooks()
        {
            ViewData["States"] = GetStates();
            var model = GetQuickbooksSettings(CurrentIdentity.CompanyId.Value);
            return View(model);
        }

        [Authorize(Roles = "CompanyAdmin")]
        // save quickbook settings
        [HttpPost, Route("~/Quickbooks/SaveSettings")]
        public ActionResult SaveSettings(QuickbooksSettingsModel model)
        {
            var message = string.Empty;
            StringBuilder sb = new StringBuilder();

            if (model.ClientId == null || model.ClientSecret == null ||
                model.InvoicePrefix == null || model.DefaultPassword == null || model.RealmId == null)
            {
                if (model.ClientId == null)
                {
                    sb.Append("Client ID is required.");
                }
                if (model.ClientSecret == null)
                {
                    sb.Append(model.ClientId != null ? "Client Secret is required." : "<br/>Client Secret is required.");
                }
                if (model.DefaultPassword == null)
                {
                    sb.Append(model.DefaultPassword != null && model.ClientSecret != null && model.ClientId != null ? "Default password is required." : "<br/>Default password is required.");
                }
                if (model.RealmId == null)
                {
                    sb.Append(model.RealmId != null && model.DefaultPassword != null && model.ClientId != null && model.ClientSecret != null ? "Realm ID is required." : "<br/>Realm ID is required.");
                }
                return Json(new
                {
                    success = false,
                    message = sb.ToString()
                });
            }

            // generate encoded authorization token header
            model.AuthorizationToken = Base64Encode(model.ClientId + ":" + model.ClientSecret);
            model.CompanyId = CurrentIdentity.CompanyId.Value;

            var data = Mapper.Map<QuickbooksSettingsRequest>(model);
            var response = _qbSettings.SyncQuickbookSettings(data);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        #region Customer Synchronization

        // get sync list data
        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetModal")]
        public ActionResult GetModal()
        {
            ViewData["States"] = GetStates();
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedCustomers", SyncListDataFromSession);
        }

        // get sync list data
        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetSynchronizationListData")]
        public ActionResult GetSynchronizationListData()
        {
            ViewData["States"] = GetStates();
            SetCustomerSession();
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedCustomersView", SyncListDataFromSession);
        }

        [HttpPost, Route("~/Quickbooks/EditLinkedCustomer")]
        public ActionResult EditLinkedCustomer(IdsModel model)
        {
            var data = GetSessionData();

            var customerObj = new LinkedCustomer();

            customerObj = data.LinkedCustomers.FirstOrDefault(x => x.LinkedSystemCustomer.SCCustomerId == int.Parse(model.CustomerId));
            if (customerObj == null)
            {
                // quickbooks cutomer which is just moved to linked customers TAB is being edited
                customerObj = data.LinkedCustomers.FirstOrDefault(x => x.LinkedQBCustomer.QbCustomerId == model.QBCustomerId);
            }
            var model1 = new Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel
            {
                SCAddress = customerObj.LinkedSystemCustomer.SCAddress,
                SCity = customerObj.LinkedSystemCustomer.SCity,
                CompanyId = customerObj.LinkedSystemCustomer.CompanyId,
                SCountry = customerObj.LinkedSystemCustomer.SCountry,
                SCCustomerId = customerObj.LinkedSystemCustomer.SCCustomerId,
                SCEmail = customerObj.LinkedSystemCustomer.SCEmail,
                SCName = customerObj.LinkedSystemCustomer.SCName,
                IsMatch = customerObj.LinkedSystemCustomer.IsMatch,
                SCPhone = customerObj.LinkedSystemCustomer.SCPhone,
                SCPostalCode = customerObj.LinkedSystemCustomer.SCPostalCode,
                QbCustomerId = customerObj.LinkedSystemCustomer.QbCustomerId,
                SState = customerObj.LinkedSystemCustomer.SState
            };

            var systemCustomer = customerObj.LinkedSystemCustomer.SCName + "<br /> " + customerObj.LinkedSystemCustomer.SCAddress + ", " +
                customerObj.LinkedSystemCustomer.SState + "<br />" + customerObj.LinkedSystemCustomer.SCPostalCode + "<br />" +
                customerObj.LinkedSystemCustomer.SCPhone + "<br />" + customerObj.LinkedSystemCustomer.SCEmail;

            var qbObj = customerObj.LinkedQBCustomer.SCName + "<br /> " + customerObj.LinkedQBCustomer.SCAddress + ", " +
                customerObj.LinkedQBCustomer.SState + "<br />" + customerObj.LinkedQBCustomer.SCPostalCode + "<br />" +
                customerObj.LinkedQBCustomer.SCPhone + "<br />" + customerObj.LinkedQBCustomer.SCEmail;

            return Json(new
            {
                success = true,
                customer = model1,
                systemcustmer = systemCustomer,
                qbCustomer = qbObj
            });
        }

        [HttpPost]
        [Route("~/Quickbooks/UnlinkCustomer")]
        public ActionResult UnlinkCustomer(UnlinkCustomersModel model)
        {
            var items = GetSessionDataToBeLinked();
            var ids = new List<int>();

            if (model.EntityId != null && model.QBEntityId != null)
            {
                // unlink actual linked customer
                items.UnLinkActualLinkedCustomerIds.Add(model.EntityId.Value);
            }
            else
            {
                if (model.EntityId != null)
                {
                    // unlink or remove customerid  from session
                    if (items.LinkedSystemCustomers.Contains(model.EntityId.Value))
                        items.LinkedSystemCustomers.Remove(model.EntityId.Value);
                }
                else if (model.QBEntityId != null)
                {
                    // unlink or remove qbcustomerid from session
                    if (items.LinkedQBCustomers.Contains(model.QBEntityId.Value))
                        items.LinkedQBCustomers.Remove(model.QBEntityId.Value);
                }
            }
            Session["LinkedCustomerOperations"] = items;

            var updateListData = GetSessionData();
            var objExist = model.EntityId != null ? updateListData.LinkedCustomers
                .FirstOrDefault(x => x.LinkedSystemCustomer.SCCustomerId == model.EntityId.Value) : updateListData.LinkedCustomers
                .FirstOrDefault(x => x.LinkedQBCustomer.QbCustomerId == model.EntityId.Value.ToString());

            if (objExist != null)
            {
                objExist.LinkedSystemCustomer.IsLinked = false;
                updateListData.LinkedCustomers.Remove(objExist);

                if (objExist.LinkedQBCustomer.SCCustomerId == null)
                {
                    updateListData.UnlinkedQBCustomers.Add(objExist.LinkedQBCustomer);
                }
                else
                {
                    updateListData.UnlinkedSystemCustomers.Add(objExist.LinkedSystemCustomer);
                }
            }
            Session["SyncListData"] = updateListData;
            return PartialView("LinkedCustomersView", updateListData);
        }

        [HttpPost, Route("~/Quickbooks/SaveUpdatedList")]
        public ActionResult SaveUpdatedList()
        {
            var sessionData = GetSessionDataToBeLinked();
            sessionData.CompanyId = CurrentIdentity.CompanyId.Value;
            var data = Mapper.Map<SyncCustomerRequest>(sessionData);
            var response = _customerServices.UpdateSynList(data);
            if (response.Success)
            {
                Session["MainSyncListData"] = null;
                Session["LinkedCustomerOperations"] = null;
                Session["SyncListData"] = null;
            }

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost, Route("~/Quickbooks/ShowLinkedCustomers")]
        public ActionResult ShowLinkedCustomers(LinkCustomerModel model)
        {
            var data = GetSessionData();

            if (model.SystemCustomers == null || model.QbCustomers == null)
            {
                if (model.SystemCustomers == null)
                {
                    var qbCustomerId = model.QbCustomers.FirstOrDefault();
                    // integrate qb customer
                    var qbCustomer = data.UnlinkedQBCustomers.FirstOrDefault(x => x.QbCustomerId == qbCustomerId.ToString());

                    var model1 = new Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel
                    {
                        SCAddress = qbCustomer.SCAddress,
                        SCity = qbCustomer.SCity,
                        CompanyId = qbCustomer.CompanyId,
                        SCountry = qbCustomer.SCountry,
                        SCCustomerId = qbCustomer.SCCustomerId,
                        SCEmail = qbCustomer.SCEmail,
                        SCName = qbCustomer.SCName,
                        IsMatch = true,
                        SCPhone = qbCustomer.SCPhone,
                        SCPostalCode = qbCustomer.SCPostalCode,
                        QbCustomerId = qbCustomer.QbCustomerId,
                        SState = qbCustomer.SState
                    };

                    var systemCustomer = qbCustomer.SCName + "<br /> " + qbCustomer.SCAddress + ", " +
                      qbCustomer.SState + "<br />" + qbCustomer.SCPostalCode + "<br />" +
                       qbCustomer.SCPhone + "<br />" + qbCustomer.SCEmail;

                    var qbObj = qbCustomer.SCName + "<br /> " + qbCustomer.SCAddress + ", " +
                        qbCustomer.SState + "<br />" + qbCustomer.SCPostalCode + "<br />" +
                        qbCustomer.SCPhone + "<br />" + qbCustomer.SCEmail;
                    return Json(new
                    {
                        success = true,
                        customer = model1,
                        systemcustmer = systemCustomer,
                        qbCustomer = qbObj
                    });
                }
                else
                {
                    // integrate system customer
                    var systemCustomerId = model.SystemCustomers.FirstOrDefault();
                    var systemCust = data.UnlinkedSystemCustomers.FirstOrDefault(x => x.SCCustomerId == systemCustomerId);

                    var model1 = new Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel
                    {
                        SCAddress = systemCust.SCAddress,
                        SCity = systemCust.SCity,
                        CompanyId = systemCust.CompanyId,
                        SCountry = systemCust.SCountry,
                        SCCustomerId = systemCust.SCCustomerId,
                        SCEmail = systemCust.SCEmail,
                        SCName = systemCust.SCName,
                        IsMatch = true,
                        SCPhone = systemCust.SCPhone,
                        SCPostalCode = systemCust.SCPostalCode,
                        QbCustomerId = systemCust.QbCustomerId,
                        SState = systemCust.SState
                    };

                    var systemCustomer = systemCust.SCName + "<br /> " + systemCust.SCAddress + ", " +
                      systemCust.SState + "<br />" + systemCust.SCPostalCode + "<br />" +
                       systemCust.SCPhone + "<br />" + systemCust.SCEmail;

                    var qbObj = systemCust.SCName + "<br /> " + systemCust.SCAddress + ", " +
                        systemCust.SState + "<br />" + systemCust.SCPostalCode + "<br />" +
                        systemCust.SCPhone + "<br />" + systemCust.SCEmail;

                    return Json(new
                    {
                        success = true,
                        customer = model1,
                        systemcustmer = systemCustomer,
                        qbCustomer = qbObj
                    });

                }
            }
            else
            {
                var systemCustomerId = model.SystemCustomers.FirstOrDefault();
                var qbCustomerId = model.QbCustomers.FirstOrDefault();

                var customerObj = data.UnlinkedSystemCustomers.FirstOrDefault(x => x.SCCustomerId == systemCustomerId);
                var qbCustomer = data.UnlinkedQBCustomers.FirstOrDefault(x => x.QbCustomerId == qbCustomerId.ToString());

                var model1 = new Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel
                {
                    SCAddress = customerObj.SCAddress,
                    SCity = customerObj.SCity,
                    CompanyId = customerObj.CompanyId,
                    SCountry = customerObj.SCountry,
                    SCCustomerId = customerObj.SCCustomerId,
                    SCEmail = customerObj.SCEmail,
                    SCName = customerObj.SCName,
                    IsMatch = false,
                    SCPhone = customerObj.SCPhone,
                    SCPostalCode = customerObj.SCPostalCode,
                    QbCustomerId = qbCustomer.QbCustomerId,
                    SState = customerObj.SState
                };
                var systemCustomer = customerObj.SCName + "<br /> " + customerObj.SCAddress + ", " +
                    customerObj.SState + "<br />" + customerObj.SCPostalCode + "<br />" +
                    customerObj.SCPhone + "<br />" + customerObj.SCEmail;

                var qbObj = qbCustomer.SCName + "<br /> " + qbCustomer.SCAddress + ", " +
                    qbCustomer.SState + "<br />" + qbCustomer.SCPostalCode + "<br />" +
                    qbCustomer.SCPhone + "<br />" + qbCustomer.SCEmail;


                return Json(new
                {
                    success = true,
                    customer = model1,
                    systemcustmer = systemCustomer,
                    qbCustomer = qbObj
                });
            }
        }

        [HttpPost, Route("~/Quickbooks/UpdateLinkedCustomer")]
        public ActionResult UpdateLinkedCustomer(Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel model)
        {
            try
            {
                var data = GetSessionDataToBeLinked();
                var IsRecordExists = data.CustomersEdited.FirstOrDefault(x => x.SCCustomerId == model.SCCustomerId);

                if (IsRecordExists != null)
                {
                    data.CustomersEdited.Remove(IsRecordExists);
                }
                data.CustomersEdited.Add(model);
                Session["LinkedCustomerOperations"] = data;

                // update list to be shown
                var updateListData = GetSessionData();

                // customer id==null means quickbooks customer is being edited
                var customerObj = model.SCCustomerId == null ? updateListData.LinkedCustomers.FirstOrDefault(x => x.LinkedQBCustomer.QbCustomerId == model.QbCustomerId) : updateListData.LinkedCustomers.FirstOrDefault(x => x.LinkedSystemCustomer.SCCustomerId == model.SCCustomerId);

                customerObj.LinkedSystemCustomer.SCAddress = model.SCAddress;
                customerObj.LinkedSystemCustomer.SCity = model.SCity;
                customerObj.LinkedSystemCustomer.SCEmail = model.SCEmail;
                customerObj.LinkedSystemCustomer.SCName = model.SCName;
                customerObj.LinkedSystemCustomer.SCPhone = model.SCPhone;
                customerObj.LinkedSystemCustomer.SCCustomerId = model.SCCustomerId;
                customerObj.LinkedSystemCustomer.QbCustomerId = model.QbCustomerId;
                customerObj.LinkedSystemCustomer.CompanyId = model.CompanyId;
                customerObj.LinkedSystemCustomer.SCPostalCode = model.SCPhone;
                customerObj.LinkedSystemCustomer.SCPostalCode = model.SCPostalCode;
                customerObj.LinkedSystemCustomer.SState = model.SState;
                customerObj.LinkedSystemCustomer.IsMatch = true;

                customerObj.LinkedQBCustomer.SCAddress = model.SCAddress;
                customerObj.LinkedQBCustomer.SCity = model.SCity;
                customerObj.LinkedQBCustomer.SCEmail = model.SCEmail;
                customerObj.LinkedQBCustomer.CompanyId = model.CompanyId;
                customerObj.LinkedQBCustomer.SCName = model.SCName;
                customerObj.LinkedQBCustomer.SCCustomerId = model.SCCustomerId;
                customerObj.LinkedQBCustomer.QbCustomerId = model.QbCustomerId;
                customerObj.LinkedQBCustomer.SCPhone = model.SCPhone;
                customerObj.LinkedQBCustomer.SCPostalCode = model.SCPhone;
                customerObj.LinkedQBCustomer.SCPostalCode = model.SCPostalCode;
                customerObj.LinkedQBCustomer.SState = model.SState;
                customerObj.LinkedQBCustomer.IsMatch = true;

                Session["SyncListData"] = updateListData;

                return PartialView("LinkedCustomersView", updateListData);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost, Route("~/Quickbooks/LinkUnlinkedCustomers")]
        public ActionResult LinkUnlinkedCustomers(Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel1 model)
        {
            var items = GetSessionDataToBeLinked();
            var obj = new Services.DTOs.QBEntitiesRequestResponse.SystemCustomerModel();

            obj.CompanyId = model.CompanyId1;
            obj.IsLinked = true;
            obj.IsMatch = true;
            obj.QbCustomerId = model.QbCustomerId1;
            obj.SCAddress = model.SCAddress1;
            obj.SCCustomerId = model.SCCustomerId1;
            obj.SCEmail = model.SCEmail1;
            obj.SCity = model.SCity1;
            obj.SCName = model.SCName1;
            obj.SCountry = model.SCountry1;
            obj.SCPhone = model.SCPhone1;
            obj.SCPostalCode = model.SCPostalCode1;
            obj.SState = model.SState1;
            items.CustomersEdited.Add(obj);

            if (model.SCCustomerId1 != null && model.QbCustomerId1 == null)
            {
                // link system customer
                items.LinkedSystemCustomers.Add(model.SCCustomerId1.Value);
            }
            if (model.SCCustomerId1 == null && model.QbCustomerId1 != null)
            {
                // link qb customer
                items.LinkedQBCustomers.Add(int.Parse(model.QbCustomerId1));
            }

            Session["LinkedCustomerOperations"] = items;

            var updateListData = GetSessionData();

            var linkedCustomer = new LinkedCustomer();
            linkedCustomer.LinkedSystemCustomer.CompanyId = model.CompanyId1;
            linkedCustomer.LinkedSystemCustomer.IsMatch = true;
            linkedCustomer.LinkedSystemCustomer.IsLinked = true;
            linkedCustomer.LinkedSystemCustomer.SCAddress = model.SCAddress1;
            linkedCustomer.LinkedSystemCustomer.SCCustomerId = model.SCCustomerId1;
            linkedCustomer.LinkedSystemCustomer.QbCustomerId = model.QbCustomerId1;
            linkedCustomer.LinkedSystemCustomer.SCEmail = model.SCEmail1;
            linkedCustomer.LinkedSystemCustomer.SCity = model.SCity1;
            linkedCustomer.LinkedSystemCustomer.SCName = model.SCName1;
            linkedCustomer.LinkedSystemCustomer.SCountry = model.SCountry1;
            linkedCustomer.LinkedSystemCustomer.SCPhone = model.SCPhone1;
            linkedCustomer.LinkedSystemCustomer.SCPostalCode = model.SCPostalCode1;
            linkedCustomer.LinkedSystemCustomer.SState = model.SState1;

            linkedCustomer.LinkedQBCustomer.SCAddress = model.SCAddress1;
            linkedCustomer.LinkedQBCustomer.SCity = model.SCity1;
            linkedCustomer.LinkedQBCustomer.SCEmail = model.SCEmail1;
            linkedCustomer.LinkedQBCustomer.IsLinked = true;
            linkedCustomer.LinkedQBCustomer.QbCustomerId = model.QbCustomerId1;
            linkedCustomer.LinkedQBCustomer.SCCustomerId = model.SCCustomerId1;
            linkedCustomer.LinkedQBCustomer.CompanyId = model.CompanyId1;
            linkedCustomer.LinkedQBCustomer.IsMatch = true;
            linkedCustomer.LinkedQBCustomer.SCName = model.SCName1;
            linkedCustomer.LinkedQBCustomer.SCPhone = model.SCPhone1;
            linkedCustomer.LinkedQBCustomer.SCPostalCode = model.SCPostalCode1;
            linkedCustomer.LinkedQBCustomer.SState = model.SState1;

            if (model.SCCustomerId1 != null)
            {
                var systsmeCustomer = updateListData.UnlinkedSystemCustomers.FirstOrDefault(x => x.SCCustomerId == model.SCCustomerId1);
                updateListData.UnlinkedSystemCustomers.Remove(systsmeCustomer);
            }
            if (model.QbCustomerId1 != null)
            {
                var qbCustomer = updateListData.UnlinkedQBCustomers.FirstOrDefault(x => x.QbCustomerId == model.QbCustomerId1.ToString());
                updateListData.UnlinkedQBCustomers.Remove(qbCustomer);
            }
            updateListData.LinkedCustomers.Add(linkedCustomer);
            Session["SyncListData"] = updateListData;

            return PartialView("LinkedCustomersView", updateListData);
        }

        #endregion

        #region  Employee Synchronization

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetEmployeeModal")]
        public ActionResult GetEmployeeModal()
        {
            ViewData["States"] = GetStates();
            var SyncListDataFromSession = GetEmployeeSessionData();
            return PartialView("LinkedEmployees", SyncListDataFromSession);
        }

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetEmployeeSynchronizationList")]
        public ActionResult GetEmployeeSynchronizationList()
        {
            ViewData["States"] = GetStates();
            SetEmployeeSession();
            var SyncListDataFromSession = GetEmployeeSessionData();
            return PartialView("LinkedEmployeesView", SyncListDataFromSession);
        }

        [HttpPost, Route("~/Quickbooks/LinkUnlinkedEmployees")]
        public ActionResult LinkUnlinkedEmployees(SystemEmployeeModel1 model)
        {
            var items = GetEmplSessionDataToBeLinked();
            var obj = new SystemEmployeeModel();

            obj.CompanyId = model.CompanyId1;
            obj.IsLinked = true;
            obj.IsMatch = true;
            obj.QBEmployeeId = model.QBEmployeeId1;
            obj.Address = model.Address1;
            obj.QBEmployeeId = model.QBEmployeeId1;
            obj.Email = model.Email1;
            obj.GivenName = model.GivenName1;
            obj.FamilyName = model.FamilyName1;

            items.EmployeesEdited.Add(obj);

            if (model.EmployeeId1 != null && model.QBEmployeeId1 == null)
            {
                // link system employee
                items.LinkedSystemEmployees.Add(model.EmployeeId1.Value);
            }
            if (model.EmployeeId1 == null && model.QBEmployeeId1 != null)
            {
                // link qb employee
                items.LinkedQBEmployees.Add(int.Parse(model.QBEmployeeId1));
            }
            Session["LinkedEmployeeOperations"] = items;

            var updateListData = GetEmployeeSessionData();
            var linkedEmployee = new LinkedEmployee();
            linkedEmployee.LinkedSystemEmployee.CompanyId = model.CompanyId1;
            linkedEmployee.LinkedSystemEmployee.IsMatch = true;
            linkedEmployee.LinkedSystemEmployee.IsLinked = true;
            linkedEmployee.LinkedSystemEmployee.Address = model.Address1;
            linkedEmployee.LinkedSystemEmployee.EmployeeId = model.EmployeeId1;
            linkedEmployee.LinkedSystemEmployee.QBEmployeeId = model.QBEmployeeId1;
            linkedEmployee.LinkedSystemEmployee.Email = model.Email1;
            linkedEmployee.LinkedSystemEmployee.GivenName = model.GivenName1;
            linkedEmployee.LinkedSystemEmployee.FamilyName = model.FamilyName1;

            linkedEmployee.LinkedQBEmployee.CompanyId = model.CompanyId1;
            linkedEmployee.LinkedQBEmployee.IsLinked = true;
            linkedEmployee.LinkedQBEmployee.QBEmployeeId = model.QBEmployeeId1;
            linkedEmployee.LinkedQBEmployee.Address = model.Address1;
            linkedEmployee.LinkedQBEmployee.Email = model.Email1;
            linkedEmployee.LinkedQBEmployee.EmployeeId = model.EmployeeId1;
            linkedEmployee.LinkedQBEmployee.CompanyId = model.CompanyId1;
            linkedEmployee.LinkedQBEmployee.IsMatch = true;
            linkedEmployee.LinkedQBEmployee.GivenName = model.GivenName1;
            linkedEmployee.LinkedQBEmployee.FamilyName = model.FamilyName1;

            if (model.EmployeeId1 != null)
            {
                var systsmeEmployee = updateListData.UnlinkedSystemEmployees.FirstOrDefault(x => x.EmployeeId == model.EmployeeId1);
                updateListData.UnlinkedSystemEmployees.Remove(systsmeEmployee);
            }
            if (model.QBEmployeeId1 != null)
            {
                var qbEmployee = updateListData.UnlinkedQBEmployees.FirstOrDefault(x => x.QBEmployeeId == model.QBEmployeeId1.ToString());
                updateListData.UnlinkedQBEmployees.Remove(qbEmployee);
            }
            updateListData.LinkedEmployees.Add(linkedEmployee);
            Session["EmployeeSyncListData"] = updateListData;
            return PartialView("LinkedEmployeesView", updateListData);
        }

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/SyncEmployeeResults")]
        public ActionResult SyncEmployeeResults()
        {
            ViewData["States"] = GetStates();

            var SyncListDataFromSession = GetEmployeeMainSessionData();
            if (SyncListDataFromSession == null || SyncListDataFromSession.LinkedEmployees.Count == 0 ||
                SyncListDataFromSession.UnlinkedSystemEmployees.Count == 0 ||
                SyncListDataFromSession.UnlinkedQBEmployees.Count == 0)
            {
                // fetch data first time on load
                SyncListDataFromSession = _companyWorkerServices.EmployeeSynchronizationList(CurrentIdentity.CompanyId);
                if (SyncListDataFromSession.Success)
                {
                    Session["EmployeeMainSyncListData"] = SyncListDataFromSession;
                    Session["EmployeeSyncListData"] = SyncListDataFromSession;
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Success = false;
                    ViewBag.Error = SyncListDataFromSession.Message;
                }
            }
            ViewBag.Success = true;
            return View();
        }

        [HttpPost, Route("~/Quickbooks/UpdateLinkedEmployee")]
        public ActionResult UpdateLinkedEmployee(SystemEmployeeModel model)
        {
            try
            {
                var data = GetEmplSessionDataToBeLinked();
                var IsRecordExists = data.EmployeesEdited.FirstOrDefault(x => x.EmployeeId == model.EmployeeId);

                if (IsRecordExists != null)
                {
                    data.EmployeesEdited.Remove(IsRecordExists);
                }
                data.EmployeesEdited.Add(model);
                Session["LinkedEmployeeOperations"] = data;

                // update list to be shown
                var updateListData = GetEmployeeSessionData();

                // customer id==null means quickbooks customer is being edited
                var employeeObj = model.EmployeeId == null ? updateListData.LinkedEmployees.FirstOrDefault(x => x.LinkedQBEmployee.QBEmployeeId == model.QBEmployeeId) : updateListData.LinkedEmployees.FirstOrDefault(x => x.LinkedSystemEmployee.EmployeeId == model.EmployeeId);

                employeeObj.LinkedSystemEmployee.GivenName = model.GivenName;
                employeeObj.LinkedSystemEmployee.FamilyName = model.FamilyName;
                employeeObj.LinkedSystemEmployee.Email = model.Email;
                employeeObj.LinkedSystemEmployee.EmployeeId = model.EmployeeId;
                employeeObj.LinkedSystemEmployee.QBEmployeeId = model.QBEmployeeId;
                employeeObj.LinkedSystemEmployee.CompanyId = model.CompanyId;
                employeeObj.LinkedSystemEmployee.IsMatch = true;

                employeeObj.LinkedQBEmployee.GivenName = model.GivenName;
                employeeObj.LinkedQBEmployee.FamilyName = model.FamilyName;
                employeeObj.LinkedQBEmployee.Email = model.Email;
                employeeObj.LinkedQBEmployee.CompanyId = model.CompanyId;
                employeeObj.LinkedQBEmployee.EmployeeId = model.EmployeeId;
                employeeObj.LinkedQBEmployee.QBEmployeeId = model.QBEmployeeId;
                employeeObj.LinkedQBEmployee.IsMatch = true;

                Session["EmployeeSyncListData"] = updateListData;

                return PartialView("LinkedEmployeesView", updateListData);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost, Route("~/Quickbooks/EditLinkedEmployee/{employeeId}")]
        public ActionResult EditLinkedEmployee(int? employeeId)
        {
            var data = GetEmployeeSessionData();
            var employeeObj = data.LinkedEmployees.FirstOrDefault(x => x.LinkedSystemEmployee.EmployeeId == employeeId);
            if (employeeObj == null)
            {
                // quickbooks cutomer which is just moved to linked customers TAB is being edited
                employeeObj = data.LinkedEmployees.FirstOrDefault(x => x.LinkedQBEmployee.QBEmployeeId == employeeId.ToString());
            }

            var model = new SystemEmployeeModel
            {
                GivenName = employeeObj.LinkedSystemEmployee.GivenName,
                FamilyName = employeeObj.LinkedSystemEmployee.FamilyName,
                CompanyId = employeeObj.LinkedSystemEmployee.CompanyId,
                EmployeeId = employeeObj.LinkedSystemEmployee.EmployeeId,
                Email = employeeObj.LinkedSystemEmployee.Email,
                QBEmployeeId = employeeObj.LinkedSystemEmployee.QBEmployeeId,
                IsMatch = employeeObj.LinkedSystemEmployee.IsMatch
            };

            var systemEmployee = employeeObj.LinkedSystemEmployee.GivenName + " " + employeeObj.LinkedSystemEmployee.FamilyName + "<br /> " + employeeObj.LinkedSystemEmployee.Email;

            var qEmployee = employeeObj.LinkedQBEmployee.GivenName + " " + employeeObj.LinkedQBEmployee.FamilyName + "<br /> " + employeeObj.LinkedQBEmployee.Email;

            return Json(new
            {
                success = true,
                employee = model,
                systememployee = systemEmployee,
                qbemployee = qEmployee
            });
        }

        [HttpPost, Route("~/Quickbooks/SaveEmployeeUpdatedList")]
        public ActionResult SaveEmployeeUpdatedList()
        {
            var sessionData = GetEmplSessionDataToBeLinked();
            sessionData.CompanyId = CurrentIdentity.CompanyId.Value;
            var data = Mapper.Map<SyncEmployeeRequest>(sessionData);
            var response = _companyWorkerServices.UpdateSyncList(data);

            if (response.Success)
            {
                Session["EmployeeMainSyncListData"] = null;
                Session["EmployeeSyncListData"] = null;
                Session["LinkedEmployeeOperations"] = null;
            }
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/Quickbooks/UnlinkEmployee")]
        public ActionResult UnlinkEmployee(UnlinkCustomersModel model)
        {
            var items = GetEmplSessionDataToBeLinked();
            var ids = new List<int>();

            if (model.EntityId != null && model.QBEntityId != null)
            {
                // unlink actual linked employee
                items.UnLinkActualLinkedEmployeeIds.Add(model.EntityId.Value);
            }
            else
            {
                if (model.EntityId != null)
                {
                    // unlink or remove employeeId  from session
                    if (items.LinkedSystemEmployees.Contains(model.EntityId.Value))
                        items.LinkedSystemEmployees.Remove(model.EntityId.Value);
                }
                else if (model.QBEntityId != null)
                {
                    // unlink or remove qbEmployeeId from session
                    if (items.LinkedQBEmployees.Contains(model.QBEntityId.Value))
                        items.LinkedQBEmployees.Remove(model.QBEntityId.Value);
                }
            }
            Session["LinkedEmployeeOperations"] = items;

            var updateListData = GetEmployeeSessionData();
            var objExist = model.EntityId != null ? (updateListData.LinkedEmployees
                .FirstOrDefault(x => x.LinkedSystemEmployee.EmployeeId == model.EntityId.Value)) : (updateListData.LinkedEmployees
                .FirstOrDefault(x => x.LinkedQBEmployee.QBEmployeeId == model.QBEntityId.Value.ToString()));

            if (objExist != null)
            {
                objExist.LinkedSystemEmployee.IsLinked = false;
                updateListData.LinkedEmployees.Remove(objExist);

                if (objExist.LinkedSystemEmployee.EmployeeId == null)
                {
                    updateListData.UnlinkedQBEmployees.Add(objExist.LinkedQBEmployee);
                }
                else
                {
                    updateListData.UnlinkedSystemEmployees.Add(objExist.LinkedSystemEmployee);
                }
            }
            Session["EmployeeSyncListData"] = updateListData;
            return PartialView("LinkedEmployeesView", updateListData);
        }

        [HttpPost, Route("~/Quickbooks/ShowLinkedEmployees")]
        public ActionResult ShowLinkedEmployees(LinkEmployeeModel model)
        {
            var data = GetEmployeeSessionData();

            if (model.SystemEmployees == null || model.QbEmployees == null)
            {
                if (model.SystemEmployees == null)
                {
                    var qbEmployeeId = model.QbEmployees.FirstOrDefault();
                    // integrate qb employee
                    var qbEmployee = data.UnlinkedQBEmployees.FirstOrDefault(x => x.QBEmployeeId == qbEmployeeId.ToString());

                    var model1 = new SystemEmployeeModel
                    {
                        Address = qbEmployee.Address,
                        GivenName = qbEmployee.GivenName,
                        FamilyName = qbEmployee.FamilyName,
                        CompanyId = qbEmployee.CompanyId,
                        EmployeeId = qbEmployee.EmployeeId,
                        Email = qbEmployee.Email,
                        IsMatch = true,
                        QBEmployeeId = qbEmployee.QBEmployeeId
                    };

                    var systemEmployee = qbEmployee.GivenName + " " + qbEmployee.FamilyName + "<br /> " +
                      qbEmployee.Email;
                    var qbObj = qbEmployee.GivenName + " " + qbEmployee.FamilyName + "<br /> " +
                      qbEmployee.Email;
                    return Json(new
                    {
                        success = true,
                        employee = model1,
                        systememployee = systemEmployee,
                        qbEmployee = qbObj
                    });
                }
                else
                {
                    // integrate system employee
                    var systemEmployeeId = model.SystemEmployees.FirstOrDefault();
                    var systemEmp = data.UnlinkedSystemEmployees.FirstOrDefault(x => x.EmployeeId == systemEmployeeId);

                    var model1 = new SystemEmployeeModel
                    {
                        Address = systemEmp.Address,
                        GivenName = systemEmp.GivenName,
                        FamilyName = systemEmp.FamilyName,
                        CompanyId = systemEmp.CompanyId,
                        EmployeeId = systemEmp.EmployeeId,
                        Email = systemEmp.Email,
                        IsMatch = true,
                        QBEmployeeId = systemEmp.QBEmployeeId
                    };
                    var systemEmployee = systemEmp.GivenName + " " + systemEmp.FamilyName + "<br /> " + systemEmp.Email;
                    var qbObj = systemEmp.GivenName + " " + systemEmp.FamilyName + "<br /> " + systemEmp.Email;

                    return Json(new
                    {
                        success = true,
                        employee = model1,
                        systememployee = systemEmployee,
                        qbEmployee = qbObj
                    });
                }
            }
            else
            {
                var systemEmployeeId = model.SystemEmployees.FirstOrDefault();
                var qbEmployeeId = model.QbEmployees.FirstOrDefault();

                var employeeObj = data.UnlinkedSystemEmployees.FirstOrDefault(x => x.EmployeeId == systemEmployeeId);
                var qbEmployee = data.UnlinkedQBEmployees.FirstOrDefault(x => x.QBEmployeeId == qbEmployeeId.ToString());
                var model1 = new SystemEmployeeModel
                {
                    Address = employeeObj.Address,
                    GivenName = employeeObj.GivenName,
                    FamilyName = employeeObj.FamilyName,
                    CompanyId = employeeObj.CompanyId,
                    EmployeeId = employeeObj.EmployeeId,
                    Email = employeeObj.Email,
                    IsMatch = true,
                    QBEmployeeId = employeeObj.QBEmployeeId
                };
                var systemEmployee = employeeObj.GivenName + " " + employeeObj.FamilyName + "<br /> " + employeeObj.Email;
                var qbObj = qbEmployee.GivenName + " " + qbEmployee.FamilyName + "<br /> " + qbEmployee.Email;

                return Json(new
                {
                    success = true,
                    employee = model1,
                    systememployee = systemEmployee,
                    qbEmployee = qbObj
                });
            }
        }

        #endregion

        #region Service Synchronization

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetServiceModal")]
        public ActionResult GetServiceModal()
        {
            var SyncListDataFromSession = GetServiceSessionData();
            return PartialView("LinkedServices", SyncListDataFromSession);
        }

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Quickbooks/GetServiceSynchronizationListData")]
        public ActionResult GetServiceSynchronizationListData()
        {
            SetServiceSession();
            var SyncListDataFromSession = GetServiceSessionData();
            return PartialView("LinkedServicesView", SyncListDataFromSession);
        }

        [HttpPost]
        [Route("~/Quickbooks/UnlinkService")]
        public ActionResult UnlinkService(UnlinkServiceModel model)
        {
            var items = GetServiceSessionDataToBeLinked();
            var ids = new List<int>();

            if (model.EntityId != null && model.QBEntityId != null)
            {
                items.UnLinkActualLinkedServicesIds.Add(model.EntityId.Value);
            }
            else
            {
                if (model.EntityId != null)
                {
                    if (items.LinkedSystemServices.Contains(model.EntityId.Value))
                        items.LinkedSystemServices.Remove(model.EntityId.Value);
                }
                else if (model.QBEntityId != null)
                {
                    if (items.LinkedQBServices.Contains(model.QBEntityId.Value))
                        items.LinkedQBServices.Remove(model.QBEntityId.Value);
                }
            }
            Session["LinkedServiceOperations"] = items;

            var updateListData = GetServiceSessionData();
            var objExist = model.EntityId != null ? updateListData.LinkedServices
                .FirstOrDefault(x => x.LinkedSystemService.ServiceId == model.EntityId.Value) : updateListData.LinkedServices
                .FirstOrDefault(x => x.LinkedQBService.QBServiceId == model.QBEntityId.Value.ToString());

            if (objExist != null)
            {
                objExist.LinkedSystemService.IsLinked = false;
                updateListData.LinkedServices.Remove(objExist);

                if (objExist.LinkedQBService.ServiceId == null)
                {
                    updateListData.UnlinkedQBServices.Add(objExist.LinkedQBService);
                }
                else
                {
                    updateListData.UnlinkedSystemServices.Add(objExist.LinkedSystemService);
                }
            }
            Session["SyncServiceListData"] = updateListData;
            return PartialView("LinkedServicesView", updateListData);
        }

        [HttpPost, Route("~/Quickbooks/EditLinkedService")]
        public ActionResult EditLinkedService(ServiceIdsModel model)
        {
            var data = GetServiceSessionData();

            var serviceObj = new LinkedService();

            serviceObj = data.LinkedServices.FirstOrDefault(x => x.LinkedSystemService.ServiceId == int.Parse(model.ServiceId));
            if (serviceObj == null)
            {
                serviceObj = data.LinkedServices.FirstOrDefault(x => x.LinkedQBService.QBServiceId == model.QBServiceId);
            }
            var model1 = new SystemServiceModel
            {
                IsMatch = serviceObj.LinkedSystemService.IsMatch,
                Description = serviceObj.LinkedSystemService.Description,
                Name = serviceObj.LinkedSystemService.Name,
                CompanyId = serviceObj.LinkedSystemService.CompanyId,
                QBServiceId = serviceObj.LinkedSystemService.QBServiceId,
                ServiceId = serviceObj.LinkedSystemService.ServiceId
            };
            var systemService = serviceObj.LinkedSystemService.Name + "<br /> " + serviceObj.LinkedSystemService.Description;
            var qbObj = serviceObj.LinkedQBService.Name + "<br /> " + serviceObj.LinkedQBService.Description;

            return Json(new
            {
                success = true,
                service = model1,
                systemservice = systemService,
                qbService = qbObj
            });
        }

        [HttpPost, Route("~/Quickbooks/SaveServiceUpdatedList")]
        public ActionResult SaveServiceUpdatedList()
        {
            var sessionData = GetServiceSessionDataToBeLinked();
            sessionData.CompanyId = CurrentIdentity.CompanyId.Value;
            var data = Mapper.Map<SyncServicesRequest>(sessionData);
            var response = _companyServiceService.UpdateSynList(data);
            if (response.Success)
            {
                Session["MainSyncServiceListData"] = null;
                Session["LinkedServiceOperations"] = null;
                Session["SyncServiceListData"] = null;
            }

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost, Route("~/Quickbooks/ShowLinkedServices")]
        public ActionResult ShowLinkedServices(LinkServiceModel model)
        {
            var data = GetServiceSessionData();

            if (model.SystemServices == null || model.QbServices == null)
            {
                if (model.SystemServices == null)
                {
                    var qbServiceId = model.QbServices.FirstOrDefault();
                    var qbService = data.UnlinkedQBServices.FirstOrDefault(x => x.QBServiceId == qbServiceId.ToString());
                    var model1 = new SystemServiceModel
                    {
                        IsMatch = true,
                        CompanyId = qbService.CompanyId,
                        Description = qbService.Description,
                        QBServiceId = qbService.QBServiceId,
                        Name = qbService.Name,
                        ServiceId = qbService.ServiceId
                    };
                    var systemCustomer = qbService.Name + "<br /> " + qbService.Description;
                    var qbObj = qbService.Name + "<br /> " + qbService.Description;

                    return Json(new
                    {
                        success = true,
                        service = model1,
                        systemservice = systemCustomer,
                        qbService = qbObj
                    });
                }
                else
                {
                    var systemServiceId = model.SystemServices.FirstOrDefault();
                    var systemSer = data.UnlinkedSystemServices.FirstOrDefault(x => x.ServiceId == systemServiceId);
                    var model1 = new SystemServiceModel
                    {
                        IsMatch = true,
                        CompanyId = systemSer.CompanyId,
                        Description = systemSer.Description,
                        QBServiceId = systemSer.QBServiceId,
                        Name = systemSer.Name,
                        ServiceId = systemSer.ServiceId
                    };
                    var systemCustomer = systemSer.Name + "<br /> " + systemSer.Description;
                    var qbObj = systemSer.Name + "<br /> " + systemSer.Description;

                    return Json(new
                    {
                        success = true,
                        service = model1,
                        systemservice = systemCustomer,
                        qbService = qbObj
                    });
                }
            }
            else
            {
                var systemServiceId = model.SystemServices.FirstOrDefault();
                var qbServiceId = model.QbServices.FirstOrDefault();

                var serviceObj = data.UnlinkedSystemServices.FirstOrDefault(x => x.ServiceId == systemServiceId);
                var qbService = data.UnlinkedQBServices.FirstOrDefault(x => x.QBServiceId == qbServiceId.ToString());

                var model1 = new SystemServiceModel
                {
                    IsMatch = false,
                    CompanyId = serviceObj.CompanyId,
                    Description = serviceObj.Description,
                    QBServiceId = qbService.QBServiceId,
                    Name = serviceObj.Name,
                    ServiceId = serviceObj.ServiceId
                };
                var systemService = serviceObj.Name + "<br /> " + serviceObj.Description;
                var qbObj = qbService.Name + "<br /> " + qbService.Description;

                return Json(new
                {
                    success = true,
                    service = model1,
                    systemservice = systemService,
                    qbService = qbObj
                });
            }
        }

        [HttpPost, Route("~/Quickbooks/UpdateLinkedService")]
        public ActionResult UpdateLinkedService(SystemServiceModel model)
        {
            try
            {
                var data = GetServiceSessionDataToBeLinked();
                var IsRecordExists = data.ServicesEdited.FirstOrDefault(x => x.ServiceId == model.ServiceId);

                if (IsRecordExists != null)
                {
                    data.ServicesEdited.Remove(IsRecordExists);
                }
                data.ServicesEdited.Add(model);
                Session["LinkedServiceOperations"] = data;
                var updateListData = GetServiceSessionData();
                var serviceObj = model.ServiceId == null ? updateListData.LinkedServices.FirstOrDefault(x => x.LinkedQBService.QBServiceId == model.QBServiceId) : updateListData.LinkedServices.FirstOrDefault(x => x.LinkedSystemService.ServiceId == model.ServiceId);
                serviceObj.LinkedSystemService.Name = model.Name;
                serviceObj.LinkedSystemService.Description = model.Description;
                serviceObj.LinkedSystemService.IsMatch = true;
                serviceObj.LinkedQBService.Name = model.Name;
                serviceObj.LinkedQBService.Description = model.Description;
                serviceObj.LinkedQBService.IsMatch = true;
                Session["SyncServiceListData"] = updateListData;
                return PartialView("LinkedServicesView", updateListData);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost, Route("~/Quickbooks/LinkUnlinkedServices")]
        public ActionResult LinkUnlinkedServices(SystemServiceModel1 model)
        {
            var items = GetServiceSessionDataToBeLinked();
            var obj = new SystemServiceModel();

            obj.CompanyId = model.CompanyId1;
            obj.IsLinked = true;
            obj.IsMatch = true;
            obj.QBServiceId = model.QBServiceId1;
            obj.Description = model.Description1;
            obj.ServiceId = model.ServiceId1;
            obj.Name = model.Name1;
            items.ServicesEdited.Add(obj);

            if (model.ServiceId1 != null && model.QBServiceId1 == null)
            {
                items.LinkedSystemServices.Add(model.ServiceId1.Value);
            }
            if (model.ServiceId1 == null && model.QBServiceId1 != null)
            {
                items.LinkedQBServices.Add(int.Parse(model.QBServiceId1));
            }
            Session["LinkedServiceOperations"] = items;
            var updateListData = GetServiceSessionData();

            var linkedService = new LinkedService();
            linkedService.LinkedSystemService.CompanyId = model.CompanyId1;
            linkedService.LinkedSystemService.IsMatch = true;
            linkedService.LinkedSystemService.IsLinked = true;
            linkedService.LinkedSystemService.Description = model.Description1;
            linkedService.LinkedSystemService.ServiceId = model.ServiceId1;
            linkedService.LinkedSystemService.QBServiceId = model.QBServiceId1;
            linkedService.LinkedSystemService.Name = model.Name1;

            linkedService.LinkedQBService.Description = model.Description1;
            linkedService.LinkedQBService.ServiceId = model.ServiceId1;
            linkedService.LinkedQBService.Name = model.Name1;
            linkedService.LinkedQBService.QBServiceId = model.QBServiceId1;
            linkedService.LinkedQBService.IsLinked = true;
            linkedService.LinkedQBService.CompanyId = model.CompanyId1;
            linkedService.LinkedQBService.IsMatch = true;

            if (model.ServiceId1 != null)
            {
                var systemService = updateListData.UnlinkedSystemServices.FirstOrDefault(x => x.ServiceId == model.ServiceId1);
                updateListData.UnlinkedSystemServices.Remove(systemService);
            }
            if (model.QBServiceId1 != null)
            {
                var qbService = updateListData.UnlinkedQBServices.FirstOrDefault(x => x.QBServiceId == model.QBServiceId1.ToString());
                updateListData.UnlinkedQBServices.Remove(qbService);
            }
            updateListData.LinkedServices.Add(linkedService);
            Session["SyncServiceListData"] = updateListData;

            return PartialView("LinkedServicesView", updateListData);
        }

        #endregion

        #endregion

        #region Private Method(s) 

        private QuickbooksSettingsModel GetQuickbooksSettings(int companyId)
        {
            var qbSettings = _qbSettings.GetQuickbookSettings(companyId);

            var model = new QuickbooksSettingsModel()
            {
                QbSettingsId = qbSettings.QuickbookSettingsId,
                ClientId = qbSettings.ClientId,
                CompanyId = qbSettings.CompanyId,
                ClientSecret = qbSettings.ClientSecret,
                RealmId = qbSettings.RealmId,
                DefaultPassword = qbSettings.DefaultPassword,
                InvoicePrefix = qbSettings.InvoicePrefix,
                IsCopyWFInvoicesToQB = qbSettings.IsCopyWFInvoicesToQB,
                CustomersSettings = qbSettings.CustomerSettings,
                EmployeesSettings = qbSettings.EmployeeSettings,
                ServicesSettings = qbSettings.ServiceSettings,
                IsQuickbooksIntegrated = qbSettings.IsQuickbooksIntegrated
            };
            return model;
        }
        
        private List<int> GetDataFromSession()
        {
            if (Session["unlinkids"] == null) return new List<int>();

            var items = (List<int>)Session["unlinkids"];
            return items;
        }

        #region Private Customers Methods
        
        private CustomerSynchronizationList GetSessionData()
        {
            if (Session["SyncListData"] == null) return new CustomerSynchronizationList();

            var items = (CustomerSynchronizationList)Session["SyncListData"];
            return items;
        }

        // main session list
        private CustomerSynchronizationList GetMainSessionData()
        {
            if (Session["MainSyncListData"] == null) return new CustomerSynchronizationList();

            var items = (CustomerSynchronizationList)Session["MainSyncListData"];
            return items;
        }

        // session list of service layer
        private SyncCustomerListModel GetSessionDataToBeLinked()
        {
            if (Session["LinkedCustomerOperations"] == null) return new SyncCustomerListModel();

            var items = (SyncCustomerListModel)Session["LinkedCustomerOperations"];
            return items;
        }

        private void SetCustomerSession()
        {
            ViewData["States"] = GetStates();
            var SyncListDataFromSession = GetMainSessionData();

            if (SyncListDataFromSession == null || SyncListDataFromSession.LinkedCustomers.Count == 0 ||
                SyncListDataFromSession.UnlinkedSystemCustomers.Count == 0 ||
                SyncListDataFromSession.UnlinkedQBCustomers.Count == 0)
            {
                // fetch data first time on load
                SyncListDataFromSession = _customerServices.CustomerSynchronizationList(CurrentIdentity.CompanyId);
                if (SyncListDataFromSession.Success)
                {
                    Session["MainSyncListData"] = SyncListDataFromSession;
                    Session["SyncListData"] = SyncListDataFromSession;
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Success = false;
                    ViewBag.Error = SyncListDataFromSession.Message;
                }
            }
            if (SyncListDataFromSession.LinkedCustomers.Count() != 0)
            {
                var totalUnmatched = SyncListDataFromSession.LinkedCustomers.Where(x => !x.LinkedSystemCustomer.IsMatch);
                if (totalUnmatched.Count() > 0)
                {
                    ViewBag.IsUnmatchRecordExists = true;
                }
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        #endregion

        #region Employee Private Methods

        // main session list
        private EmployeeSynchronizationList GetEmployeeMainSessionData()
        {
            if (Session["EmployeeMainSyncListData"] == null) return new EmployeeSynchronizationList();

            var items = (EmployeeSynchronizationList)Session["EmployeeMainSyncListData"];
            return items;
        }

        private void SetEmployeeSession()
        {
            ViewData["States"] = GetStates();

            var SyncListDataFromSession = GetEmployeeMainSessionData();
            if (SyncListDataFromSession == null || SyncListDataFromSession.LinkedEmployees.Count == 0 ||
                SyncListDataFromSession.UnlinkedSystemEmployees.Count == 0 ||
                SyncListDataFromSession.UnlinkedQBEmployees.Count == 0)
            {
                // fetch data first time on load
                SyncListDataFromSession = _companyWorkerServices.EmployeeSynchronizationList(CurrentIdentity.CompanyId);
                if (SyncListDataFromSession.Success)
                {
                    Session["EmployeeMainSyncListData"] = SyncListDataFromSession;
                    Session["EmployeeSyncListData"] = SyncListDataFromSession;
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Success = false;
                    ViewBag.Error = SyncListDataFromSession.Message;
                }
            }
        }

        // session list ot be displayed
        private EmployeeSynchronizationList GetEmployeeSessionData()
        {
            if (Session["EmployeeSyncListData"] == null) return new EmployeeSynchronizationList();

            var items = (EmployeeSynchronizationList)Session["EmployeeSyncListData"];
            return items;
        }

        // session list of service layer
        private SyncEmployeeListModel GetEmplSessionDataToBeLinked()
        {
            if (Session["LinkedEmployeeOperations"] == null) return new SyncEmployeeListModel();

            var items = (SyncEmployeeListModel)Session["LinkedEmployeeOperations"];
            return items;
        }

        #endregion

        #region Private Services Methods

        private void SetServiceSession()
        {
            var SyncListDataFromSession = GetServiceMainSessionData();

            if (SyncListDataFromSession == null || SyncListDataFromSession.LinkedServices.Count == 0 ||
                SyncListDataFromSession.UnlinkedSystemServices.Count == 0 ||
                SyncListDataFromSession.UnlinkedQBServices.Count == 0)
            {
                // fetch data first time on load
                SyncListDataFromSession = _companyServiceService.ServiceSynchronizationList(CurrentIdentity.CompanyId);
                if (SyncListDataFromSession.Success)
                {
                    Session["MainSyncServiceListData"] = SyncListDataFromSession;
                    Session["SyncServiceListData"] = SyncListDataFromSession;
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Success = false;
                    ViewBag.Error = SyncListDataFromSession.Message;
                }
            }
            if (SyncListDataFromSession.LinkedServices.Count() != 0)
            {
                var totalUnmatched = SyncListDataFromSession.LinkedServices.Where(x => !x.LinkedSystemService.IsMatch);
                if (totalUnmatched.Count() > 0)
                {
                    ViewBag.IsUnmatchRecordExists = true;
                }
            }
        }

        // session list ot be displayed
        private ServiceSynchronizationList GetServiceSessionData()
        {
            if (Session["SyncServiceListData"] == null) return new ServiceSynchronizationList();

            var items = (ServiceSynchronizationList)Session["SyncServiceListData"];
            return items;
        }

        // main session list
        private ServiceSynchronizationList GetServiceMainSessionData()
        {
            if (Session["MainSyncServiceListData"] == null) return new ServiceSynchronizationList();
            var items = (ServiceSynchronizationList)Session["MainSyncServiceListData"];
            return items;
        }

        // session list of service layer
        private SyncServiceListModel GetServiceSessionDataToBeLinked()
        {
            if (Session["LinkedServiceOperations"] == null) return new SyncServiceListModel();

            var items = (SyncServiceListModel)Session["LinkedServiceOperations"];
            return items;
        }

        #endregion

        private IEnumerable<SelectListItem> GetStates()
        {
            var states = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text="--Select--" },
                new SelectListItem { Value = "AL", Text="Alabama" },
                new SelectListItem { Value = "AK", Text="Alaska" },
                new SelectListItem { Value = "AZ", Text="Arizona" },
                new SelectListItem { Value = "AR", Text="Arkansas" },
                new SelectListItem { Value = "CA", Text="California" },
                new SelectListItem { Value = "CO", Text="Colorado" },
                new SelectListItem { Value = "CT", Text="Connecticut" },
                new SelectListItem { Value = "DE", Text="Delaware" },
                new SelectListItem { Value = "DC", Text="District of Columbia" },
                new SelectListItem { Value = "FL", Text="Florida" },
                new SelectListItem { Value = "GA", Text="Georgia" },
                new SelectListItem { Value = "HI", Text="Hawaii" },
                new SelectListItem { Value = "ID", Text="Idaho" },
                new SelectListItem { Value = "IL", Text="Illinois" },
                new SelectListItem { Value = "IN", Text="Indiana" },
                new SelectListItem { Value = "IA", Text="Iowa" },
                new SelectListItem { Value = "KS", Text="Kansas" },
                new SelectListItem { Value = "KY", Text="Kentucky" },
                new SelectListItem { Value = "LA", Text="Louisiana" },
                new SelectListItem { Value = "ME", Text="Maine" },
                new SelectListItem { Value = "MD", Text="Maryland" },
                new SelectListItem { Value = "MA", Text="Massachusetts" },
                new SelectListItem { Value = "MI", Text="Michigan" },
                new SelectListItem { Value = "MN", Text="Minnesota" },
                new SelectListItem { Value = "MS", Text="Mississippi" },
                new SelectListItem { Value = "MO", Text="Missouri" },
                new SelectListItem { Value = "MT", Text="Montana" },
                new SelectListItem { Value = "NE", Text="Nebraska" },
                new SelectListItem { Value = "NV", Text="Nevada" },
                new SelectListItem { Value = "NH", Text="New Hampshire" },
                new SelectListItem { Value = "NJ", Text="New Jersey" },
                new SelectListItem { Value = "NM", Text="New Mexico" },
                new SelectListItem { Value = "NY", Text="New York" },
                new SelectListItem { Value = "NC", Text="North Carolina" },
                new SelectListItem { Value = "ND", Text="North Dakota" },
                new SelectListItem { Value = "OH", Text="Ohio" },
                new SelectListItem { Value = "OK", Text="Oklahoma" },
                new SelectListItem { Value = "OR", Text="Oregon" },
                new SelectListItem { Value = "PA", Text="Pennsylvania" },
                new SelectListItem { Value = "RI", Text="Rhode Island" },
                new SelectListItem { Value = "SC", Text="South Carolina" },
                new SelectListItem { Value = "SD", Text="South Dakota" },
                new SelectListItem { Value = "TN", Text="Tennessee" },
                new SelectListItem { Value = "TX", Text="Texas" },
                new SelectListItem { Value = "UT", Text="Utah" },
                new SelectListItem { Value = "VT", Text="Vermont" },
                new SelectListItem { Value = "VA", Text="Virginia" },
                new SelectListItem { Value = "WA", Text="Washington" },
                new SelectListItem { Value = "WV", Text="West Virginia" },
                new SelectListItem { Value = "WI", Text="Wisconsin" },
                new SelectListItem { Value = "WY", Text="Wyoming" },
                new SelectListItem { Value = "AS", Text="American Samoa" },
                new SelectListItem { Value = "GU", Text="Guam" },
                new SelectListItem { Value = "MP", Text="Northern Mariana Islands" },
                new SelectListItem { Value = "PR", Text="Puerto Rico" },
                new SelectListItem { Value = "VI", Text="U.S. Virgin Islands" }
            };
            return states;
        }

        #endregion
    }
}
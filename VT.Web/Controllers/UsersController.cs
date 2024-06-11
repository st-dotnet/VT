using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using VT.Common;
using VT.Common.Utils;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using VT.Web.Models;
using DataAccess;
using VT.QuickBooks.Interfaces;
using EO.Pdf;

namespace VT.Web.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        #region Fields

        private readonly ICompanyWorkerService _companyWorkerService;
        private readonly ICompanyService _companyService;
        private readonly IServiceRecordService _serviceRecordService;
        private readonly IServiceRecordItemService _serviceRecordItemService;
        private readonly ICustomerService _customerService;
        private readonly IInvoices _qbInvoiceService;
        private readonly ICustomer _qbCustomerService;
        private readonly IEmployee _qbEmployeeService;

        #endregion

        #region Constructor

        public UsersController(ICompanyWorkerService companyWorkerService,
            Services.Interfaces.ICompanyService companyService,
            IServiceRecordService serviceRecordService,
            IServiceRecordItemService serviceRecordItemService,
            ICustomerService customerService,
            IInvoices qbInvoiceService, ICustomer qbCustomerService, IEmployee qbEmployeeService)
        {
            _companyWorkerService = companyWorkerService;
            _companyService = companyService;
            _serviceRecordService = serviceRecordService;
            _serviceRecordItemService = serviceRecordItemService;
            _customerService = customerService;
            _qbInvoiceService = qbInvoiceService;
            _qbCustomerService = qbCustomerService;
            _qbEmployeeService = qbEmployeeService;
        }

        #endregion

        #region Action Methods

        //display all list of users
        [Route("~/Users")]
        public ActionResult Index()
        {
            PopulateViews();
            ViewData["OrgId"] = !string.IsNullOrEmpty(Request.QueryString["oid"]) ? Request.QueryString["oid"].ToString() : string.Empty;

            var from = new List<SelectListCustomModel>();

            // load customers pertaining to specific org(Logged In Company Admin)
            if (CurrentIdentity.CompanyId != null)
            {
                var customers = _customerService.GetAllCustomers(CurrentIdentity.CompanyId.Value);
                // from list dropdown
                from = customers.Select(x => new SelectListCustomModel
                {
                    Value = x.CustomerId.ToString(),
                    Text = x.Name,
                    IsDeleted = x.IsDeleted
                }).ToList();
            }

            ViewData["From"] = from;
            ViewData["To"] = new List<SelectListItem>();
            return View();
        }

        //read method of kendo user grid
        [HttpPost]
        [Route("~/Users/UserList")]
        public ActionResult UserList([DataSourceRequest] DataSourceRequest request, string additionalInfo)
        {
            bool? filter = null;
            if (additionalInfo == "Active") filter = true;
            if (additionalInfo == "Not Active") filter = false;

            #region Void Invoice on QuickBooks

            //var voidInvoiceRequest = new VoidInvoiceModel
            //{
            //    Id = "154",      //String, filterable, sortable Unique identifier for this object.Sort order is ASC by default.
            //    SyncToken = "0"  // (String) Version number of the object.It is used to lock an object for use by one app at a time.As soon as an application modifies an object, its SyncToken is incremented.Attempts to modify an object specifying an older SyncToken fails.Only the latest version of the object is maintained by QuickBooks Online.
            //};

            //var jsonRequest = JsonConvert.SerializeObject(voidInvoiceRequest);
            //var voidResponse = _qbInvoiceService.VoidInvoice(jsonRequest);

            // DO POST SUCCESS STUFF

            #endregion

            #region Delete Invoice from QuickBooks

            //var deleteRequest = new DeleteInvoiceModel
            //{
            //    Id = "151",   // invoice id
            //    SyncToken = "3"    //Version number of the object. It is used to lock an object for use by one app at a time. As soon as an                            application modifies an object, its SyncToken is incremented. Attempts to modify an object specifying an                            older SyncToken fails. Only the latest version of the object is maintained by QuickBooks Online.
            //};

            //var jsonRequest = JsonConvert.SerializeObject(deleteRequest);
            //var response = _qbInvoiceService.DeleteInvoice(jsonRequest);

            // DO POST SUCCESS STUFF

            #endregion

            #region Get Invoice from QuickBooks

            //int invoiceId = 149; // hard coded invoice id
            //var invoiceResponse = _qbInvoiceService.GetInvoice(invoiceId);

            #endregion

            #region Create Invoice from QuickBooks

            //string uri = string.Format("{0}/v3/company/{1}/query?query={2}", qboBaseUrl, realmId, encodedQuery);

            //var request1 = new CreateInvoice();
            //request1.Line = new List<Line>();
            //request1.CustomerRef = new CustomerRef();

            //var line = new Line();
            //line.Amount = 567;
            //line.DetailType = "SalesItemLineDetail";
            //line.SalesItemLineDetail = new SalesItemLineDetail
            //{
            //    ItemRef = new ItemRef
            //    {
            //        name = "Services",
            //        value = "1"
            //    }
            //};
            //request1.Line.Add(line);
            //request1.CustomerRef.value = "61";  // customer Id
            //var jsonInvoiceRequest = JsonConvert.SerializeObject(request1);

            //// create invoice request
            //var response = _qbInvoiceService.CreateInvoice(jsonInvoiceRequest);

            // DO SUCCESS TRUE/FALSE Related STUFF

            #endregion

            var data = filter != null ? GetUserList().Where(x => x.IsActive.Value == filter).ToList() : GetUserList().ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        //delete user(s) method
        [HttpPost]
        [Route("~/Users/DeleteUsers")]
        public ActionResult DeleteUsers(DeleteUsersViewModel model)
        {
            var response = _companyWorkerService.DeleteUsers(model.Ids);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/Users/GetUserAllDetails/{id}")]
        public ActionResult GetUserAllDetails(int id)
        {
            var user = _companyWorkerService.GetCompanyWorker(id);
            var model = Mapper.Map<UserDetailViewModel>(user);
            return PartialView("UserDetail", model);
        }

        [HttpPost]
        [Route("~/Users/SaveUser")]
        public ActionResult SaveUser(SaveUserModel model)
        {
            var response = _companyWorkerService.AddUser(new CompanyWorkerSaveRequest
            {
                CompanyId = CurrentIdentity.CompanyId != null ? CurrentIdentity.CompanyId : (model.CompanyWorkerId > 0 ? model.OrgId : model.CompanyId),
                CompanyWorkerId = model.CompanyWorkerId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Username,
                IsAdmin = model.IsAdmin,
                MiddleName = model.MiddleName,
                Password = model.AuthKey,
                AccessibleCustomers = model.To
            });

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/Users/GetPasswordResetModal/{id}")]
        public ActionResult GetPasswordResetModal(int id)
        {
            var model = new PasswordResetModel { UserId = id };
            return PartialView("ResetPassword", model);
        }

        [HttpPost]
        [Route("~/Users/ResetPassword")]
        public ActionResult ResetPassword(PasswordResetModel model)
        {
            var request = Mapper.Map<ResetPasswordRequest>(model);
            var response = _companyWorkerService.ResetUserPassword(request);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // activate org
        [HttpPost]
        [Route("~/Users/ActivateEmp/{id}")]
        public ActionResult ActivateEmp(int id)
        {
            var response = _companyWorkerService.ActivateWorker(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        // Deactivate user
        [HttpPost]
        [Route("~/Users/DeactivateEmp/{id}")]
        public ActionResult DeactivateEmp(int id)
        {
            var response = _companyWorkerService.DeactivateWorker(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        [HttpPost]
        [Route("~/Users/ServiceRecords/{id}")]
        public ActionResult ServiceRecords(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Users/ServiceRecordItems/{id}")]
        public ActionResult ServiceRecordItems(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordItemList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Route("~/Users/CheckEmail")]
        public ActionResult CheckEmail(string email)
        {
            var result = _companyWorkerService.IsEmailAlreadyExist(email);
            return Content(result.Success ? "true" : "false");
        }

        [HttpPost]
        [Route("~/Users/VerifyImport")]
        public ActionResult VerifyImport()
        {
            var response = new ExportUserListModel();
            try
            {
                if (Request.Files.Count > 0)
                {
                    var httpPostedFileBase = Request.Files[0];
                    if (httpPostedFileBase != null)
                    {
                        response.Data = GetDataFromFile(httpPostedFileBase);

                        var recordCount = response.Data.Count;
                        var validCount = response.Data.Count(x => x.Status == ExportStatus.Valid.ToString());
                        var inValidCount = response.Data.Count(x => x.Status == ExportStatus.Invalid.ToString());

                        if (validCount == recordCount)
                        {
                            response.Message = "All records are valid.";
                            response.Css = "success";
                        }
                        else if (inValidCount == recordCount)
                        {
                            response.Message = "All records are invalid.";
                            response.Css = "danger";
                        }
                        else
                        {
                            response.Message = string.Format("There are {0} valid records and {1} invalid records.",
                                validCount, inValidCount);
                            response.Css = "info";
                        }
                        response.Success = true;
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = "Csv file contains no data or is not valid.";
                    response.Css = "danger";
                }
            }
            catch (Exception exception)
            {
                response.Success = false;
                response.Message = "Csv file contains no data or is not valid.";
                response.Css = "danger";
            }
            return Json(response);
        }

        [HttpPost]
        [Route("~/Users/Import")]
        public ActionResult Import(ImportUsersModel model, HttpPostedFileBase uploadCsv)
        {
            if (uploadCsv != null && uploadCsv.ContentLength > 0)
            {
                var data = GetDataFromFile(uploadCsv);

                foreach (var dataItem in data.Where(x => x.Status == ExportStatus.Valid.ToString()))
                {
                    _companyWorkerService.AddUser(new CompanyWorkerSaveRequest
                    {
                        CompanyId = model.OrganizationId,
                        Email = dataItem.Email,
                        FirstName = dataItem.FirstName,
                        LastName = dataItem.LastName,
                        MiddleName = dataItem.MiddleName,
                        IsAdmin = dataItem.IsAdministrator,
                        Password = dataItem.Password
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Records imported successfully."
                });
            }
            return Json(new
            {
                success = false,
                message = "Csv file contains not data or not valid."
            });
        }

        [HttpPost]
        [Route("~/Users/UserCustomerAccess")]
        public ActionResult UserCustomerAccess(CustomerAccessModel model)
        {
            var response = _customerService.UserCustomerAccess(new UserCustomerAccessRequest
            {
                Customers = model.ToList,
                UserId = model.CompanyWorkerUserId
            });

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/Users/GetUserDetail/{id?}")]
        public JsonResult GetUserDetail(int? id)
        {
            var model = id != null ? GetUser(id.Value) : new SaveUserModel { CompanyId = CurrentIdentity.CompanyId };
            if (id != null)
            {
                // list of all customers of this user's company
                var customers = _customerService.GetAllCustomersForUser(id.Value);  // id is user id 

                // list of accessible customers for this user
                var accessList = _customerService.GetUserCustomerAccess(id.Value);
                model.To = accessList.OrderBy(x => x.CustomerOrder).Select(x => x.CustomerId).ToList();

                // from list dropdown
                model.FromList = customers.Where(x => !model.To.Contains(x.CustomerId)).OrderBy(x => x.Name).Select(x => new SelectListCustomModel
                {
                    Value = x.CustomerId.ToString(),
                    Text = x.Name,
                    IsDeleted = x.IsDeleted
                }).ToList();

                // to list dropdown
                model.ToList = accessList.OrderBy(x => x.CustomerOrder).Select(x => new SelectListCustomModel
                {
                    Value = x.CustomerId.ToString(),
                    Text = x.Customer.Name,
                    IsDeleted = x.Customer.IsDeleted
                }).ToList();
            }

            return Json(model);
        }

        [Route("~/Users/GetUserCustomerAccess/{id}")]
        public ActionResult GetUserCustomerAccess(int id)
        {
            var model = new ViewCustomerAccessModel();

            // list of all customers of this user's company
            var customers = _customerService.GetAllCustomersForUser(id);  // id is user id 

            // list of accessible customers for this user
            var accessList = _customerService.GetUserCustomerAccess(id);

            var toList = accessList.OrderBy(x => x.CustomerOrder).Select(x => x.CustomerId).ToList();

            model.CompanyWorkerUserId = id;

            // from list dropdown
            model.FromList = customers.Where(x => !toList.Contains(x.CustomerId)).OrderBy(x => x.Name).Select(x => new SelectListCustomModel
            {
                Value = x.CustomerId.ToString(),
                Text = x.Name,
                IsDeleted = x.IsDeleted
            }).ToList();

            // to list dropdown
            model.ToList = accessList.OrderBy(x => x.CustomerOrder).Select(x => new SelectListCustomModel
            {
                Value = x.CustomerId.ToString(),
                Text = x.Customer.Name,
                IsDeleted = x.Customer.IsDeleted
            }).ToList();
            return Json(model);
        }

        [Route("~/Users/GetCustomers/{id}")]
        public ActionResult GetCustomers(int id) // company id
        {
            var customers = _customerService.GetAllCustomers(id);

            // from list dropdown
            var model = customers.OrderBy(x => x.Name).Select(x => new SelectListCustomModel
            {
                Value = x.CustomerId.ToString(),
                Text = x.Name,
                IsDeleted = x.IsDeleted
            }).ToList();

            return Json(model);
        }

        #endregion

        #region Private Methods

        private List<ExportUserValidation> GetDataFromFile(HttpPostedFileBase httpPostedFileBase)
        {
            var data = new List<ExportUserValidation>();
            var datatable = DataTable.New.ReadLazy(httpPostedFileBase.InputStream);
            var emails = _companyWorkerService.GetAllActiveUsersEmail();

            foreach (var row in datatable.Rows)
            {
                var dataItem = new ExportUserValidation();
                bool flag = true;

                if (string.IsNullOrEmpty(row["Email"]))
                {
                    flag = false;
                    dataItem.Reason = @"Email is empty.";
                }
                if (emails.Contains(row["Email"]))
                {
                    flag = false;
                    dataItem.Reason = @"Email already exists in the app.";
                }
                if (string.IsNullOrEmpty(row["FirstName"]))
                {
                    flag = false;
                    dataItem.Reason = @"First name is empty.";
                }
                if (string.IsNullOrEmpty(row["LastName"]))
                {
                    flag = false;
                    dataItem.Reason = @"Last name is empty.";
                }
                if (string.IsNullOrEmpty(row["Password"]))
                {
                    flag = false;
                    dataItem.Reason = @"Password name is empty.";
                }
                var isAdmin = false;
                if (!string.IsNullOrEmpty(row["IsAdministrator"]))
                {
                    var value = row["IsAdministrator"].Trim().ToUpper();

                    if (value == "Y" || value == "YES" || value == "TRUE")
                    {
                        isAdmin = true;
                    }
                    else if (value == "N" || value == "NO" || value == "FALSE")
                    {
                        isAdmin = false;
                    }
                    else
                    {
                        flag = false;
                        dataItem.Reason = @"Is Administrator is not valid.";
                    }
                }
                dataItem.Status = flag ? ExportStatus.Valid.ToString() : ExportStatus.Invalid.ToString();
                dataItem.FirstName = row["FirstName"];
                dataItem.MiddleName = row["MiddleName"];
                dataItem.LastName = row["LastName"];
                dataItem.Password = row["Password"];
                dataItem.Email = row["Email"];
                dataItem.IsAdministrator = isAdmin;
                data.Add(dataItem);
            }
            var duplicateEmails = data.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            foreach (var dataItem in data)
            {
                if (duplicateEmails.Contains(dataItem.Email))
                {
                    dataItem.Status = ExportStatus.Invalid.ToString();
                    dataItem.Reason = @"Email exists multiple times in the records.";
                }
            }
            return data;
        }

        private SaveUserModel GetUser(int id)
        {
            var data = _companyWorkerService.GetCompanyWorker(id);
            var model = Mapper.Map<SaveUserModel>(data);
            model.OrgId = model.CompanyId.GetValueOrDefault();
            return model;
        }

        private IEnumerable<UsersListViewModel> GetUserList()
        {
            var users = _companyWorkerService.GetAllUsers(CurrentIdentity.CompanyId);

            var list = users.Select(x => new UsersListViewModel
            {
                Id = x.CompanyWorkerId,
                Name = string.Format("{0} {1}", x.FirstName, x.LastName),
                Email = x.Email,
                CompanyId = x.CompanyId,
                CompanyName = x.Company != null ? x.Company.Name : "(N/A :Super Admin)",
                IsAdmin = x.IsAdmin ? "Yes" : "No",
                IsActive = !x.IsDeleted,
                IsCompanyDeleted = x.Company != null && x.Company.IsDeleted,
                IsDeleted = x.IsDeleted
            }).ToList();
            return list;
        }

        private IEnumerable<ServiceRecordDetail> GetServiceRecordList(int id)
        {
            var list = _serviceRecordService.GetServiceRecordsByCompanyWorker(id);
            var result = list.Select(x => new ServiceRecordDetail
            {
                BilledToCompany = x.BilledToCompany,
                CompanyId = x.CompanyId,
                CompanyName = x.Company != null ? x.Company.Name : "N/A",
                CompanyWorkerEmail = string.Format("{0} {1} ({2})", x.CompanyWorker.FirstName, x.CompanyWorker.LastName, x.CompanyWorker.Email),
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                Description = x.Description,
                EndTime = x.EndTime,
                HasNonService = x.ServiceRecordItems.Any(y => y.CostOfService == null || y.CostOfService == 0),
                ServiceRecordId = x.ServiceRecordId,
                StartTime = x.StartTime,
                TotalAmount = x.TotalAmount,
                Status = EnumUtil.GetDescription(x.Status)
            }).ToList();
            return result;
        }

        private IEnumerable<ServiceRecordItemDetail> GetServiceRecordItemList(int id)
        {
            var list = _serviceRecordItemService.GetRecordItems(id);
            var result = list.Select(x => new ServiceRecordItemDetail
            {
                CompanyServiceId = x.CompanyServiceId != null ? x.CompanyServiceId.Value : 0,
                CostOfService = x.CostOfService,
                ServiceName = x.ServiceName,
                CustomerId = x.CustomerId,
                EndTime = x.EndTime,
                ServiceRecordId = x.ServiceRecordId,
                StartTime = x.StartTime,
                Description = x.Description,
                ServiceRecordItemId = x.ServiceRecordItemId,
                Type = EnumUtil.GetDescription(x.Type),
            }).ToList();
            return result;
        }

        private void PopulateViews()
        {
            var list = GetOrganizations();
            ViewData["Organizations"] = list;
        }

        private List<SelectListItem> GetOrganizations()
        {
            var companies = _companyService.GetOranizationList().Where(x => !x.IsDeleted);
            var list = new List<SelectListItem> { new SelectListItem { Text = "--Select--", Value = "" } };
            list.AddRange(companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyId.ToString(CultureInfo.InvariantCulture),
                Selected = x.IsDeleted // we want to identify which all companies are deleted and want to apply some css on that
            }));
            return list;
        }

        #endregion
    }
}
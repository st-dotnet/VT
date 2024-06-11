using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DataAccess;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using VT.Common;
using VT.Common.Utils;
using VT.Data.Entities;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using VT.Web.Models;
using VT.Web.Models.QBCustomers;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.QuickBooks.Interfaces;

namespace VT.Web.Controllers
{
    public class CustomersController : BaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly Services.Interfaces.ICompanyService _companyService;
        private readonly ICompanyServiceService _companyServiceService;
        private readonly ICustomerServiceService _customerServiceService;
        private readonly IServiceRecordService _serviceRecordService;
        private readonly IServiceRecordItemService _serviceRecordItemService;
        private readonly IEmailService _emailService;
        private readonly IQuickbookSettings _qbSettings;

        #endregion

        #region Constructor

        public CustomersController(ICustomerService customerService,
            Services.Interfaces.ICompanyService companyService,
            ICompanyServiceService companyServiceService,
            ICustomerServiceService customerServiceService,
            IServiceRecordService serviceRecordService,
            IServiceRecordItemService serviceRecordItemService,
            IEmailService emailService, IQuickbookSettings qbSettings)
        {
            _customerService = customerService;
            _companyService = companyService;
            _qbSettings = qbSettings;
            _companyServiceService = companyServiceService;
            _customerServiceService = customerServiceService;
            _serviceRecordService = serviceRecordService;
            _serviceRecordItemService = serviceRecordItemService;
            _emailService = emailService;
        }

        #endregion

        #region Action Method(s)

        [HttpGet]
        [Route("~/Customers")]
        public ActionResult Index()
        {
            PopulateViews();
            ViewData["States"] = GetStates();         
            return View();
        }

        [HttpPost]
        [Route("~/Customers/DeleteCustomers")]
        public ActionResult DeleteCustomers(DeleteCustomerViewModel model)
        {
            var response = _customerService.DeleteCustomers(model.Ids);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }
        // activate customer
        [HttpPost]
        [Route("~/Customers/ActivateCustomer/{id}")]
        public ActionResult ActivateCustomer(int id)
        {
            var response = _customerService.ActivateCustomer(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        // activate customer credit card
        [HttpPost]
        [Route("~/Customers/ActivateCustomerCreditCard/{customerId}")]
        public ActionResult ActivateCustomerCreditCard(int customerId)
        {
            var response = _customerService.ActivateCustomerCreditCard(customerId);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        // deactivate customer credit card
        [HttpPost]
        [Route("~/Customers/DeactivateCustomerCreditCard/{customerId}")]
        public ActionResult DeactivateCustomerCreditCard(int customerId)
        {
            var response = _customerService.DeactivateCustomerCreditCard(customerId);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }
        // Deactivate customer
        [HttpPost]
        [Route("~/Customers/DeactivateCustomer/{id}")]
        public ActionResult DeactivateCustomer(int id)
        {
            var response = _customerService.DeactivateCustomer(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        [HttpPost]
        [Route("~/Customers/AddCustomer")]
        public ActionResult AddCustomer()
        {
            Session["data"] = null;
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [Route("~/Customers/EditCustomer/{id}")]
        public ActionResult EditCustomer(int id)
        {
            Session["data"] = null;

            var response = _customerService.GetCustomerDetail(id);

            if (response == null) return null;

            var model = new SaveCustomerViewModel
            {
                Name = response.Name,
                Address = response.Address,
                City = response.City,
                State = response.State,
                Country = response.Country,
                PostalCode = response.PostalCode,
                ContactEmail = response.ContactEmail,
                ContactFirstName = response.ContactFirstName,
                ContactLastName = response.ContactLastName,
                ContactMobile = response.ContactMobile,
                ContactTelephone = response.ContactTelephone,
                ContactMiddleName = response.ContactMiddleName,
                CustomerId = response.CustomerId,
                IsCcActive = response.IsCcActive,
                CompanyId = response.CompanyId,
                IsActive = response.IsActive,
                HasGatewayCustomer = !string.IsNullOrEmpty(response.GatewayCustomerId)
            };
            return Json(model);
        }

        [HttpPost]
        [Route("~/Customers/GetCustomerServicesGrid")]
        public ActionResult GetCustomerServicesGrid(BaseCustomerServiceViewModel model)
        {
            PopulateCompanyServices(model.CompanyId);
            return PartialView("CustomerServicesGrid", new SaveCustomerViewModel
            {
                CompanyId = model.CompanyId,
                CustomerId = model.CustomerId
            });
        }

        [HttpPost]
        [Route("~/Customers/SaveCustomer")]
        public ActionResult SaveCustomer(SaveCustomerViewModel model)
        {
            var response = _customerService.SaveCustomer(new CustomerSaveRequest
            {
                Address = model.Address,
                City = model.City,
                CompanyId = model.CompanyId,
                ContactEmail = model.ContactEmail,
                CustomerId = model.CustomerId,
                EditCompanyId = model.EditCompanyId,
                ContactFirstName = model.ContactFirstName,
                ContactLastName = model.ContactLastName,
                IsCcActive = model.IsCcActive,
                ContactMobile = model.ContactMobile,
                ContactTelephone = model.ContactTelephone,
                ContactMiddleName = model.ContactMiddleName,
                Country = model.Country,
                Name = model.Name,
                PostalCode = model.PostalCode,
                State = model.State,
                IsActive = model.IsActive
            });

            var data = GetDataFromSession();

            if (response.Success && data != null)
            {
                foreach (var customerServiceViewModel in data.Where(x => x.HasChanged || x.InMemoryAdded))
                {
                    var result = _customerServiceService.SaveCustomerService(new SaveCustomerServiceRequest
                    {
                        CustomerId = response.Customer.CustomerId,
                        CustomerServiceId = customerServiceViewModel.InMemoryAdded ? 0 : customerServiceViewModel.CustomerServiceId,
                        CompanyServiceId = customerServiceViewModel.CompanyServiceId,
                        Name = customerServiceViewModel.ServiceName,
                        Description = customerServiceViewModel.Description,
                        Price = customerServiceViewModel.Price
                    });
                }
                foreach (var customerServiceViewModel in data.Where(x => x.InMemoryDeleted))
                {
                    _customerServiceService.DeleteCustomerService(customerServiceViewModel.CustomerServiceId);
                }
            }

            Session["data"] = null;

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/Customers/CustomerList")]
        public ActionResult CustomerList([DataSourceRequest] DataSourceRequest request, string additionalInfo)
        {
            bool? filter = null;
            if (additionalInfo == "Active") filter = true;
            if (additionalInfo == "Not Active") filter = false;

            //#region Pull Customers From Customers

            //var qboBaseUrl = ApplicationSettings.QBBaseUrl;
            //var token = ApplicationSettings.AccessToken;
            //string realmId = ApplicationSettings.QBRealmId;

            //var principal = User as ClaimsPrincipal;
            //const string query = "select * from Customer";

            //// build the  request
            //string encodedQuery = WebUtility.UrlEncode(query);
            ////add qbobase url and query

            //// Url to create Invoivce.
            ////string uri = string.Format("{0}/v3/company/{1}/query?query={2}", qboBaseUrl, realmId, encodedQuery);

            //string uri = string.Format("{0}/v3/company/{1}/invoice", qboBaseUrl, realmId);
            //string result = "";

            //try
            //{
            //    var client = new HttpClient();
            //    client.DefaultRequestHeaders.Add("Accept", "application/json;charset=UTF-8");
            //    client.DefaultRequestHeaders.Add("ContentType", "application/json;charset=UTF-8");
            //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            //    //result = await client.GetStringAsync(uri);

            //    var request1 = new CreateInvoice();
            //    request1.Line = new List<Models.CustomersModels.Line>();
            //    request1.CustomerRef = new CustomerRef();

            //    /*  Hard coded creation of invoice request */
            //    var line = new Models.CustomersModels.Line();
            //    line.Amount = 789;
            //    line.DetailType = "SalesItemLineDetail";
            //    line.SalesItemLineDetail = new Models.CustomersModels.SalesItemLineDetail
            //    {
            //        ItemRef = new ItemRef
            //        {
            //            name = "Services",
            //            value = "1"
            //        }
            //    };

            //    request1.Line.Add(line);
            //    request1.CustomerRef.value = "61";

            //    var jsonInvoiceRequest = JsonConvert.SerializeObject(request1);
            //    var content = new StringContent(jsonInvoiceRequest.ToString(), Encoding.UTF8, "application/json");
            //    var response = await client.PostAsync(uri, content);
            //    if (response != null)
            //    {
            //        var customer = new QBCustomerModel();
            //        //var customers = JsonConvert.DeserializeObject<QBCustomerModel>(result);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return View("CallService", (object)"QBO API call Failed!");
            //}

            //#endregion

            var data = filter != null ? GetCustomerList().Where(x => x.IsActive.Value == filter).ToList() : GetCustomerList().ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/GetCustomerDetail/{id}")]
        public ActionResult GetCustomerDetail(int id)
        {
            var response = _customerService.GetCustomerDetail(id);

            if (response == null) return null;

            var model = new CustomerDetailViewModel
            {
                Name = response.Name,
                Address = response.Address,
                City = response.City,
                State = response.State,
                Country = response.Country,
                PostalCode = response.PostalCode,
                ContactEmail = response.ContactEmail,
                ContactFirstName = response.ContactFirstName,
                ContactLastName = response.ContactLastName,
                ContactMobile = response.ContactMobile,
                ContactTelephone = response.ContactTelephone,
                CustomerId = response.CustomerId,
                IsCreditCardSetup = response.IsCreditCardSetup,
                IsDeleted = response.IsDeleted
            };
            return PartialView("CustomerDetailView", model);
        }

        [HttpPost]
        [Route("~/Customers/ServiceRecords/{id}")]
        public ActionResult ServiceRecords(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/ServiceRecordItems/{id}")]
        public ActionResult ServiceRecordItems(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordItemList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/GetCustomerServicesDetail")]
        public ActionResult GetCustomerServicesDetail(BaseCustomerServiceViewModel request)
        {
            var model = new CustomerServiceViewModel
            {
                CustomerId = request.CustomerId,
                CompanyId = request.CompanyId,
            };
            PopulateCompanyServices(request.CompanyId);
            return PartialView("SaveCustomerService", model);
        }

        [HttpPost]
        [Route("~/Customers/GetCompanyServiceDetail/{id}")]
        public JsonResult GetCompanyServiceDetail(int id)
        {
            var companyService = _companyServiceService.GetCompanyService(id);
            return Json(new
            {
                success = companyService != null,
                message = companyService != null ? string.Empty : "Company service does not exist",
                result = new
                {
                    Name = companyService != null ? companyService.Name : string.Empty,
                    Description = companyService != null ? companyService.Description : string.Empty
                }
            });
        }
        [HttpPost]
        [Route("~/Customers/SaveCustomerServices")]
        public ActionResult SaveCustomerServices(CustomerServiceViewModel model)
        {
            //validation
            var message = string.Empty;

            if (model.CompanyServiceId < 1)
                message += "Company service is required.";

            if (string.IsNullOrEmpty(model.Description))
                message += "<br/> Description is required.";

            if (model.Price <= 0)
                message += "<br/> Price is required.";

            if (message.StartsWith("<br/>"))
            {
                message = message.Substring(5);
            }

            if (!string.IsNullOrEmpty(message))
            {
                return Json(new
                {
                    success = false,
                    message = message
                });
            }
            //New
            var data = GetDataFromSession();

            var companyService = GetCompanyService(model.CompanyServiceId);

            if (companyService != null)
                model.ServiceName = companyService.Name;

            var customerServiceFromData = data.FirstOrDefault(x => x.CustomerServiceId == model.CustomerServiceId);

            if (customerServiceFromData != null)
            {
                customerServiceFromData.CompanyServiceId = model.CompanyServiceId;
                customerServiceFromData.Description = model.Description;
                customerServiceFromData.Price = model.Price;
                customerServiceFromData.HasChanged = true;
            }
            else
            {
                if (data.Any(x => x.CompanyServiceId == model.CompanyServiceId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "For this company service record already exists."
                    });
                }

                model.CustomerServiceId = GetMaxId();
                model.InMemoryAdded = true;
                data.Add(model);
            }

            return Json(new
            {
                success = true,
                message = "Data saved."
            });
        }

        [Route("~/Customers/GetCustomerService/{id}")]
        public JsonResult GetCustomerService(int id)
        {
            var success = false;
            var data = GetDataFromSession();
            var customerService = data.FirstOrDefault(x => x.CustomerServiceId == id);
            success = customerService != null;
            return Json(new
            {
                success = success,
                customerServiceId = customerService != null ? customerService.CustomerServiceId : 0,
                companyServiceId = customerService != null ? customerService.CompanyServiceId : 0,
                description = customerService != null ? customerService.Description : string.Empty,
                price = customerService != null ? customerService.Price : 0,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/DeleteCustomerService/{id}")]
        public ActionResult DeleteCustomerService(int id)
        {
            var success = false;
            var message = string.Empty;

            var data = GetDataFromSession();
            if (data.Any())
            {
                var customerService = data.FirstOrDefault(x => x.CustomerServiceId == id);
                if (customerService != null)
                {
                    customerService.InMemoryDeleted = true;

                    success = true;
                    message = "Customer service deleted.";
                }
            }

            return Json(new
            {
                success = success,
                message = message
            });
        }

        [HttpPost]
        [Route("~/Customers/CustomerServiceList/{id}")]
        public ActionResult CustomerServiceList(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetCustomerServiceList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/DeleteCustomerServices")]
        public ActionResult DeleteCustomerServices(DeleteCustomerServiceViewModel request)
        {
            var result = _customerServiceService.DeleteCustomerServices(request.Ids);
            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost]
        [Route("~/Customers/FillCompanyServices/{id}")]
        public ActionResult FillCompanyServices(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetCustomerServiceList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("~/Customers/GetCompanyServices/{id}")]
        public ActionResult GetCompanyServices(int id, int? customerId)
        {
            List<CompanyServiceListViewModel> data;
            if (Session["data"] != null)
            {
                //use data from session
                var customerServices = GetDataFromSession();
                var companyServiceIds = customerServices.Select(x => x.CompanyServiceId);
                var companyServices = _companyServiceService.GetAllCompanyServices(id);
                data = companyServices.Where(x => !companyServiceIds.Contains(x.CompanyServiceId)).ToList()
                    .Select(y => new CompanyServiceListViewModel()
                    {
                        Id = y.CompanyServiceId,
                        Name = y.Name,
                        CompanyId = y.CompanyId,
                        Description = y.Description
                    }).ToList();
            }
            else
            {
                //use data from database
                data = GetCompanyServiceList(id, customerId.Value).ToList();
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //DropDownList Bind 
        [HttpGet]
        [Route("~/Customers/GetCompanyServicesNew")]
        public ActionResult GetCompanyServicesNew(int? companyId, int? customerId, int? companyServiceId)
        {
            List<CompanyServiceListViewModel> data;
            if (companyServiceId == 0 || companyServiceId == null)
            {
                if (Session["data"] != null)
                {
                    //use data from session
                    var customerServices = GetDataFromSession();
                    var companyServiceIds = customerServices.Select(x => x.CompanyServiceId);
                    var companyServices = _companyServiceService.GetAllCompanyServices(companyId);
                    data = companyServices.Where(x => !companyServiceIds.Contains(x.CompanyServiceId)).ToList()
                        .Select(y => new CompanyServiceListViewModel()
                        {
                            Id = y.CompanyServiceId,
                            Name = y.Name,
                            CompanyId = y.CompanyId,
                            Description = y.Description
                        }).ToList();
                }
                else
                {
                    //use data from database
                    data = GetCompanyServiceList(companyId.Value, customerId.Value).ToList();
                }
            }
            else
            {
                var companyService = _companyServiceService.GetCompanyService(companyServiceId.Value);
                data = new List<CompanyServiceListViewModel>
                {
                    new CompanyServiceListViewModel
                    {
                        CompanyId = companyService.CompanyId,
                        Description = companyService.Description,
                        Id = companyService.CompanyServiceId,
                        Name = companyService.Name
                    }
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/GetImportCustomersModal")]
        public ActionResult GetImportCustomersModal()
        {
            return PartialView("ImportCustomers", new ImportCustomerViewModel
            {
                Organizations = GetOrganizations()
            });
        }

        [HttpGet]
        [Route("~/Customers/GetSelectedCompanyServices/{id}")]
        public ActionResult GetSelectedCompanyServices(int id)
        {
            var companyService = _companyServiceService.GetCompanyService(id);
            var list = new List<CompanyServiceListViewModel>
            {
                new CompanyServiceListViewModel
                {
                    CompanyId = companyService.CompanyId,
                    Description = companyService.Description,
                    Id = companyService.CompanyServiceId,
                    Name = companyService.Name
                }
            };
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Customers/VerifyImport")]
        public ActionResult VerifyImport()
        {
            var response = new ImportCustomerListModel();

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
        [Route("~/Customers/SendEmail")]
        public ActionResult SendEmail(SendEmailViewModel model)
        {
            try
            {
                string template = EmailConstants.AddUpdateCcInfo;
                var token = Guid.NewGuid();

                var tokenResponse = _customerService.SetExpireTokenForCustomer(new CustomerSetExpireTokenRequest
                {
                    CustomerId = model.CustomerId,
                    Token = token.ToString()
                });

                if (tokenResponse.Success)
                {
                    var request = new SendCustomerEmailRequest
                    {
                        CustomerId = model.CustomerId,
                        FromEmail = ApplicationSettings.FromEmail,
                        SetCreditCardUrl =
                            string.Format("{0}/SetPaymentMethod/{1}", ApplicationSettings.SecureBaseUrl, token),
                        Template = template
                    };

                    var response = _emailService.SendEmail(request);

                    return Json(new
                    {
                        success = response.Success,
                        message = response.Message == null ? "Some error occured while sening email." : response.Message
                    });
                }
                return Json(new
                {
                    success = false,
                    message = "Some error occured while setting expire token. Please contact admin."
                });
            }
            catch (Exception exception)
            {
                return Json(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }

        [HttpPost]
        [Route("~/Customers/Import")]
        public ActionResult Import(ImportCustomerViewModel model, HttpPostedFileBase uploadCsv)
        {
            if (uploadCsv != null && uploadCsv.ContentLength > 0)
            {
                var data = GetDataFromFile(uploadCsv);
                data = data.Where(x => x.Status == ExportStatus.Valid.ToString()).ToList();

                var importCustomers = Mapper.Map<List<ImportCustomerRequest>>(data);

                var result = _customerService.BatchSave(new CustomerBatchRequest
                {
                    ImportCustomers = importCustomers,
                    CompanyId = model.CompanyId
                });

                return Json(new
                {
                    success = result.Success,
                    message = result.Success ? "Records imported successfully." : result.Message
                });
            }
            return Json(new
            {
                success = false,
                message = "Csv file contains not data or not valid."
            });
        }


        #region Customer Synchronization

        // get sync list data
        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Customers/GetModal")]
        public ActionResult GetModal()
        {
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedCustomers", SyncListDataFromSession);
        }

        // get sync list data

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/Customers/GetSynchronizationListData")]
        public ActionResult GetSynchronizationListData()
        {
            ViewData["States"] = GetStates();
            SetSession();
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedCustomersView", SyncListDataFromSession);
        }

        [HttpPost, Route("~/Customers/EditLinkedCustomer")]
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
        [Route("~/Customers/UnlinkCustomer")]
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
                .FirstOrDefault(x => x.LinkedQBCustomer.QbCustomerId == model.QBEntityId.Value.ToString());

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

        [HttpPost, Route("~/Customers/SaveUpdatedList")]
        public ActionResult SaveUpdatedList()
        {
            var sessionData = GetSessionDataToBeLinked();
            sessionData.CompanyId = CurrentIdentity.CompanyId.Value;
            var data = Mapper.Map<SyncCustomerRequest>(sessionData);
            var response = _customerService.UpdateSynList(data);
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

        [HttpPost, Route("~/Customers/ShowLinkedCustomers")]
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

        [HttpPost, Route("~/Customers/UpdateLinkedCustomer")]
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

        [HttpPost, Route("~/Customers/LinkUnlinkedCustomers")]
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

        #endregion

        #region Private Methods

        private void SetSession()
        {
            ViewData["States"] = GetStates();
            var SyncListDataFromSession = GetMainSessionData();

            if (SyncListDataFromSession == null || SyncListDataFromSession.LinkedCustomers.Count == 0 ||
                SyncListDataFromSession.UnlinkedSystemCustomers.Count == 0 ||
                SyncListDataFromSession.UnlinkedQBCustomers.Count == 0)
            {
                // fetch data first time on load
                SyncListDataFromSession = _customerService.CustomerSynchronizationList(CurrentIdentity.CompanyId);
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

        private List<ImportCustomerValidation> GetDataFromFile(HttpPostedFileBase httpPostedFileBase)
        {
            var data = new List<ImportCustomerValidation>();

            var datatable = DataTable.New.ReadLazy(httpPostedFileBase.InputStream);
            var emails = _customerService.GetCustomerContactEmails();
            var names = _customerService.GetCustomerNames();

            foreach (var row in datatable.Rows)
            {
                var dataItem = new ImportCustomerValidation();
                bool flag = true;

                if (string.IsNullOrEmpty(row["Email"]))
                {
                    flag = false;
                    dataItem.Reason = @"Email is empty.";
                }
                else
                {
                    if (!ValidateData.IsEmail(row["Email"]))
                    {
                        flag = false;
                        dataItem.Reason = @"Email is not valid.";
                    }
                }
                if (emails.Contains(row["Email"]))
                {
                    flag = false;
                    dataItem.Reason = @"Contact Email already exists in the app.";
                }

                if (string.IsNullOrEmpty(row["CustomerName"]))
                {
                    flag = false;
                    dataItem.Reason = @"Customer name is empty.";
                }

                if (names.Contains(row["CustomerName"]))
                {
                    flag = false;
                    dataItem.Reason = @"Customer name  already exists in the app.";
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
                if (string.IsNullOrEmpty(row["Address"]))
                {
                    flag = false;
                    dataItem.Reason = @"Address is empty.";
                }
                if (string.IsNullOrEmpty(row["City"]))
                {
                    flag = false;
                    dataItem.Reason = @"City is empty.";
                }
                if (string.IsNullOrEmpty(row["State"]))
                {
                    flag = false;
                    dataItem.Reason = @"State is empty.";
                }
                if (string.IsNullOrEmpty(row["PostalCode"]))
                {
                    flag = false;
                    dataItem.Reason = @"Postal code is empty.";
                }
                else
                {
                    if (!ValidateData.IsZipCode(row["PostalCode"]))
                    {
                        flag = false;
                        dataItem.Reason = @"PostalCode is not valid.";
                    }
                }
                if (string.IsNullOrEmpty(row["Country"]))
                {
                    flag = false;
                    dataItem.Reason = @"Country is empty.";
                }

                if (string.IsNullOrEmpty(row["Mobile"]) && string.IsNullOrEmpty(row["Telephone"]))
                {
                    flag = false;
                    dataItem.Reason = @"Mobile/ or Telephone is empty.";
                }
                else
                {
                    if (!string.IsNullOrEmpty(row["Mobile"]) && !ValidateData.IsPhoneNumber(row["Mobile"]))
                    {
                        flag = false;
                        dataItem.Reason = @"Mobile is not valid.";
                    }

                    if (!string.IsNullOrEmpty(row["Telephone"]) && !ValidateData.IsPhoneNumber(row["Telephone"]))
                    {
                        flag = false;
                        dataItem.Reason = @"Telephone is not valid.";
                    }
                }

                dataItem.Status = flag ? ExportStatus.Valid.ToString() : ExportStatus.Invalid.ToString();

                //contact
                dataItem.FirstName = row["FirstName"];
                dataItem.MiddleName = row["MiddleName"];
                dataItem.LastName = row["LastName"];
                dataItem.Telephone = row["Telephone"];
                dataItem.Mobile = row["Mobile"];
                dataItem.Email = row["Email"];

                //Customer
                dataItem.CustomerName = row["CustomerName"];
                dataItem.Address = row["Address"];
                dataItem.City = row["City"];
                dataItem.State = row["State"];
                dataItem.PostalCode = row["PostalCode"];
                dataItem.Country = row["Country"];

                data.Add(dataItem);
            }
            return data;
        }

        private IEnumerable<CompanyServiceListViewModel> GetCompanyServiceList(int id, int customerId)
        {
            var companies = _companyServiceService.GetFilteredCompanyServices(id, customerId);
            var list = companies.Select(x => new CompanyServiceListViewModel
            {
                Id = x.CompanyServiceId,
                Name = x.Name,
                Description = x.Description,
                CompanyId = x.CompanyId
            }).ToList();
            return list;
        }

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

        private List<CustomerServiceViewModel> GetDataFromSession()
        {
            var data = new List<CustomerServiceViewModel>();

            if (Session["data"] == null)
            {
                Session["data"] = data;
            }
            else
            {
                data = (List<CustomerServiceViewModel>)Session["data"];
            }
            return data;
        }

        private void SetDataToSession(List<CustomerServiceViewModel> data)
        {
            Session["data"] = data;
        }

        private int GetMaxId()
        {
            var data = new List<CustomerServiceViewModel>();

            if (Session["data"] == null)
                Session["data"] = data;
            else
                data = (List<CustomerServiceViewModel>)Session["data"];

            var max = data.Any() ? data.Max(x => x.CustomerServiceId) : 0;
            return (max == 0) ? 1 : max + 1;
        }

        private IEnumerable<ServiceRecordDetail> GetServiceRecordList(int customerId)
        {
            var list = _serviceRecordService.GetServiceRecordsByCustomer(customerId);
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
                Status = EnumUtil.GetDescription(x.Status),
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
                Description = x.Description,
                CustomerId = x.CustomerId,
                EndTime = x.EndTime,
                ServiceRecordId = x.ServiceRecordId,
                StartTime = x.StartTime,
                ServiceRecordItemId = x.ServiceRecordItemId,
                Type = EnumUtil.GetDescription(x.Type),
            }).ToList();
            return result;
        }

        private IEnumerable<CustomerListViewModel> GetCustomerList()
        {
            var customers = _customerService.GetAllCustomers(CurrentIdentity.CompanyId);

            var list = (from c in customers
                        let contact = c.ContactPersons.FirstOrDefault()
                        select new CustomerListViewModel
                        {
                            Id = c.CustomerId,
                            CompanyId = c.CompanyId,
                            CompanyName = c.Company != null ? c.Company.Name : "N/A",
                            Gateway = c.Company != null ? (int)c.Company.PaymentGatewayType : 0,
                            Email = contact != null ? contact.Email : "N/A",
                            Contact = contact != null ? string.Format("{0} {1}", contact.FirstName, contact.LastName) : "N/A",
                            Name = c.Name,
                            Telephone = contact != null ? contact.Telephone : "N/A",
                            GatewayCustomerId = c.GatewayCustomerId,
                            IsCcActive = c.IsCcActive,
                            IsCompanyDeleted = c.Company != null && c.Company.IsDeleted,
                            IsActive = !c.IsDeleted,
                            IsCreditCardActive = c.IsCcActive,
                            IsCreditCardSetup = c.CustomerJson != null ? true : false,
                            IsDeleted = c.IsDeleted
                        }).ToList();

            return list;
        }

        private IEnumerable<CustomerServiceViewModel> GetCustomerServiceList(int customerId)
        {
            if (Session["data"] != null)
            {
                var list = GetDataFromSession();
                return list.Where(x => !x.InMemoryDeleted).ToList();
            }

            var customerServices = _customerServiceService.GetCustomerServices(customerId);
            var data = (from c in customerServices
                        select new CustomerServiceViewModel
                        {
                            CustomerServiceId = c.CustomerServiceId,
                            CustomerId = c.CustomerId,
                            ServiceName = c.Name,
                            Description = c.Description,
                            Price = c.Cost,
                            IsServiceDeleted = c.CompanyService.IsDeleted,
                            CompanyServiceId = c.CompanyServiceId
                        }).ToList();

            //Set data in-memory for manipulation
            SetDataToSession(data);
            return data;
        }

        private IEnumerable<CompanyService> GetCompanyServices(int companyId)
        {
            return _companyServiceService.GetAllCompanyServices(companyId);
        }

        private CompanyService GetCompanyService(int companyServiceId)
        {
            return _companyServiceService.GetCompanyService(companyServiceId);
        }

        private void PopulateViews()
        {
            var list = GetOrganizations();
            ViewData["StateList"] = GetStates();
            ViewData["Organizations"] = list;
        }

        private void PopulateCompanyServices(int companyId)
        {
            var services = GetCompanyServices(companyId).Where(x => !x.IsDeleted);
            var list = new List<SelectListItem> { new SelectListItem { Text = "--Select--", Value = "" } };
            list.AddRange(services.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyServiceId.ToString(CultureInfo.InvariantCulture)
            }));
            ViewData["CompanyServices"] = list;
        }

        private List<SelectListItem> GetOrganizations()
        {
            var companies = _companyService.GetOranizationList().Where(x => !x.IsDeleted);
            var list = new List<SelectListItem> { new SelectListItem { Text = "--Select--", Value = "" } };
            list.AddRange(companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyId.ToString(CultureInfo.InvariantCulture)
            }));
            return list;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        #region Private Customers Methods 

        // session list ot be displayed
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

        #endregion

        #endregion
    }
}
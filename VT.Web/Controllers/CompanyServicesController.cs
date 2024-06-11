using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using VT.Common.Utils;
using VT.Data;
using VT.Services.Interfaces;
using VT.Web.Models;
using VT.Services.DTOs;
using System.Globalization;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.Web.Models.QBCustomers;
using AutoMapper;
using System;

namespace VT.Web.Controllers
{
    public class CompanyServicesController : BaseController
    {
        #region Fields

        private readonly ICompanyServiceService _companyServiceService;
        private readonly ICompanyService _companyService;
        private readonly IServiceRecordService _serviceRecordService;
        private readonly IServiceRecordItemService _serviceRecordItemService;

        #endregion

        #region Constructor

        public CompanyServicesController(ICompanyServiceService companyServiceService,
            ICompanyService companyService, IServiceRecordService serviceRecordService,
            IServiceRecordItemService serviceRecordItemService)
        {
            _companyServiceService = companyServiceService;
            _companyService = companyService;
            _serviceRecordService = serviceRecordService;
            _serviceRecordItemService = serviceRecordItemService;
        }

        #endregion

        #region Action Method(s)

        [HttpGet]
        [Route("~/CompanyServices")]
        public ActionResult Index()
        {
            PopulateViews();

            bool canSee = false;
            var message = string.Empty;

            if (CurrentIdentity != null)
            {
                canSee = CurrentIdentity.CompanyId == null;
                if (CurrentIdentity.CompanyId != null)
                {
                    var company = _companyService.GetCompany(CurrentIdentity.CompanyId.Value);
                    ViewBag.ImageUrl = company.ImageName;
                    if (company != null)
                    {
                        canSee = !string.IsNullOrEmpty(company.GatewayCustomerId) &&
                                 !string.IsNullOrEmpty(company.MerchantAccountId);

                        if (string.IsNullOrEmpty(company.GatewayCustomerId) &&
                            string.IsNullOrEmpty(company.MerchantAccountId))
                        {
                            message = "Your Merchant Account and Credit Card Account is not setup. Please setup these accounts in settings.";
                        }
                        else if (string.IsNullOrEmpty(company.GatewayCustomerId))
                        {
                            message = "Your Credit Card Account is not setup. Please setup these accounts in settings.";
                        }
                        else if (string.IsNullOrEmpty(company.MerchantAccountId))
                        {
                            message = "Your Merchant Account is not setup. Please setup these accounts in settings.";
                        }
                    }
                }
            }
            ViewBag.CanSee = canSee;
            ViewBag.Message = message;

            return View();
        }

        [HttpPost]
        [Route("~/CompanyServices/DeleteCompanyServices")]
        public ActionResult DeleteCompanyServices(DeleteUsersViewModel model)
        {
            var response = _companyServiceService.DeleteCompanyServices(model.Ids);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // activate service
        [HttpPost]
        [Route("~/CompanyServices/ActivateCompanyService/{id}")]
        public ActionResult ActivateCompanyService(int id)
        {
            var response = _companyServiceService.ActivateService(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // deactivate service
        [HttpPost]
        [Route("~/CompanyServices/DeactivateCompanyService/{id}")]
        public ActionResult DeactivateCompanyService(int id)
        {
            var response = _companyServiceService.DeactivateService(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/CompanyServices/UnDeleteCompanyServices/{id}")]
        public ActionResult UnDeleteCompanyServices(int id)
        {
            var response = _companyServiceService.UndeleteCompanyService(id);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/CompanyServices/GetCompanyService/{id}")]
        public ActionResult GetCompanyService(int id)
        {
            var companyService = _companyServiceService.GetCompanyService(id);
            if (companyService == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Company service does not exist in the database."
                });
            }
            var model = new CompanyServiceListViewModel
            {
                Id = companyService.CompanyServiceId,
                Name = companyService.Name,
                Description = companyService.Description,
                CompanyId = companyService.CompanyId,
                IsActive = !companyService.IsDeleted,
                IsCompanyDeleted = companyService.Company != null && companyService.Company.IsDeleted
            };
            return Json(new
            {
                success = true,
                message = string.Empty,
                cs = model
            });
        }

        [HttpPost]
        [Route("~/CompanyServices/GetCompanyServiceDetail/{id}")]
        public ActionResult GetCompanyServiceDetail(int id)
        {
            var companyService = _companyServiceService.GetCompanyService(id);
            if (companyService == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Company service does not exist in the database."
                });
            }
            var model = new CompanyServiceListViewModel
            {
                Id = companyService.CompanyServiceId,
                Name = companyService.Name,
                Description = companyService.Description,
                CompanyId = companyService.CompanyId,
                IsActive = companyService.IsDeleted
            };
            return PartialView("CompanyServiceDetail", model);
        }

        [HttpPost]
        [Route("~/CompanyServices/CompanyServiceList")]
        public ActionResult CompanyServiceList([DataSourceRequest] DataSourceRequest request, string additionalInfo)
        {
            bool? filter = null;
            if (additionalInfo == "Active") filter = true;
            if (additionalInfo == "Not Active") filter = false;

            var data = filter != null ? GetCompanyServiceList().Where(x => x.IsActive.Value == filter).ToList() : GetCompanyServiceList().ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/CompanyServices/SaveCompanyService")]
        public ActionResult SaveCompanyService(SaveCompanyServiceModel model)
        {
            var response = _companyServiceService.SaveCompanyService(new SaveCompanyServiceRequest
            {
                CompanyServiceId = model.CompanyServiceId,
                CompanyId = model.CompanyId != null ? model.CompanyId.Value : 0, //TODO: CompanyId can be nullable here            
                Name = model.Name,
                Description = model.Description
            });

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        [HttpPost]
        [Route("~/CompanyServices/ServiceRecords/{id}")]
        public ActionResult ServiceRecords(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordList(id).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/CompanyServices/ServiceRecordItems/{id}")]
        public ActionResult ServiceRecordItems(int id, int companyServiceId, [DataSourceRequest] DataSourceRequest request)
        {
            var data = GetServiceRecordItemList(id, companyServiceId).ToList();
            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region Services Methods

        // get sync list data
        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/CompanyServices/GetModal")]
        public ActionResult GetModal()
        {
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedServices", SyncListDataFromSession);
        }

        [Authorize(Roles = "CompanyAdmin")]
        [Route("~/CompanyServices/GetSynchronizationListData")]
        public ActionResult GetSynchronizationListData()
        {
            SetSession();
            var SyncListDataFromSession = GetSessionData();
            return PartialView("LinkedServicesView", SyncListDataFromSession);
        }

        [HttpPost]
        [Route("~/CompanyServices/UnlinkService")]
        public ActionResult UnlinkService(UnlinkServiceModel model)
        {
            var items = GetSessionDataToBeLinked();
            var ids = new List<int>();

            if (model.EntityId != null && model.QBEntityId != null)
            {
                // unlink actual linked customer
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

            var updateListData = GetSessionData();
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

        [HttpPost, Route("~/CompanyServices/EditLinkedService")]
        public ActionResult EditLinkedService(ServiceIdsModel model)
        {
            var data = GetSessionData();

            var serviceObj = new LinkedService();

            serviceObj = data.LinkedServices.FirstOrDefault(x => x.LinkedSystemService.ServiceId == int.Parse(model.ServiceId));
            if (serviceObj == null)
            {
                // quickbooks service which is just moved to linked services TAB is being edited
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
                customer = model1,
                systemcustmer = systemService,
                qbCustomer = qbObj
            });
        }

        [HttpPost, Route("~/CompanyServices/SaveUpdatedList")]
        public ActionResult SaveUpdatedList()
        {
            var sessionData = GetSessionDataToBeLinked();
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

        [HttpPost, Route("~/CompanyServices/ShowLinkedServices")]
        public ActionResult ShowLinkedCustomers(LinkServiceModel model)
        {
            var data = GetSessionData();

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
                        customer = model1,
                        systemcustmer = systemCustomer,
                        qbCustomer = qbObj
                    });
                }
                else
                {
                    // integrate system service
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
                        customer = model1,
                        systemcustmer = systemCustomer,
                        qbCustomer = qbObj
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
                    customer = model1,
                    systemcustmer = systemService,
                    qbCustomer = qbObj
                });
            }
        }

        [HttpPost, Route("~/CompanyServices/UpdateLinkedService")]
        public ActionResult UpdateLinkedService(SystemServiceModel model)
        {
            try
            {
                var data = GetSessionDataToBeLinked();
                var IsRecordExists = data.ServicesEdited.FirstOrDefault(x => x.ServiceId == model.ServiceId);

                if (IsRecordExists != null)
                {
                    data.ServicesEdited.Remove(IsRecordExists);
                }
                data.ServicesEdited.Add(model);
                Session["LinkedServiceOperations"] = data;

                // update list to be shown
                var updateListData = GetSessionData();

                // service id==null means quickbooks service is being edited
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

        [HttpPost, Route("~/CompanyServices/LinkUnlinkedServices")]
        public ActionResult LinkUnlinkedServices(SystemServiceModel1 model)
        {
            var items = GetSessionDataToBeLinked();
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
                // link system service
                items.LinkedSystemServices.Add(model.ServiceId1.Value);
            }
            if (model.ServiceId1 == null && model.QBServiceId1 != null)
            {
                // link qb service
                items.LinkedQBServices.Add(int.Parse(model.QBServiceId1));
            }

            Session["LinkedServiceOperations"] = items;

            var updateListData = GetSessionData();

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

        #region Private Methods

        private IEnumerable<ServiceRecordDetail> GetServiceRecordList(int id)
        {
            var list = _serviceRecordService.GetServiceRecordsByCompanyService(id);
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
                Status = EnumUtil.GetDescription(x.Status),
                TotalAmount = x.TotalAmount
            }).ToList();
            return result;
        }

        private IEnumerable<ServiceRecordItemDetail> GetServiceRecordItemList(int id, int? companyServiceId)
        {
            var list = _serviceRecordItemService.GetRecordItems(id);
            var result = list.Select(x => new ServiceRecordItemDetail
            {
                CompanyServiceId = x.CompanyServiceId != null ? x.CompanyServiceId.Value : 0,
                CostOfService = x.CostOfService,
                ServiceName = x.ServiceName,
                CustomerId = x.CustomerId,
                EndTime = x.EndTime,
                Description = x.Description,
                ServiceRecordId = x.ServiceRecordId,
                StartTime = x.StartTime,
                ServiceRecordItemId = x.ServiceRecordItemId,
                Type = EnumUtil.GetDescription(x.Type),
            }).ToList();
            return result;
        }

        private IEnumerable<CompanyServiceListViewModel> GetCompanyServiceList()
        {
            var companies = _companyServiceService.GetAllCompanyServices(CurrentIdentity.CompanyId);
            var list = companies.Select(x => new CompanyServiceListViewModel
            {
                Id = x.CompanyServiceId,
                CompanyName = x.Company.Name,
                Name = x.Name,
                Description = x.Description,
                CompanyId = x.CompanyId,
                IsActive = !x.IsDeleted,
                IsCompanyDeleted = x.Company.IsDeleted,
                IsDeleted = x.IsDeleted
            }).ToList();
            return list;
        }

        private void PopulateViews()
        {
            var companies = _companyService.GetOranizationList().Where(x => !x.IsDeleted);
            var list = new List<SelectListItem> { new SelectListItem { Text = "--Select--", Value = "" } };
            list.AddRange(companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyId.ToString(CultureInfo.InvariantCulture)
            }));
            ViewData["Organizations"] = list;
        }


        #region Private Services Methods

        private void SetSession()
        {
            var SyncListDataFromSession = GetMainSessionData();

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
        private ServiceSynchronizationList GetSessionData()
        {
            if (Session["SyncServiceListData"] == null) return new ServiceSynchronizationList();

            var items = (ServiceSynchronizationList)Session["SyncServiceListData"];
            return items;
        }

        // main session list
        private ServiceSynchronizationList GetMainSessionData()
        {
            if (Session["MainSyncServiceListData"] == null) return new ServiceSynchronizationList();
            var items = (ServiceSynchronizationList)Session["MainSyncServiceListData"];
            return items;
        }

        // session list of service layer
        private SyncServiceListModel GetSessionDataToBeLinked()
        {
            if (Session["LinkedServiceOperations"] == null) return new SyncServiceListModel();

            var items = (SyncServiceListModel)Session["LinkedServiceOperations"];
            return items;
        }

        #endregion

        #endregion
    }
}
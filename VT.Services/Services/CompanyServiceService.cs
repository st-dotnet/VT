using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using VT.Data.Context;
using VT.Data.Entities;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using Newtonsoft.Json;
using VT.Common.Utils;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.DTOs.CompanyServices;

namespace VT.Services.Services
{
    public class CompanyServiceService : ICompanyServiceService
    {
        #region Field(s)

        private readonly IVerifyTechContext _context;
        private readonly IQBCompanyService _qbCompanyServices;

        #endregion

        #region Constructor

        public CompanyServiceService(IVerifyTechContext context, IQBCompanyService qbCompanyServices)
        {
            _context = context;
            _qbCompanyServices = qbCompanyServices;
        }

        #endregion

        #region Interface implementation

        public CompanySaveResponse Save(CompanySaveRequest request)
        {
            var response = new CompanySaveResponse();

            if (request.OrganizationId > 0) //Edit
            {
                var company = _context.Companies.FirstOrDefault(x => x.CompanyId == request.OrganizationId);

                if (company == null)
                {
                    response.Message = "This organization does not exist";
                    return response;
                }

                company.Name = request.Name;
                _context.SaveChanges();
            }
            else //Add
            {
                _context.Companies.Add(new Company
                {
                    Name = request.Name
                });
                _context.SaveChanges();
            }
            response.Success = true;
            return response;
        }
        // deactivate service
        public BaseResponse DeactivateService(int id)
        {
            var response = new BaseResponse();
            try
            {
                var service = _context.CompanyServices.FirstOrDefault(x => x.CompanyServiceId == id);
                if (service == null)
                {
                    response.Success = false;
                    response.Message = "Service doesn't exists.";
                }
                // deactivate service
                service.IsDeleted = true;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Service has been deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }
        // activate Customer
        public BaseResponse ActivateService(int id)
        {
            var response = new BaseResponse();
            try
            {
                var service = _context.CompanyServices.FirstOrDefault(x => x.CompanyServiceId == id);
                if (service == null)
                {
                    response.Success = false;
                    response.Message = "Service doesn't exists.";
                    return response;
                }
                // activate service
                service.IsDeleted = false;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Service has been activated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public Company GetCompany(int companyId)
        {
            return _context.Companies.FirstOrDefault(x => x.CompanyId == companyId);
        }

        public IList<Company> GetAllCompanies()
        {
            return _context.Companies.ToList();
        }

        public BaseResponse DeleteCompany(int companyId)
        {
            var response = new BaseResponse();

            var company = _context.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "This organization does not exist";
                return response;
            }

            _context.Companies.Remove(company);
            _context.SaveChanges();
            response.Success = true;
            return response;
        }

        public BaseResponse DeleteCompanyServices(List<int> ids)
        {
            var response = new BaseResponse();
            try
            {
                var records = _context.CustomerServices.Where(x => ids.Contains(x.CompanyServiceId)).ToList();
                if (records.Count == 0)
                {
                    var companyServices =
                        _context.CompanyServices.Where(x => ids.Contains(x.CompanyServiceId)).ToList();

                    foreach (var companyService in companyServices)
                    {
                        companyService.IsDeleted = true;
                        companyService.Name = string.Format("{0}_DELETED_{1}", companyService.Name,
                            DateTime.UtcNow.ToString("O"));
                    }
                    _context.SaveChanges();
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    var services = new List<string>();

                    var message = new StringBuilder("No services have been deleted because the following services are in use by some customers. Please issue a request to delete services that are not in use by anyone. <br/><br/>");
                    foreach (var customerService in records.Where(customerService =>
                    !services.Contains(customerService.CompanyService.Name)))
                        services.Add(customerService.CompanyService.Name);

                    foreach (var service in services)
                        message.Append(service + "<br/>");

                    response.Message = message.ToString();
                }
            }
            catch (Exception exception)
            {
                response.Message = exception.ToString();
            }
            return response;
        }

        public Data.Entities.CompanyService GetCompanyService(int companyServiceId)
        {
            return _context.CompanyServices.Include(x => x.Company).FirstOrDefault(x => x.CompanyServiceId == companyServiceId);
        }

        public BaseResponse UndeleteCompanyService(int companyServiceId)
        {
            var response = new BaseResponse();
            try
            {
                var companyService = _context.CompanyServices.FirstOrDefault(x => x.CompanyServiceId == companyServiceId);
                if (companyService == null)
                {
                    response.Message = "Company service does not exist in the database";
                }
                companyService.Name = companyService.Name.Split('_')[0];
                companyService.IsDeleted = false;
                _context.SaveChanges();
                response.Message = "Company service has been successfully activated.";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = string.Format("Some error has occure : {0}", ex.ToString());
            }
            return response;
        }

        public IList<Data.Entities.CompanyService> GetAllCompanyServices(int? companyId = null)
        {
            return companyId == null
                ? _context.CompanyServices.Include(x => x.Company).ToList()
                : _context.CompanyServices.Include(x => x.Company)
                .Where(x => x.CompanyId == companyId).ToList();
        }

        public SaveCompanyServiceResponse SaveCompanyService(SaveCompanyServiceRequest request)
        {
            var response = new SaveCompanyServiceResponse();
            var baseResponse = new BaseResponse();

            if (request.CompanyServiceId > 0) // Edit
            {
                var companyService = _context.CompanyServices.FirstOrDefault(x => !x.IsDeleted && x.CompanyServiceId == request.CompanyServiceId);
                if (companyService == null)
                {
                    response.Message = "This service does not exist in the database";
                    return response;
                }
                else
                {
                    companyService.CompanyId = request.CompanyId;
                    companyService.Name = request.Name;
                    companyService.Description = request.Description;
                }
                _context.SaveChanges();
                response.Success = true;
            }
            else
            {
                var companyService = new Data.Entities.CompanyService
                {
                    CompanyId = request.CompanyId,
                    Description = request.Description,
                    Name = request.Name
                };

                if (string.IsNullOrEmpty(companyService.QuickbookServiceId))
                {
                    baseResponse = CreateServiceOnQB(request);
                    if (baseResponse.Success)
                    {
                        companyService.QuickbookServiceId = baseResponse.QBEntityId;
                        _context.CompanyServices.Add(companyService);
                        _context.SaveChanges();

                        response.Message = baseResponse.Message;
                        response.Success = true;
                    }
                    else
                    {
                        response.Success = baseResponse.Success;
                        response.Message = baseResponse.Message;
                    }
                }
            }
            return response;
        }

        public IList<Data.Entities.CompanyService> GetFilteredCompanyServices(int companyId, int customerId)
        {
            var companyServiceIds = _context.CustomerServices
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.CompanyServiceId).ToList();
            return _context.CompanyServices
                .Where(x => !companyServiceIds.Contains(x.CompanyServiceId) && x.CompanyId == companyId).ToList();
        }

        #region Synchronization Methods

        public ServiceSynchronizationList ServiceSynchronizationList(int? companyId)
        {
            var synList = new ServiceSynchronizationList();
            var unlinkedSystemServices = new List<SystemServiceModel>();
            var unlinkedQBServices = new List<QBServiceModel>();
            try
            {
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == companyId.Value);
                var dbServices = _context.CompanyServices.Where(x => x.CompanyId == companyId.Value).ToList();
                var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                var qbServices = _qbCompanyServices.GetAllServices(authorizationToken);
                if (!qbServices.Success)
                {
                    synList.Success = false;
                    synList.Message = qbServices.Message;
                    return synList;
                }
                foreach (var qbService in qbServices.QueryResponse.Item.
                    Where(x => x.Type == "Service").ToList())
                {
                    var linkedService = new LinkedService();
                    var unlinkedService = new UnlinkedService();
                    var ServiceExists = dbServices
                        .FirstOrDefault(x => x.QuickbookServiceId == qbService.Id);
                    if (ServiceExists != null)
                    {
                        var systemService = GetSystemService(ServiceExists, companyId);
                        var QbService = GetQBService(qbService, companyId.Value, ServiceExists.CompanyServiceId);
                        bool isMatch = IsServiceMatched(systemService, QbService);
                        systemService.IsMatch = isMatch;
                        QbService.IsMatch = isMatch;
                        linkedService.LinkedSystemService = systemService;
                        linkedService.LinkedQBService = QbService;
                        synList.LinkedServices.Add(linkedService);
                    }
                    else
                    {
                        var unlinkedQBService = GetQBService(qbService, companyId.Value, null);
                        synList.UnlinkedQBServices.Add(unlinkedQBService);
                    }
                }
                foreach (var dbService in dbServices)
                {
                    if (dbService.QuickbookServiceId == null)
                    {
                        var systemCustomer = GetSystemService(dbService, companyId);
                        synList.UnlinkedSystemServices.Add(systemCustomer);
                    }
                }
                synList.Success = true;
            }
            catch (Exception ex)
            {
                synList.Success = false;
                synList.Message = ex.Message.ToString();
            }
            return synList;
        }

        #endregion


        public BaseResponse UpdateSynList(SyncServicesRequest request)
        {
            var response = new BaseResponse();
            var updateResponse = new BaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                var dbServices = _context.CompanyServices.ToList();
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                #region Unlinked Actual Linked Services

                if (request.UnLinkActualLinkedServicesIds.Count != 0)
                {
                    response = UnLinkActualLinkedServices(dbServices, request.UnLinkActualLinkedServicesIds);
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                }

                #endregion

                #region Linked System Services
                if (request.LinkedSystemServices.Count != 0)
                {
                    response = LinkedSystemServices(dbServices, request.LinkedSystemServices, request.ServicesEdited);
                    if (!response.Success)
                    {
                        sb.Append("Couldn't save this servie on Quikcbooks " + response.Message + " ");
                    }
                }

                #endregion

                #region Linked Quickbooks Services
                if (request.LinkedQBServices.Count != 0)
                {
                    response = LinkedQBServices(request.LinkedQBServices, qbSettings, request);
                    dbServices = _context.CompanyServices.ToList();
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                    updateResponse = UpdateServicesForQuickbooks(request.ServicesEdited, qbSettings, request, dbServices);
                }

                #endregion

                #region Update Serices on Quickbooks

                if (request.ServicesEdited.Count != 0 && request.LinkedQBServices.Count == 0)
                {
                    updateResponse = UpdateCustomerOnQB(request.ServicesEdited, qbSettings, request, dbServices);
                    var dbSer = _context.CompanyServices.ToList();
                    if (updateResponse.Success)
                        foreach (var service in request.ServicesEdited)
                        {
                            var dbService = dbSer.FirstOrDefault(x => x.CompanyServiceId == service.ServiceId);

                            dbService.Name = service.Name;
                            dbService.Description = service.Description;
                            _context.SaveChanges();
                        }
                }

                #endregion

                response.Success = true;
                response.Message = "Services have been synchronized successfully." + sb.ToString();
                if (!qbSettings.ServiceSettings)
                {
                    qbSettings.ServiceSettings = true;
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

        #region Private Methods

        private BaseResponse UpdateCustomerOnQB(List<SystemServiceModel> request, QuickbookSettings qbSettings, SyncServicesRequest request1, List<Data.Entities.CompanyService> dbservices)
        {
            var response = new BaseResponse();
            var response1 = new GetQBServiceResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestService in request)
                {
                    var dbService = dbservices.FirstOrDefault(x => x.CompanyServiceId == requestService.ServiceId);
                    if (request1.LinkedSystemServices != null || request1.LinkedSystemServices.Count != 0)
                        if (!request1.LinkedSystemServices.Contains(requestService.ServiceId.Value))
                        {
                            var service = new UpdateServiceRequest();
                            var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                            var qbService = _qbCompanyServices.GetQBService(requestService.QBServiceId != null ? requestService.QBServiceId : requestService.ServiceId.ToString(), authorizationToken);

                            service.sparse = true;
                            service.IncomeAccountRef = new DTOs.QBEntitiesRequestResponse.IncomeAccountRef
                            {
                                name = "Sales of Product Income",
                                value = "79"
                            };
                            service.ExpenseAccountRef = new DTOs.QBEntitiesRequestResponse.ExpenseAccountRef
                            {
                                name = "Cost of Goods Sold",
                                value = "80"
                            };
                            service.Id = requestService.QBServiceId;
                            service.SyncToken = qbService.Item.SyncToken;
                            service.Name = requestService.Name;
                            service.Description = requestService.Description;
                            var jsonService = JsonConvert.SerializeObject(service);
                            var serviceUpdationResponse = _qbCompanyServices.UpdateService(jsonService, authorizationToken);
                            if (serviceUpdationResponse.Success)
                                response1 = XmlUtil.Deserialize<GetQBServiceResponse>(serviceUpdationResponse.ResponseValue);
                            dbService.QuickbookServiceId = response1.Item.Id;
                            _context.SaveChanges();

                            if (!serviceUpdationResponse.Success)
                            {
                                response.Success = false;
                                response.Message = serviceUpdationResponse.Message;
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

        private BaseResponse UnLinkActualLinkedServices(List<Data.Entities.CompanyService> dbServices, List<int> unlinkedIds)
        {
            var response = new BaseResponse();
            try
            {
                foreach (var serviceId in unlinkedIds)
                {
                    var service = dbServices.FirstOrDefault(x => x.CompanyServiceId == serviceId);
                    if (service != null)
                    {
                        service.QuickbookServiceId = null;
                        _context.SaveChanges();
                    }
                }
                response.Success = true;
                response.Message = "Services hasve been unlinked successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse LinkedSystemServices(List<Data.Entities.CompanyService> dbServices, List<int> servicesToBeLinkedIds, List<SystemServiceModel> servicesEdited)
        {
            var response = new BaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                foreach (var serviceId in servicesToBeLinkedIds)
                {
                    var dbService = dbServices.FirstOrDefault(x => x.CompanyServiceId == serviceId);
                    var checkService = servicesEdited.FirstOrDefault(x => x.ServiceId == serviceId);

                    if (dbService != null)
                    {
                        var request = new SaveCompanyServiceRequest
                        {
                            Name = checkService != null ? checkService.Name : dbService.Name,
                            Description = checkService != null ? checkService.Description : dbService.Description
                        };
                        response = CreateServiceOnQB(request);
                        if (response.Success)
                        {
                            dbService.QuickbookServiceId = response.QBEntityId;
                            _context.SaveChanges();
                        }
                        else
                        {
                            sb.Append("<b>" + request.Name + "</b> " + response.Message + "<br/> ");
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

        private BaseResponse CreateServiceOnQB(SaveCompanyServiceRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                if (qbSettings == null) return response;

                var serviceRequest = new CreateQBServiceRequest();
                serviceRequest.Name = request.Name;
                serviceRequest.Description = request.Description;
                serviceRequest.Type = "Service";
                serviceRequest.IncomeAccountRef = new DTOs.QBEntitiesRequestResponse.IncomeAccountRef
                {
                    name = "Sales of Product Income",
                    value = "79"
                };
                serviceRequest.ExpenseAccountRef = new DTOs.QBEntitiesRequestResponse.ExpenseAccountRef
                {
                    name = "Cost of Goods Sold",
                    value = "80"
                };
                var jsonService = JsonConvert.SerializeObject(serviceRequest);
                var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                var serviceCreationResponse = _qbCompanyServices.CreateQBCompanyService(jsonService, authorizationTokenHeader);
                if (!serviceCreationResponse.Success)
                {
                    response.Success = false;
                    response.Message = serviceCreationResponse.Message;
                    return response;
                }
                var qbService = XmlUtil.Deserialize<GetCompanyServiceResponse>(serviceCreationResponse.ResponseValue);
                response.Success = true;
                response.Message = "Service created successfully.";
                response.QBEntityId = qbService.Item.Id;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }

            return response;
        }

        private BaseResponse LinkedQBServices(List<int> linkedQBServices, QuickbookSettings qbSettings, SyncServicesRequest req)
        {
            var response = new BaseResponse();
            try
            {
                var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                var qbServices = _qbCompanyServices.GetAllServices(authorizationTokenHeader);

                foreach (var qbServiceId in linkedQBServices)
                {
                    var qbService = qbServices.QueryResponse.Item.FirstOrDefault(x => x.Id == qbServiceId.ToString());
                    var serviceExists = req.ServicesEdited.FirstOrDefault(x => x.QBServiceId == qbServiceId.ToString());
                    if (qbService != null)
                    {
                        var service = new Data.Entities.CompanyService
                        {
                            Name = serviceExists != null ? serviceExists.Name : qbService.Name,
                            Description = serviceExists != null ? serviceExists.Description : qbService.Description,
                            CompanyId = qbSettings.CompanyId,
                            QuickbookServiceId = serviceExists != null ? serviceExists.QBServiceId : qbService.Id.ToString()
                        };
                        _context.CompanyServices.Add(service);
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

        private BaseResponse UpdateServicesForQuickbooks(List<SystemServiceModel> request, QuickbookSettings qbSettings, SyncServicesRequest request1, List<Data.Entities.CompanyService> dbServices)
        {
            var response = new BaseResponse();
            var response1 = new GetQBServiceResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestService in request)
                {
                    var dbService = dbServices.FirstOrDefault(x => x.QuickbookServiceId == requestService.QBServiceId);
                    var service = new UpdateServiceRequest();
                    var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                    var qbService = _qbCompanyServices.GetQBService(requestService.QBServiceId != null ? requestService.QBServiceId : requestService.ServiceId.ToString(), authorizationToken);
                    service.sparse = true;
                    service.IncomeAccountRef = new DTOs.QBEntitiesRequestResponse.IncomeAccountRef
                    {
                        name = "Sales of Product Income",
                        value = "79"
                    };
                    service.ExpenseAccountRef = new DTOs.QBEntitiesRequestResponse.ExpenseAccountRef
                    {
                        name = "Cost of Goods Sold",
                        value = "80"
                    };
                    service.Id = requestService.QBServiceId;
                    service.SyncToken = qbService.Item.SyncToken;
                    service.Name = requestService.Name;
                    service.Description = requestService.Description;
                    var jsonService = JsonConvert.SerializeObject(service);
                    var serviceUpdationResponse = _qbCompanyServices.UpdateService(jsonService, authorizationToken);
                    if (serviceUpdationResponse.Success)
                    {
                        var obj = XmlUtil.Deserialize<GetQBServiceResponse>(serviceUpdationResponse.ResponseValue);
                        dbService.QuickbookServiceId = obj.Item.Id;
                        _context.SaveChanges();
                    }
                    if (!serviceUpdationResponse.Success)
                    {
                        response.Success = false;
                        response.Message = serviceUpdationResponse.Message;
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

        private SystemServiceModel GetSystemService(Data.Entities.CompanyService dbService, int? companyId)
        {
            return new SystemServiceModel
            {
                Description = dbService.Description,
                Name = dbService.Name,
                QBServiceId = dbService.QuickbookServiceId,
                ServiceId = dbService.CompanyServiceId,
                CompanyId = companyId.Value
            };
        }

        private QBServiceModel GetQBService(Item qbService, int? companyId, int? serviceId)
        {
            return new QBServiceModel
            {
                CompanyId = companyId.Value,
                Description = qbService.Description,
                Name = qbService.Name,
                QBServiceId = qbService.Id,
            };
        }

        private bool IsServiceMatched(SystemServiceModel systemService, QBServiceModel qbServiceModel)
        {
            var qbName = qbServiceModel.Name.TrimEnd();
            var syName = systemService.Name.TrimEnd();

            bool isMatch =
                qbName == syName &&
                systemService.Description == qbServiceModel.Description;
            return isMatch;
        }

        #endregion
    }
}

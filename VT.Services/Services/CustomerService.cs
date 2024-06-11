using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using VT.Common;
using VT.Common.Utils;
using VT.Data.Context;
using VT.Data.Entities;
using VT.QuickBooks.DTOs.Customers;
using VT.QuickBooks.Interfaces;
using VT.Services.DTOs;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.Services.Interfaces;

namespace VT.Services.Services
{
    public class CustomerService : ICustomerService
    {
        #region Field(s)

        private readonly IVerifyTechContext _context;
        private readonly ICustomer _qbCustomerServices;

        #endregion

        #region Constructor

        public CustomerService(IVerifyTechContext context, ICustomer qbCustomerServices)
        {
            _context = context;
            _qbCustomerServices = qbCustomerServices;
        }

        #endregion

        #region Interface implementation

        public IList<Data.Entities.Customer> GetAllCustomers()
        {
            return _context.Customers.Include(x => x.Company).ToList();
        }

        public IList<Data.Entities.Customer> GetAllCustomers(int? companyId)
        {
            var query = _context.Customers.Include(x => x.Company);
            return companyId == null
                ? query.ToList()
                : query.Where(x => x.CompanyId == companyId).ToList();
        }

        public CustomerSaveResponse SaveQBCustomerId(string qbCustomerId, int customerId)
        {
            var response = new CustomerSaveResponse();
            try
            {
                var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                customer.QuickbookCustomerId = qbCustomerId;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public IList<Data.Entities.Customer> GetAllCustomersForUser(int userId)
        {
            var customers = new List<Data.Entities.Customer>();
            var user = _context.CompanyWorkers.FirstOrDefault(x => x.CompanyWorkerId == userId);

            if (user != null && user.CompanyId != null)
            {
                customers = _context.Customers.Where(x => x.CompanyId == user.CompanyId).ToList();
            }

            return customers;
        }
        // activate customer credit card
        public BaseResponse ActivateCustomerCreditCard(int customerId)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                    return response;
                }

                // inactivate customer credit card
                customer.IsCcActive = true;
                _context.SaveChanges();
                response.Success = true;
                response.Message = "Customer has been activated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message.ToString();
            }
            return response;
        }

        // deactivate customer credit card
        public BaseResponse DeactivateCustomerCreditCard(int customerId)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                    return response;
                }
                // inactivate customer credit card
                customer.IsCcActive = false;
                _context.SaveChanges();
                response.Success = true;
                response.Message = "Customer has been deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message.ToString();
            }
            return response;
        }

        // deactivate Customer
        public BaseResponse DeactivateCustomer(int id)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == id);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                }
                // deactivate customer
                customer.IsDeleted = true;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Customer has been deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        // activate Customer
        public BaseResponse ActivateCustomer(int id)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == id);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                    return response;
                }
                // activate customer
                customer.IsDeleted = false;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Customer has been activated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public IList<Data.Entities.Customer> GetUserCustomers(int companyWorkerId)
        {
            var customers =
                _context.CompanyWorkerCustomers.Include(x => x.Customer).Where(x => x.CompanyWorkerId == companyWorkerId)
                    .OrderBy(x => x.CustomerOrder)
                    .Select(x => x.Customer)
                    .ToList();

            return customers;
        }

        public Data.Entities.Customer GetCustomer(int customerId)
        {
            return _context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
        }

        public Data.Entities.Customer GetCustomer(string token)
        {
            return _context.Customers.FirstOrDefault(x => x.Token == token);
        }

        public CustomerSaveResponse SaveCustomer(CustomerSaveRequest request)
        {
            var response = new CustomerSaveResponse();
            var baseResponse = new BaseResponse();

            Data.Entities.Customer customer = null;
            if (request.EditCompanyId > 0) //Edit
            {
                customer = _context.Customers
                    .Include(x => x.Addresses)
                    .Include(x => x.ContactPersons)
                    .FirstOrDefault(x => x.CustomerId == request.CustomerId);

                if (customer == null)
                {
                    response.Message = "This customer does not exist";
                    return response;
                }
                customer.CompanyId = request.EditCompanyId;
                customer.Name = request.Name;

                var address = customer.Addresses.FirstOrDefault();
                var contactPerson = customer.ContactPersons.FirstOrDefault();

                //Address
                if (address == null)
                {
                    AddAddress(customer, request);
                }
                else
                {
                    address.StreetAddress = request.Address;
                    address.City = request.City;
                    address.Territory = request.State;
                    address.Country = request.Country;
                    address.PostalCode = request.PostalCode;
                }

                //Contact Person
                if (contactPerson == null)
                {
                    AddContactPerson(customer, request);
                }
                else
                {
                    contactPerson.Email = request.ContactEmail;
                    contactPerson.FirstName = request.ContactFirstName;
                    contactPerson.LastName = request.ContactLastName;
                    contactPerson.Telephone = request.ContactTelephone;
                    contactPerson.Mobile = request.ContactMobile;
                    contactPerson.MiddleName = request.ContactMiddleName;
                }
            }
            else //Add
            {
                customer = new Data.Entities.Customer
                {
                    Name = request.Name,
                    CompanyId = request.CompanyId,
                };
                AddAddress(customer, request);
                AddContactPerson(customer, request);
                _context.Customers.Add(customer);
            }

            customer.IsCcActive = request.IsCcActive;

            // save customer on Quick books
            if (string.IsNullOrEmpty(customer.QuickbookCustomerId))
            {
                baseResponse = SaveCustomerOnQB(request);
                if (baseResponse.Success) customer.QuickbookCustomerId = baseResponse.QBEntityId;
            }
            _context.SaveChanges();
            response.Customer = customer;
            response.Success = true;
            return response;
        }

        public BaseResponse DeleteCustomers(List<int> ids)
        {
            var response = new BaseResponse();
            try
            {
                var customers = _context.Customers.Where(x => ids.Contains(x.CustomerId)).ToList();

                foreach (var customer in customers)
                {
                    customer.IsDeleted = true;
                    //customer.Name = string.Format("{0}_DELETED_{1}", customer.Name, DateTime.UtcNow.ToString("O"));
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

        public BaseResponse SetExpireTokenForCustomer(CustomerSetExpireTokenRequest request)
        {
            var response = new BaseResponse();

            var customer = _context.Customers.Include(x => x.Company)
                    .FirstOrDefault(x => x.CustomerId == request.CustomerId);

            if (customer == null)
            {
                response.Message = "This customer does not exist";
                return response;
            }

            customer.Token = request.Token;
            customer.ExpireAt = DateTime.UtcNow.AddDays(1);

            _context.SaveChanges();
            response.Success = true;

            return response;
        }

        public IList<string> GetCustomerContactEmails()
        {
            return _context.ContactPersons.Select(x => x.Email).ToList();
        }

        public IList<string> GetCustomerNames()
        {
            return _context.Customers.Where(x => !x.IsDeleted).Select(x => x.Name).ToList();
        }

        public BaseResponse BatchSave(CustomerBatchRequest request)
        {
            var response = new BaseResponse();

            var customersToBeImported = request.ImportCustomers;

            using (var dbContextTransaction = _context.Db.BeginTransaction())
            {
                try
                {
                    foreach (var dataItem in customersToBeImported.Where(x => x.Status == ExportStatus.Valid.ToString()))
                    {
                        var customerSaveRequest = new CustomerSaveRequest
                        {
                            CompanyId = request.CompanyId,
                            Name = dataItem.CustomerName,

                            //Contact
                            ContactFirstName = dataItem.FirstName,
                            ContactMiddleName = dataItem.MiddleName,
                            ContactLastName = dataItem.LastName,
                            ContactEmail = dataItem.Email,
                            ContactMobile = dataItem.Mobile,
                            ContactTelephone = dataItem.Telephone,

                            //Address
                            Address = dataItem.Address,
                            City = dataItem.City,
                            State = dataItem.State,
                            PostalCode = dataItem.PostalCode,
                            Country = dataItem.Country
                        };

                        var customer = new Data.Entities.Customer
                        {
                            Name = customerSaveRequest.Name,
                            CompanyId = request.CompanyId,
                        };
                        AddAddress(customer, customerSaveRequest);
                        AddContactPerson(customer, customerSaveRequest);
                        _context.Customers.Add(customer);
                    }

                    _context.SaveChanges();
                    dbContextTransaction.Commit();
                    response.Success = true;
                }
                catch (Exception exception)
                {
                    dbContextTransaction.Rollback();
                    response.Message = exception.ToString();
                }
            }
            return response;
        }

        public BaseResponse UserCustomerAccess(UserCustomerAccessRequest request)
        {
            var response = new BaseResponse();
            try
            {
                // existing customer access
                var existingCustomerAccess = _context.CompanyWorkerCustomers.Where(x => x.CompanyWorkerId == request.UserId);

                // remove existing access if any
                foreach (var access in existingCustomerAccess)
                {
                    _context.CompanyWorkerCustomers.Remove(access);
                }

                var count = 0;

                if (request.Customers != null)
                {
                    // add new customer access
                    foreach (var customerId in request.Customers)
                    {
                        _context.CompanyWorkerCustomers.Add(new CompanyWorkerCustomer
                        {
                            CompanyWorkerId = request.UserId,
                            CustomerId = customerId,
                            CustomerOrder = count++
                        });
                    }
                }

                // save all changes
                _context.SaveChanges();

                response.Success = true;
            }
            catch (Exception exception)
            {
                response.Message = exception.Message;
            }
            return response;
        }

        public List<CompanyWorkerCustomer> GetUserCustomerAccess(int companyWorkerId)
        {
            var accessList = _context.CompanyWorkerCustomers
                 .Include(x => x.Customer)
                 .Where(x => x.CompanyWorkerId == companyWorkerId)
                 .OrderBy(x => x.CustomerOrder).ToList();

            return accessList;
        }

        public CustomerDetailResponse GetCustomerDetail(int customerId)
        {
            CustomerDetailResponse response = null;
            var customer = _context.Customers
               .Include(x => x.Addresses)
               .Include(x => x.ContactPersons)
               .FirstOrDefault(x => x.CustomerId == customerId);

            if (customer != null)
            {
                var address = customer.Addresses.FirstOrDefault();
                var contactPerson = customer.ContactPersons.FirstOrDefault();

                response = new CustomerDetailResponse
                {
                    CustomerId = customer.CustomerId,
                    CompanyId = customer.CompanyId,
                    Name = customer.Name,
                    GatewayCustomerId = customer.GatewayCustomerId,
                    Address = address != null ? address.StreetAddress : string.Empty,
                    City = address != null ? address.City : string.Empty,
                    State = address != null ? address.Territory : string.Empty,
                    PostalCode = address != null ? address.PostalCode : string.Empty,
                    Country = address != null ? address.Country : string.Empty,
                    IsDeleted = customer.IsDeleted,
                    IsActive = customer.IsCcActive,
                    IsCcActive = customer.IsCcActive,
                    IsCreditCardSetup = customer.CustomerJson != null ? true : false,
                    ContactFirstName = contactPerson != null ? contactPerson.FirstName : string.Empty,
                    ContactLastName = contactPerson != null ? contactPerson.LastName : string.Empty,
                    ContactEmail = contactPerson != null ? contactPerson.Email : string.Empty,
                    ContactTelephone = contactPerson != null ? contactPerson.Telephone : string.Empty,
                    ContactMobile = contactPerson != null ? contactPerson.Mobile : string.Empty,
                    ContactMiddleName = contactPerson != null ? contactPerson.MiddleName : string.Empty,
                    Success = true
                };
            }
            else
            {
                response = new CustomerDetailResponse
                {
                    Message = "Customer does not exist in the database."
                };
            }
            return response;
        }

        #region Synchronization Methods

        public CustomerSynchronizationList CustomerSynchronizationList(int? companyId)
        {

            var synList = new CustomerSynchronizationList();
            var unlinkedSystemCustomers = new List<SystemCustomerModel>();
            var unlinkedQBCustomers = new List<QBCustomerModel>();
            try
            {
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == companyId.Value);
                var dbCustomers = _context.Customers
                    .Include(x => x.Addresses).Where(x => x.CompanyId == companyId.Value)
                    .ToList();
                var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret)
                    .ToBase64Encode();
                var qbCustomers = _qbCustomerServices.GetAllCustomers(authorizationToken);
                if (!qbCustomers.Success)
                {
                    synList.Success = false;
                    synList.Message = qbCustomers.Message;
                    return synList;
                }

                foreach (var qbCustomer in qbCustomers.QueryResponse.Customer.ToList())
                {
                    var linkedCustomer = new LinkedCustomer();
                    var unlinkedCustomer = new UnlinkedCustomer();

                    var CustomerExists = dbCustomers
                        .FirstOrDefault(x => x.QuickbookCustomerId == qbCustomer.Id);
                    if (CustomerExists != null)
                    {
                        var syetmCustomer = GetSystemCustomer(CustomerExists, companyId);
                        var QbCustomer = GetQBCustomer(qbCustomer, companyId.Value, CustomerExists.CustomerId);

                        bool isMatch = IsCustomerMatched(syetmCustomer, QbCustomer);
                        syetmCustomer.IsMatch = isMatch;
                        QbCustomer.IsMatch = isMatch;
                        linkedCustomer.LinkedSystemCustomer = syetmCustomer;
                        linkedCustomer.LinkedQBCustomer = QbCustomer;
                        synList.LinkedCustomers.Add(linkedCustomer);
                    }
                    else
                    {
                        var unlinkedQBCustomer = GetQBCustomer(qbCustomer, companyId.Value, null);
                        synList.UnlinkedQBCustomers.Add(unlinkedQBCustomer);
                    }
                }
                foreach (var dbCustomer in dbCustomers)
                {
                    if (dbCustomer.QuickbookCustomerId == null)
                    {
                        var systemCustomer = GetSystemCustomer(dbCustomer, companyId);
                        synList.UnlinkedSystemCustomers.Add(systemCustomer);
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

        public BaseResponse UpdateSynList(SyncCustomerRequest request)
        {
            var response = new BaseResponse();
            var updateResponse = new BaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                var dbCustomers = _context.Customers.ToList();
                var qbSettings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                #region Unlink Actual Linked Customers

                if (request.UnLinkActualLinkedCustomerIds.Count != 0)
                {
                    response = UnLinkActualLinkedCustomer(dbCustomers, request.UnLinkActualLinkedCustomerIds);
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                }

                #endregion

                #region Linked System Customers
                if (request.LinkedSystemCustomers.Count != 0)
                { 
                    response = LinkedSystemCustomers(dbCustomers, request.LinkedSystemCustomers, request.CustomersEdited);
                    if (!response.Success)
                    {
                        sb.Append("Couldn't save this customer on Quikcbooks " + response.Message + " ");
                    }
                }

                #endregion

                #region Linked Quick books Customers
                if (request.LinkedQBCustomers.Count != 0)
                {
                    response = LinkedQBCustomers(request.LinkedQBCustomers, qbSettings, request);
                    dbCustomers = _context.Customers.ToList();
                    if (!response.Success)
                    {
                        sb.Append(response.Message + " ");
                    }
                    updateResponse = UpdateCustomerForQuickbooks(request.CustomersEdited, qbSettings, request, dbCustomers);
                }

                #endregion

                #region Update Customer on Quikcbooks              

                if (request.CustomersEdited.Count != 0 && request.LinkedQBCustomers.Count == 0)
                {
                    updateResponse = UpdateCustomerOnQB(request.CustomersEdited, qbSettings, request, dbCustomers);
                    var dbCust = _context.Customers.ToList();
                    if (updateResponse.Success)
                        foreach (var customer in request.CustomersEdited)
                        {
                            var dbCustomer = dbCust.FirstOrDefault(x => x.CustomerId == customer.SCCustomerId);

                            var address = dbCustomer.Addresses.FirstOrDefault();
                            var contactPerson = dbCustomer.ContactPersons.FirstOrDefault();

                            address.StreetAddress = customer.SCAddress;
                            address.Territory = customer.SState;
                            address.PostalCode = customer.SCPostalCode;
                            address.City = customer.SCity;

                            contactPerson.Email = customer.SCEmail;
                            contactPerson.Telephone = customer.SCPhone;
                            contactPerson.Mobile = customer.SCPhone;
                            dbCustomer.Name = customer.SCName;
                            _context.SaveChanges();
                        }
                }

                #endregion

                response.Success = true;
                response.Message = "Customers has been synchronized successfully." + sb.ToString();
                if (!qbSettings.CustomerSettings)
                {
                    qbSettings.CustomerSettings = true;
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

        #endregion

        #region Private Method(s)

        private BaseResponse LinkedQBCustomers(List<int> linkedQBCustomers, QuickbookSettings qbSettings, SyncCustomerRequest req)
        {
            var response = new BaseResponse();
            try
            {
                var authorizationTokenHeader = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                var qbCustomers = _qbCustomerServices.GetAllCustomers(authorizationTokenHeader);

                foreach (var qbCustomerId in linkedQBCustomers)
                {
                    var qbCustomer = qbCustomers.QueryResponse.Customer
                        .FirstOrDefault(x => x.Id == qbCustomerId.ToString());
                    var custExists = req.CustomersEdited.FirstOrDefault(x => x.QbCustomerId == qbCustomerId.ToString());
                    if (qbCustomer != null)
                    {
                        var customer = new Data.Entities.Customer();
                        var name = qbCustomer.FamilyName != null ? qbCustomer.GivenName + " " + qbCustomer.FamilyName : qbCustomer.GivenName + " ";
                        customer = new Data.Entities.Customer
                        {
                            Name = custExists != null ? custExists.SCName : name,
                            CompanyId = qbSettings.CompanyId,
                            QuickbookCustomerId = custExists != null ? custExists.QbCustomerId : qbCustomerId.ToString()
                        };
                        var request = new CustomerSaveRequest
                        {
                            Address = custExists != null ? custExists.SCAddress : qbCustomer.BillAddr.Line1,
                            City = custExists != null ? custExists.SCity : qbCustomer.BillAddr.City,
                            State = custExists != null ? custExists.SState : qbCustomer.BillAddr.CountrySubDivisionCode,
                            PostalCode = custExists != null ? custExists.SCPostalCode : qbCustomer.BillAddr.PostalCode,
                            ContactEmail = custExists != null ? custExists.SCEmail : qbCustomer.PrimaryEmailAddr.Address,
                            ContactFirstName = custExists != null ? custExists.SCName : qbCustomer.GivenName,
                            ContactLastName = custExists != null ? custExists.SCName : qbCustomer.FamilyName != null ? qbCustomer.FamilyName : "NA",
                            ContactTelephone = custExists != null ? custExists.SCPhone : qbCustomer.PrimaryPhone.FreeFormNumber,
                            ContactMobile = custExists != null ? custExists.SCPhone : qbCustomer.PrimaryPhone.FreeFormNumber,
                        };

                        AddAddress(customer, request);
                        AddContactPerson(customer, request);
                        _context.Customers.Add(customer);
                        customer.QuickbookCustomerId = qbCustomer.Id;
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

        private BaseResponse LinkedSystemCustomers(List<Data.Entities.Customer> dbCustomers, List<int> custmersToBeLinkedIds, List<SystemCustomerModel> customersEdited)
        {
            var response = new BaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                foreach (var customerId in custmersToBeLinkedIds)
                {
                    var dbCustomer = dbCustomers.FirstOrDefault(x => x.CustomerId == customerId);
                    var checkCustomer = customersEdited.FirstOrDefault(x => x.SCCustomerId == customerId);
                    if (dbCustomer != null)
                    {
                        var address = dbCustomer.Addresses.FirstOrDefault();
                        var contact = dbCustomer.ContactPersons.FirstOrDefault();
                        var index = checkCustomer.SCName.IndexOf(' ');
                        var first = checkCustomer != null ? checkCustomer.SCName.Substring(0, index) : contact.FirstName;
                        var last = checkCustomer != null ? checkCustomer.SCName.Substring(index + 1) : contact.LastName;

                        var request = new CustomerSaveRequest
                        {
                            Address = checkCustomer != null ? checkCustomer.SCAddress : address.StreetAddress,
                            City = checkCustomer != null ? checkCustomer.SCity : address.City,
                            State = checkCustomer != null ? checkCustomer.SState : address.Territory,
                            PostalCode = checkCustomer != null ? checkCustomer.SCPostalCode : address.PostalCode,
                            ContactMobile = checkCustomer != null ? checkCustomer.SCPhone : contact.Mobile,
                            ContactEmail = checkCustomer != null ? checkCustomer.SCEmail : contact.Email,
                            ContactFirstName = first,
                            ContactLastName = last,
                            CompanyId = dbCustomer.CompanyId,
                            Country = checkCustomer != null ? checkCustomer.SCountry : address.Country,
                            Name = checkCustomer != null ? checkCustomer.SCName : dbCustomer.Name
                        };
                        response = SaveCustomerOnQB(request);
                        if (response.Success)
                        {
                            dbCustomer.QuickbookCustomerId = response.QBEntityId;
                            _context.SaveChanges();
                        }
                        else
                        {
                            sb.Append("<b>" + request.ContactFirstName + " " + request.ContactLastName + "</b> " + response.Message + "<br/> ");
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

        private BaseResponse UnLinkActualLinkedCustomer(List<Data.Entities.Customer> dbCustomers, List<int> unlinkedIds)
        {
            var response = new BaseResponse();
            try
            {
                foreach (var customerId in unlinkedIds)
                {
                    var customer = dbCustomers.FirstOrDefault(x => x.CustomerId == customerId);
                    if (customer != null)
                    {
                        customer.QuickbookCustomerId = null;
                        _context.SaveChanges();
                    }
                }
                response.Success = true;
                response.Message = "Customers hasve been unlinked successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse UpdateCustomerOnQB(List<SystemCustomerModel> request, QuickbookSettings qbSettings, SyncCustomerRequest request1, List<Data.Entities.Customer> dbcustomers)
        {
            var response = new BaseResponse();
            var response1 = new GetCustomerResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestCustomer in request)
                {
                    var dbcustomer = dbcustomers.FirstOrDefault(x => x.CustomerId == requestCustomer.SCCustomerId);
                    if (request1.LinkedSystemCustomers != null || request1.LinkedSystemCustomers.Count != 0)
                        if (!request1.LinkedSystemCustomers.Contains(requestCustomer.SCCustomerId.Value))
                        {
                            var customer = new UpdateCustomerRequest();
                            var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                            var qbCustomer = _qbCustomerServices.GetCustomer(requestCustomer.QbCustomerId != null ? requestCustomer.QbCustomerId : requestCustomer.SCCustomerId.ToString(), authorizationToken);
                            customer.sparse = true;
                            customer.BillAddr = new BillAddr
                            {
                                City = requestCustomer.SCity,
                                Line1 = requestCustomer.SCAddress,
                                PostalCode = requestCustomer.SCPostalCode,
                                CountrySubDivisionCode = requestCustomer.SState,
                            };
                            customer.Id = requestCustomer.QbCustomerId;
                            customer.SyncToken = qbCustomer.Customer.SyncToken;
                            customer.PrimaryPhone = new QuickBooks.DTOs.Customers.PrimaryPhone
                            {
                                FreeFormNumber = requestCustomer.SCPhone
                            };
                            customer.GivenName = requestCustomer.SCName;
                            customer.FamilyName = "";
                            customer.PrimaryEmailAddr = new QuickBooks.DTOs.Customers.PrimaryEmailAddr
                            {
                                Address = requestCustomer.SCEmail
                            };
                            var jsonCustomer = JsonConvert.SerializeObject(customer);
                            var customerUpdationResponse = _qbCustomerServices.UpdateCustomer(jsonCustomer, authorizationToken);
                            if (customerUpdationResponse.Success)
                                response1 = XmlUtil.Deserialize<GetCustomerResponse>(customerUpdationResponse.ResponseValue);
                            dbcustomer.QuickbookCustomerId = response1.Customer.Id;
                            _context.SaveChanges();

                            if (!customerUpdationResponse.Success)
                            {
                                response.Success = false;
                                response.Message = customerUpdationResponse.Message;
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

        private BaseResponse UpdateCustomerForQuickbooks(List<SystemCustomerModel> request, QuickbookSettings qbSettings, SyncCustomerRequest request1, List<Data.Entities.Customer> dbcustomers)
        {
            var response = new BaseResponse();
            var response1 = new GetCustomerResponse();
            try
            {
                if (qbSettings == null) return response;

                //if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                //{
                foreach (var requestCustomer in request)
                {
                    var dbcustomer = dbcustomers.FirstOrDefault(x => x.QuickbookCustomerId == requestCustomer.QbCustomerId);

                    var customer = new UpdateCustomerRequest();
                    var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                    var qbCustomer = _qbCustomerServices.GetCustomer(requestCustomer.QbCustomerId != null ? requestCustomer.QbCustomerId : requestCustomer.SCCustomerId.ToString(), authorizationToken);

                    customer.sparse = true;
                    customer.BillAddr = new BillAddr
                    {
                        City = requestCustomer.SCity,
                        Line1 = requestCustomer.SCAddress,
                        PostalCode = requestCustomer.SCPostalCode,
                        CountrySubDivisionCode = requestCustomer.SState,
                    };
                    customer.Id = requestCustomer.QbCustomerId;
                    customer.SyncToken = qbCustomer.Customer.SyncToken;
                    customer.PrimaryPhone = new QuickBooks.DTOs.Customers.PrimaryPhone
                    {
                        FreeFormNumber = requestCustomer.SCPhone
                    };
                    customer.GivenName = requestCustomer.SCName;
                    customer.FamilyName = "";
                    customer.PrimaryEmailAddr = new QuickBooks.DTOs.Customers.PrimaryEmailAddr
                    {
                        Address = requestCustomer.SCEmail
                    };
                    var jsonCustomer = JsonConvert.SerializeObject(customer);
                    var customerUpdationResponse = _qbCustomerServices.UpdateCustomer(jsonCustomer, authorizationToken);
                    if (customerUpdationResponse.Success)
                        response1 = XmlUtil.Deserialize<GetCustomerResponse>(customerUpdationResponse.ResponseValue);
                    dbcustomer.QuickbookCustomerId = response1.Customer.Id;
                    _context.SaveChanges();

                    if (!customerUpdationResponse.Success)
                    {
                        response.Success = false;
                        response.Message = customerUpdationResponse.Message;
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

        private BaseResponse SaveCustomerOnQB(CustomerSaveRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var qbSettings = _context.QuickbookSettings
                    .FirstOrDefault(x => x.CompanyId == request.CompanyId);
                if (qbSettings == null) return response;

                if (qbSettings.IsQuickbooksIntegrated && qbSettings.CustomerSettings)
                {
                    var custRequest = new CreateCustomerRequest();
                    custRequest.BillAddr = new BillAddr
                    {
                        City = request.City,
                        Country = "USA",
                        CountrySubDivisionCode = request.State,
                        Line1 = request.Address,
                        PostalCode = request.PostalCode
                    };
                    custRequest.PrimaryPhone = new QuickBooks.DTOs.Customers.PrimaryPhone
                    {
                        FreeFormNumber = request.ContactMobile
                    };
                    custRequest.PrimaryEmailAddr = new QuickBooks.DTOs.Customers.PrimaryEmailAddr
                    {
                        Address = request.ContactEmail
                    };
                    custRequest.GivenName = request.ContactFirstName;
                    custRequest.FamilyName = request.ContactLastName;
                    custRequest.FullyQualifiedName = "Supreme Technologies";
                    custRequest.DisplayName = request.ContactFirstName + " " + request.ContactLastName;
                    var jsonCustomer = JsonConvert.SerializeObject(custRequest);
                    var authorizationToken = (qbSettings.ClientId + ":" + qbSettings.ClientSecret).ToBase64Encode();
                    var customerCreationResponse = _qbCustomerServices.CreateCustomer(jsonCustomer, authorizationToken);

                    if (!customerCreationResponse.Success)
                    {
                        response.Success = false;
                        response.Message = customerCreationResponse.Message;
                        return response;
                    }
                    var qbCustomer = XmlUtil.Deserialize<GetCustomerResponse>(customerCreationResponse.ResponseValue);
                    response.Success = customerCreationResponse.Success;
                    response.Message = customerCreationResponse.Message;
                    response.QBEntityId = qbCustomer.Customer.Id;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could't process the request, Please make sure that customer settings under Quikcbooks Settings Tab in ON.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }

            return response;
        }

        private bool IsCustomerMatched(SystemCustomerModel systemCustomer, QBCustomerModel qbCustomerModel)
        {
            var qbName = qbCustomerModel.SCName.TrimEnd();
            var syName = systemCustomer.SCName.TrimEnd();
            bool isMatch =
                 systemCustomer.SCAddress == qbCustomerModel.SCAddress &&
                 systemCustomer.SCEmail == qbCustomerModel.SCEmail &&
                 systemCustomer.SCPhone == qbCustomerModel.SCPhone &&
                 systemCustomer.SState == qbCustomerModel.SState &&
                 systemCustomer.SCPostalCode == qbCustomerModel.SCPostalCode &&
                syName == qbName;

            return isMatch;
        }

        private SystemCustomerModel GetSystemCustomer(Data.Entities.Customer dbCustomer, int? companyId)
        {
            var defaultAddress = dbCustomer.Addresses.FirstOrDefault();
            var defaultContact = dbCustomer.ContactPersons.FirstOrDefault();
            return new SystemCustomerModel
            {
                SCAddress = defaultAddress != null ? defaultAddress.StreetAddress : "",
                SCity = defaultAddress != null ? defaultAddress.City : "",
                SCountry = defaultAddress != null ? defaultAddress.Country : "",
                SCEmail = defaultContact != null ? defaultContact.Email : "",
                IsActive = dbCustomer.IsDeleted,
                SCName = dbCustomer.Name != null ? dbCustomer.Name : "",
                SCPhone = defaultContact != null ? defaultContact.Mobile : "",
                SCPostalCode = defaultAddress != null ? defaultAddress.PostalCode : "",
                SState = defaultAddress != null ? defaultAddress.Territory : "",
                QbCustomerId = dbCustomer.QuickbookCustomerId,
                CompanyId = companyId.Value,
                SCCustomerId = dbCustomer.CustomerId,
            };
        }

        private QBCustomerModel GetQBCustomer(CustomerResponse qbCustomer, int? companyId, int? customerId)
        {
            return new QBCustomerModel
            {
                SCAddress = qbCustomer.BillAddr != null ? qbCustomer.BillAddr.Line1 : "",
                SCountry = qbCustomer.BillAddr != null ? qbCustomer.BillAddr.Country : "",
                SCEmail = qbCustomer.PrimaryEmailAddr != null ? qbCustomer.PrimaryEmailAddr.Address : "",
                IsActive = qbCustomer.Active == "true" ? true : false,
                SCName = qbCustomer.GivenName + " " + qbCustomer.FamilyName,
                SCPhone = qbCustomer.PrimaryPhone != null ? qbCustomer.PrimaryPhone.FreeFormNumber : "",
                SCPostalCode = qbCustomer.BillAddr != null ? qbCustomer.BillAddr.PostalCode : "",
                SState = qbCustomer.BillAddr != null ? qbCustomer.BillAddr.CountrySubDivisionCode : "",
                CompanyId = companyId.Value,
                QbCustomerId = qbCustomer.Id,
                SCCustomerId = customerId != null ? customerId.Value : customerId
            };
        }

        public void AddAddress(Data.Entities.Customer customer, CustomerSaveRequest request)
        {
            customer.Addresses = new Collection<Address>
            {
                new Address
                {
                    StreetAddress = request.Address,
                    City = request.City,
                    Territory = request.State,
                    Country = request.Country,
                    PostalCode = request.PostalCode
                }
            };
        }

        public void AddContactPerson(Data.Entities.Customer customer, CustomerSaveRequest request)
        {
            customer.ContactPersons = new Collection<ContactPerson>
            {
                new ContactPerson
                {
                    Email = request.ContactEmail,
                    FirstName = request.ContactFirstName,
                    LastName = request.ContactLastName,
                    Telephone = request.ContactTelephone,
                    Mobile = request.ContactMobile,
                    MiddleName = request.ContactMiddleName,
                    ContactType = ContactTypes.Office.ToString() //TODO : 
                }
            };
        }

        #endregion
    }
}